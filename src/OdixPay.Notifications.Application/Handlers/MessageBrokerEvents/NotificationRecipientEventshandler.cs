using MediatR;
using Microsoft.Extensions.Logging;
using OdixPay.Notifications.Application.Commands;
using OdixPay.Notifications.Domain.DTO.Events;
using OdixPay.Notifications.Domain.Interfaces;

namespace OdixPay.Notifications.Application.Handlers.MessageBrokerEvents
{
    public class NotificationRecipientsEventHandler(ILogger<NotificationRecipientsEventHandler> logger, IMediator mediator) : INotificationRecipientsEventHandler
    {
        private readonly ILogger<NotificationRecipientsEventHandler> _logger = logger;
        private readonly IMediator _mediator = mediator;

        public async Task<bool> CreateAsync(UserDataChangedEvent eventData)
        {
            await _mediator.Send(new CreateNotificationRecipientCommand()
            {
                Name = eventData.Name,
                Type = eventData.Type,
                UserId = eventData.UserId,
                Value = eventData.Value
            });

            return true;
        }
    }
}