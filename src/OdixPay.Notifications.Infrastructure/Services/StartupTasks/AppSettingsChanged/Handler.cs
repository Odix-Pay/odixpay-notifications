using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OdixPay.Notifications.Domain.Interfaces;
using OdixPay.Notifications.Domain.Models;
using OdixPay.Notifications.Infrastructure.Constants;

namespace OdixPay.Notifications.Infrastructure.Services.StartupTasks.AppSettingsChanged;

public class AppSettingsChangedEventsHandler(IEventBusService eventBusService, IServiceProvider serviceProvider) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        eventBusService.SubscribeAsync<MessageEnvelope<AppSettingsChangedEvent>, bool>(EventTopics.NotificationEvents.Subscriptions.AppSettingsChanged, HandleUserSettingsChangedAsync);

        return Task.CompletedTask;
    }

    private async Task<bool> HandleUserSettingsChangedAsync(MessageEnvelope<AppSettingsChangedEvent> eventData)
    {

        var recipientRepo = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<INotificationRecipientRepository>() ?? throw new InvalidOperationException();

        var userId = eventData.Data?.UserId;
        var locale = eventData.Data?.LanguageISO2.ToLowerInvariant() ?? "en";

        if (userId == null)
        {
            return false;
        }

        await recipientRepo.UpdateRecipientLanguageAsync(userId, locale);

        return true;
    }
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}