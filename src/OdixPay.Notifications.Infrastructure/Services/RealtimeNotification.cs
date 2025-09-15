using Microsoft.AspNetCore.SignalR;
using OdixPay.Notifications.Domain.Interfaces;
using OdixPay.Notifications.Contracts.Hubs;

namespace OdixPay.Notifications.Infrastructure.Services;

public class RealtimeNotificationService(IHubContext<NotificationsHub> hubContext) : IRealtimeNotifier
{
    private readonly IHubContext<NotificationsHub> _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));

    public async Task SendToGroupAsync(string group, string eventName, object payload, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(group))
            throw new ArgumentException("Group name cannot be null or empty.", nameof(group));

        if (string.IsNullOrWhiteSpace(eventName))
            throw new ArgumentException("Event name cannot be null or empty.", nameof(eventName));

        ct.ThrowIfCancellationRequested();

        await _hubContext.Clients.Group(group).SendAsync(eventName, payload, ct);
    }

    public async Task SendToAllAsync(string eventName, object payload, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(eventName))
            throw new ArgumentException("Event name cannot be null or empty.", nameof(eventName));

        ct.ThrowIfCancellationRequested();

        await _hubContext.Clients.All.SendAsync(eventName, payload, ct);
    }
}