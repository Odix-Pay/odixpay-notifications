using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OdixPay.Notifications.Contracts.Constants;
using OdixPay.Notifications.Domain.Enums;
using OdixPay.Notifications.Domain.Interfaces;
using OdixPay.Notifications.Domain.Models;
using OdixPay.Notifications.Infrastructure.Constants;

namespace OdixPay.Notifications.Infrastructure.Services.StartupTasks.RealtimeNotifications;


public class RealtimeNotificationsHandler(IEventBusService eventBusService, IServiceProvider serviceProvider, ILogger<RealtimeNotificationsHandler> logger) : IHostedService
{
    private readonly IEventBusService _eventBusService = eventBusService ?? throw new ArgumentNullException(nameof(eventBusService));
    private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    private readonly ILogger<RealtimeNotificationsHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _eventBusService.SubscribeAsync<MessageEnvelope<RealtimeNotificationsDto>, bool>(EventTopics.NotificationEvents.Subscriptions.RealTimeNotification, HandleUserSettingsChangedAsync);

        return Task.CompletedTask;
    }

    private async Task<bool> HandleUserSettingsChangedAsync(MessageEnvelope<RealtimeNotificationsDto> eventData)
    {

        var provider = _serviceProvider.CreateScope().ServiceProvider;

        var notifier = provider.GetRequiredService<IRealtimeNotifier>() ?? throw new InvalidOperationException();
        var notificationsHandler = provider.GetRequiredService<INotificationCommandHandler>() ?? throw new InvalidOperationException();

        if (eventData?.Data == null)
        {
            return false;
        }

        var data = eventData.Data;

        _logger.LogInformation("Processing real-time notification event for user {UserId} with message {Topic}", data.Title, data.Message);

        // create a notification for admin users
        var created = await notificationsHandler.HandleCreateNotificationAsync(new()
        {
            UserId = "admin-notifications",
            Type = NotificationType.InApp,
            Title = data.Title ?? "Admin Notification",
            Message = data.Message,
            Recipient = "admin-notifications",
            Priority = NotificationPriority.High,
            Data = null,
            MaxRetries = 0,
        });

        if (created == null)
        {
            return false;
        }

        // send real-time notification to admin group
        await notifier.SendToGroupAsync(HubPrefixes.GetGroup(HubPrefixes.Admins, [HubPrefixes.Notifications]), eventData.Data.Topic ?? RealtimeEventNames.AdminNotificationReceived, created);

        return true;
    }
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}