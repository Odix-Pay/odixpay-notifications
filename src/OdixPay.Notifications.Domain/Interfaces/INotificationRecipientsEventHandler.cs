using OdixPay.Notifications.Domain.DTO.Events;

namespace OdixPay.Notifications.Domain.Interfaces;



public interface INotificationRecipientsEventHandler
{
    Task<bool> CreateAsync(UserDataChangedEvent eventData);
}