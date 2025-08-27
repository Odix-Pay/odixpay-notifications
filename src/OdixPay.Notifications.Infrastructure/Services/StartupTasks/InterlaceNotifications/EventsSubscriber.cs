using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OdixPay.Notifications.Domain.Interfaces;
using OdixPay.Notifications.Domain.Models;
using OdixPay.Notifications.Infrastructure.Constants;

namespace OdixPay.Notifications.Infrastructure.Services.StartupTasks.InterlaceNotifications;

public class InterlaceNotificationsSubscriber(IEventBusService eventBus, IServiceProvider serviceProvider) : IHostedService
{

    private readonly IEventBusService _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
    private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Card Status changed subscriber
        await _eventBus.SubscribeAsync<MessageEnvelope<List<CardStatusChangedEvent>>, object>(
            EventTopics.NotificationEvents.Subscriptions.InterlaceCardStatusChanged, async (eventData) => await HandleCardStatusChangedAsync(eventData));

        // Cardholder KYC Status changed subscriber
        await _eventBus.SubscribeAsync<MessageEnvelope<List<CardholderStatusChangedEvent>>, object>(
            EventTopics.NotificationEvents.Subscriptions.InterlaceCardholderStatusChanged,
            async (eventData) => await HandleCardholderStatusChangedAsync(eventData));

        // Transaction Status changed subscriber
        await _eventBus.SubscribeAsync<MessageEnvelope<List<CardTransactionStatusChangedEvent>>, object>(
            EventTopics.NotificationEvents.Subscriptions.InterlaceTransactionStatusChanged,
            async (eventData) => await HandleTransactionStatusChangedAsync(eventData));
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    // Handlers
    private async Task<bool> HandleCardStatusChangedAsync(MessageEnvelope<List<CardStatusChangedEvent>> evt)
    {
        using var scope = _serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<INotificationCommandHandler>() ?? throw new InvalidOperationException("NotificationCommandHandler is not registered.");

        var eventDataList = evt.Data;

        if (eventDataList == null || eventDataList.Count == 0)
        {
            throw new ArgumentNullException(nameof(evt), "Event data or its payload cannot be null or empty.");
        }

        foreach (var data in eventDataList)
        {
            if (data == null)
            {
                continue;
            }

            var response = await handler.HandleCreateNotificationAsync(new()
            {
                Data = data.Data,
                MaxRetries = data.MaxRetries,
                Priority = data.Priority,
                Recipient = data.Recipient,
                Sender = data.Sender,
                Message = data.Message,
                ScheduledAt = data.ScheduledAt ?? DateTime.UtcNow,
                TemplateId = data.TemplateId,
                TemplateSlug = data.TemplateSlug,
                TemplateVariables = data.TemplateVariables != null ? new Dictionary<string, string>()
                {
                    { "firstName", data.TemplateVariables.FirstName },
                    { "lastName", data.TemplateVariables.LastName },
                    { "title", data.TemplateVariables.Title },
                    { "message", data.TemplateVariables.Message }
                } : null,
                Title = data.Title,
                Type = data.Type,
                UserId = data.UserId
            });
        }


        return true;
    }

    private async Task<bool> HandleCardholderStatusChangedAsync(MessageEnvelope<List<CardholderStatusChangedEvent>> evt)
    {
        using var scope = _serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<INotificationCommandHandler>() ??
            throw new InvalidOperationException("NotificationCommandHandler is not registered.");

        var eventDataList = evt.Data;

        if (eventDataList == null || eventDataList.Count == 0)
        {
            throw new ArgumentNullException(nameof(evt), "Event data or its payload cannot be null or empty.");
        }

        foreach (var data in eventDataList)
        {

            if (data == null)
            {
                continue;
            }

            var response = await handler.HandleCreateNotificationAsync(new()
            {
                Data = data.Data,
                MaxRetries = data.MaxRetries,
                Priority = data.Priority,
                Recipient = data.Recipient,
                Sender = data.Sender,
                Message = data.Message,
                ScheduledAt = data.ScheduledAt ?? DateTime.UtcNow,
                TemplateId = data.TemplateId,
                TemplateSlug = data.TemplateSlug,
                TemplateVariables = data.TemplateVariables != null ? new Dictionary<string, string>()
                {
                    { "firstName", data.TemplateVariables.FirstName },
                    { "lastName", data.TemplateVariables.LastName },
                    { "title", data.TemplateVariables.Title },
                    { "message", data.TemplateVariables.Message }
                } : null,
                Title = data.Title,
                Type = data.Type,
                UserId = data.UserId
            });
        }


        return true;
    }

    private async Task<object?> HandleTransactionStatusChangedAsync(MessageEnvelope<List<CardTransactionStatusChangedEvent>> evt, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<INotificationCommandHandler>() ??
            throw new InvalidOperationException("NotificationCommandHandler is not registered.");

        var eventDataList = evt.Data;

        System.Console.WriteLine("Event Data:.....................................\n");

        System.Console.WriteLine(JsonSerializer.Serialize(evt));

        System.Console.WriteLine("Event Data:.....................................End\n");

        if (eventDataList == null || eventDataList.Count == 0)
        {
            throw new ArgumentNullException(nameof(evt), "Event data or its payload cannot be null or empty.");
        }

        foreach (var data in eventDataList)
        {
            if (data == null)
            {
                continue;
            }

            var response = await handler.HandleCreateNotificationAsync(new()
            {
                Data = data.Data,
                MaxRetries = data.MaxRetries,
                Priority = data.Priority,
                Recipient = data.Recipient,
                Sender = data.Sender,
                Message = data.Message,
                ScheduledAt = data.ScheduledAt ?? DateTime.UtcNow,
                TemplateId = data.TemplateId,
                TemplateSlug = data.TemplateSlug,
                TemplateVariables = data.TemplateVariables != null
                 ? new Dictionary<string, string>()
                {
                    { "firstName", data.TemplateVariables.FirstName },
                    { "lastName", data.TemplateVariables.LastName },
                    { "message", data.TemplateVariables.Message },
                    { "title", data.TemplateVariables.Title },

                } : null,
                Title = data.Title,
                Type = data.Type,
                UserId = data.UserId
            });
        }


        return true;
    }

}
