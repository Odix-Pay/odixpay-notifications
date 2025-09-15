namespace OdixPay.Notifications.Domain.Interfaces;

public interface IRealtimeNotifier
{
    Task SendToGroupAsync(string group, string eventName, object payload, CancellationToken ct = default);
    Task SendToAllAsync(string eventName, object payload, CancellationToken ct = default);
}
