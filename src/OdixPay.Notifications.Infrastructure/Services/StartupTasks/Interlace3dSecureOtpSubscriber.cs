using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OdixPay.Notifications.Domain.DTO.Requests;
using OdixPay.Notifications.Domain.Enums;
using OdixPay.Notifications.Domain.Interfaces;
using OdixPay.Notifications.Domain.Models;
using OdixPay.Notifications.Infrastructure.Constants;

namespace OdixPay.Notifications.Infrastructure.Services.StartupTasks;

public class Interlace3dSecureOtpSubscriber(IEventBusService eventBusService, IServiceProvider serviceProvider) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await eventBusService.SubscribeAsync<MessageEnvelope<Interlace3dDomainForwardingEvent>, object>(EventTopics.NotificationEvents.Subscriptions.Card3dForwardingOtp, async (eventData) =>
        {
            using var scope = serviceProvider.CreateScope();

            var handler = scope.ServiceProvider.GetRequiredService<INotificationCommandHandler>() ?? throw new InvalidOperationException("NotificationCommandHandler is not registered.");

            if (eventData == null || eventData.Data == null)
            {
                throw new ArgumentNullException(nameof(eventData), "Event data or its payload cannot be null.");
            }

            var request = new CreateNotificationRequest
            {
                Message = $"Your OTP is {eventData.Data.Url }",
                Title = "Interlace 3D Secure OTP",
                MaxRetries = 3,

                Priority = NotificationPriority.Critical,
                ScheduledAt = DateTime.UtcNow,
            };

            if (!string.IsNullOrEmpty(eventData.Data.PhoneCountryCode) && !string.IsNullOrEmpty(eventData.Data.PhoneNumber))
            {

                request.Recipient = $"+{eventData.Data.PhoneCountryCode}{eventData.Data.PhoneNumber}".Replace("++", "+").Trim();

                request.Type = NotificationType.SMS;

                await handler.HandleCreateNotificationAsync(request);
            }

            if (!string.IsNullOrEmpty(eventData.Data.Email))
            {
                request.Recipient = eventData.Data.Email;
                request.Type = NotificationType.Email;

                await handler.HandleCreateNotificationAsync(request);
            }

            return null;
        });
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}