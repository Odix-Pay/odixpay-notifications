using AutoMapper;
using MediatR;
using OdixPay.Notifications.Application.Queries;
using OdixPay.Notifications.Domain.DTO.Requests;
using OdixPay.Notifications.Domain.Interfaces;

namespace OdixPay.Notifications.Application.Handlers;

public class GetNotificationByIdHandler(INotificationRepository notificationRepository, IMapper mapper) : IRequestHandler<GetNotificationByIdQuery, NotificationResponse?>
{
    private readonly INotificationRepository _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

    public async Task<NotificationResponse?> Handle(GetNotificationByIdQuery request, CancellationToken cancellationToken)
    {
        var notification = await _notificationRepository.GetNotificationByIdAsync(request.Id, cancellationToken);
        if (notification == null)
            return null;

        return _mapper.Map<NotificationResponse>(notification);
    }
}

public class GetNotificationsByUserIdHandler(INotificationRepository notificationRepository, IMapper mapper) : IRequestHandler<GetNotificationsQuery, NotificationListResponse>
{
    private readonly INotificationRepository _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

    public async Task<NotificationListResponse> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        var Page = request.Page.GetValueOrDefault(1);
        var Limit = request.Limit.GetValueOrDefault(20);
        request.Page = Page;
        request.Limit = Limit;

        cancellationToken.ThrowIfCancellationRequested();

        var count = await _notificationRepository.GetNotificationsCountAsync(request, cancellationToken);

        var notifications = await _notificationRepository.GetNotificationsAsync(request, cancellationToken);

        var notificationResponses = notifications.Select(n => _mapper.Map<NotificationResponse>(n));

        return new NotificationListResponse
        {
            Data = notificationResponses,
            Limit = Limit,
            Page = Page,
            Total = count
        };
    }

}

public class GetUnreadCountHandler(INotificationRepository notificationRepository) : IRequestHandler<GetUnreadCountQuery, int>
{
    private readonly INotificationRepository _notificationRepository = notificationRepository;

    public async Task<int> Handle(GetUnreadCountQuery request, CancellationToken cancellationToken)
    {
        return await _notificationRepository.GetUnreadCountAsync(request.UserId, cancellationToken);
    }
}


