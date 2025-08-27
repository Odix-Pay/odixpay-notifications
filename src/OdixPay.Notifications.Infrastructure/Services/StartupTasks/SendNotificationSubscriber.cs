using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OdixPay.Notifications.Domain.DTO.Requests;
using OdixPay.Notifications.Domain.Interfaces;
using OdixPay.Notifications.Domain.Models;
using OdixPay.Notifications.Infrastructure.Constants;

namespace OdixPay.Notifications.Infrastructure.Services.StartupTasks;

public class SendNotificationSubscriber(IEventBusService eventBusService, IServiceProvider serviceProvider) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await eventBusService.SubscribeAsync<MessageEnvelope<CreateNotificationRequest>, object>(EventTopics.NotificationEvents.CreateNotification, async (eventData) =>
        {
            using var scope = serviceProvider.CreateScope();

            var handler = scope.ServiceProvider.GetRequiredService<INotificationCommandHandler>() ?? throw new InvalidOperationException("NotificationCommandHandler is not registered.");

            if (eventData == null || eventData.Data == null)
            {
                throw new ArgumentNullException(nameof(eventData), "Event data or its payload cannot be null.");
            }


            var response = await handler.HandleCreateNotificationAsync(eventData.Data);

            return null;


        });

        await eventBusService.SubscribeAsync<MessageEnvelope<List<CreateNotificationRequest>>, object>(EventTopics.NotificationEvents.CreateNotificationMany, async (eventData) =>
        {
            using var scope = serviceProvider.CreateScope();

            var handler = scope.ServiceProvider.GetRequiredService<INotificationCommandHandler>() ?? throw new InvalidOperationException("NotificationCommandHandler is not registered.");

            if (eventData == null || eventData.Data == null)
            {
                throw new ArgumentNullException(nameof(eventData), "Event data or its payload cannot be null.");
            }

            var tasks = eventData.Data.Select(request => handler.HandleCreateNotificationAsync(request)).ToList();

            await Task.WhenAll(tasks);

            return null;


        });
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}