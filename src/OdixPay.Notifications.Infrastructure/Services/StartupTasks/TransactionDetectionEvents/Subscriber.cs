using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OdixPay.Notifications.Domain.Enums;
using OdixPay.Notifications.Domain.Interfaces;
using OdixPay.Notifications.Domain.Models;
using OdixPay.Notifications.Infrastructure.Constants;

namespace OdixPay.Notifications.Infrastructure.Services.StartupTasks.TransactionDetectionEvents;

public class SubscribeToScanningEvents(IEventBusService eventBusService, IServiceProvider serviceProvider) : IHostedService
{

    private readonly IEventBusService _eventBusService = eventBusService;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    private async Task<object> HandleEvents(MessageEnvelope<BlockchainTransactionObserved> subscriber)
    {
        var scope = _serviceProvider.CreateScope();
        var recipientsRepo = scope.ServiceProvider.GetRequiredService<INotificationRecipientRepository>() ?? throw new InvalidOperationException();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationCommandHandler>() ?? throw new InvalidOperationException();

        var data = subscriber.Data;
        // Handle the event (e.g., log it, process it, etc.)
        Console.WriteLine($"Received transaction: {data.TransactionHash} on network {data.Network}");

        if (data is null)
        {
            return null;
        }

        var toRecipient = await recipientsRepo.GetByUserIdAndTypeAsync(data.ToAddress, NotificationType.Push, CancellationToken.None);

        var fromRecipient = await recipientsRepo.GetByUserIdAndTypeAsync(data.FromAddress, NotificationType.Push, CancellationToken.None);

        if (toRecipient == null && fromRecipient == null)
        {
            toRecipient = new()
            {
                DefaultLanguage = "en",
                Type = NotificationType.Push,
                UserId = data.ToAddress,
                Recipient = "cJvGIdwSIEtFpnusfJC-HA:APA91bGpwr2yvzfNVUaOsLs8gu0buh-VTIL7g_V1dHN3LCIWTWpqLV0_5XnZ5K0Ygamakb21hNOEeeFdCoGJOdn4IHg6_vmSjSewD6ELfKPaRqWSdLNKbmg",
                Name = "Unknown User",
            };
            Console.WriteLine($"No push notification recipient found for ToAddress: {data.ToAddress} or FromAddress: {data.FromAddress}");
            // return null;
        }

        var direction = "incoming" ?? data.Direction?.ToLower();
        var isIncoming = direction == "inbound" || direction == "incoming";
        var isOutgoing = direction == "outbound" || direction == "outgoing";

        if (isIncoming && toRecipient != null)
        {
            await notificationService.HandleCreateNotificationAsync(new()
            {
                UserId = toRecipient.UserId,
                Type = NotificationType.Push,
                Title = "Incoming Transaction Detected",
                Message = $"You have received {data.Amount} {data.Currency} on {data.Network}.",
                Recipient = toRecipient.Recipient,
                Priority = NotificationPriority.High,
                Data = new Dictionary<string, object>
                {
                    { "transactionHash", data.TransactionHash },
                    { "network", data.Network },
                    { "amount", data.Amount.ToString() },
                    { "fromAddress", data.FromAddress },
                    { "toAddress", data.ToAddress },
                    { "blockNumber", data.BlockNumber.ToString() },
                    { "timestamp", data.Timestamp.ToString("o") }
                },
                MaxRetries = 3,
                ScheduledAt = DateTime.UtcNow,
            });
        }
        else if (isOutgoing && fromRecipient != null)
        {
            await notificationService.HandleCreateNotificationAsync(new()
            {
                UserId = fromRecipient.UserId,
                Type = NotificationType.Push,
                Title = "Outgoing Transaction Detected",
                Message = $"You have sent {data.Amount} {data.Currency} on {data.Network}.",
                Recipient = fromRecipient.UserId,
                Priority = NotificationPriority.High,
                Data = new Dictionary<string, object>
                {
                    { "transactionHash", data.TransactionHash },
                    { "network", data.Network },
                    { "amount", data.Amount.ToString() },
                    { "fromAddress", data.FromAddress },
                    { "toAddress", data.ToAddress },
                    { "blockNumber", data.BlockNumber.ToString() },
                    { "timestamp", data.Timestamp.ToString("o") }
                },
                MaxRetries = 3,
                ScheduledAt = DateTime.UtcNow,
            });
        }
        else
        {
            Console.WriteLine($"No valid direction found or recipient missing for transaction: {data.TransactionHash}");
        }

        return null;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _eventBusService.SubscribeAsync<MessageEnvelope<BlockchainTransactionObserved>, object>(EventTopics.NotificationEvents.Subscriptions.Scanner.ObservedTx, HandleEvents);

    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

