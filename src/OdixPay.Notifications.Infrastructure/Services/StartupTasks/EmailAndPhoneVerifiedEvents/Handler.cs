using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OdixPay.Notifications.Domain.Enums;
using OdixPay.Notifications.Domain.Interfaces;
using OdixPay.Notifications.Domain.Models;
using OdixPay.Notifications.Infrastructure.Constants;

namespace OdixPay.Notifications.Infrastructure.Services.StartupTasks.EmailAndPhoneVerifiedEvents;

public class EmailAndPhoneVerifiedEvents(IEventBusService eventBusService, IServiceProvider serviceProvider) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        eventBusService.SubscribeAsync<MessageEnvelope<UserDataChangedEvent>, bool>(EventTopics.NotificationEvents.Subscriptions.EmailVerified, HandleEmailChangedAsync);

        eventBusService.SubscribeAsync<MessageEnvelope<UserDataChangedEvent>, bool>(EventTopics.NotificationEvents.Subscriptions.EmailUpdated, HandleEmailChangedAsync);

        eventBusService.SubscribeAsync<MessageEnvelope<UserDataChangedEvent>, bool>(EventTopics.NotificationEvents.Subscriptions.PhoneVerified, HandlePhoneChangedAsync);

        eventBusService.SubscribeAsync<MessageEnvelope<UserDataChangedEvent>, bool>(EventTopics.NotificationEvents.Subscriptions.PhoneUpdated, HandlePhoneChangedAsync);

        return Task.CompletedTask;
    }

    private async Task<bool> HandleEmailChangedAsync(MessageEnvelope<UserDataChangedEvent> eventData)
    {
        var email = eventData.Data?.EmailAddress;

        if (!string.IsNullOrEmpty(email) && IsValidEmail(email) && !string.IsNullOrEmpty(eventData.Data?.UserId))
        {
            var scope = serviceProvider.CreateScope();

            var service = scope.ServiceProvider.GetRequiredService<INotificationRecipientsEventHandler>() ?? throw new InvalidOperationException();

            await service.CreateAsync(new()
            {
                Name = eventData.Data.UserName,
                Type = NotificationType.Email,
                UserId = eventData.Data.UserId,
                Value = eventData.Data.EmailAddress
            });

        }

        return true;
    }

    private async Task<bool> HandlePhoneChangedAsync(MessageEnvelope<UserDataChangedEvent> eventData)
    {
        var phoneNumber = eventData.Data?.PhoneNumber;

        if (!string.IsNullOrEmpty(phoneNumber) && IsValidPhoneNumber(phoneNumber) && !string.IsNullOrEmpty(eventData.Data?.UserId))
        {
            var scope = serviceProvider.CreateScope();

            var service = scope.ServiceProvider.GetRequiredService<INotificationRecipientsEventHandler>() ?? throw new InvalidOperationException();

            await service.CreateAsync(new()
            {
                Name = eventData.Data.UserName,
                Type = NotificationType.SMS,
                UserId = eventData.Data.UserId,
                Value = phoneNumber
            });

        }

        return true;
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private bool IsValidPhoneNumber(string phoneNumber)
    {
        var isValidPhoneNumber = false;
        try
        {
            var phoneNumberUtil = PhoneNumbers.PhoneNumberUtil.GetInstance();
            var parsedNumber = phoneNumberUtil.Parse(phoneNumber, null);

            if (!phoneNumberUtil.IsValidNumber(parsedNumber))
            {
                isValidPhoneNumber = false;
            }
        }
        catch (PhoneNumbers.NumberParseException)
        {
            isValidPhoneNumber = false;
        }

        return isValidPhoneNumber;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}