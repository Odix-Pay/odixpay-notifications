using System.Data;
using Dapper;
using OdixPay.Notifications.Domain.DTO.Requests;
using OdixPay.Notifications.Domain.Entities;
using OdixPay.Notifications.Domain.Enums;
using OdixPay.Notifications.Domain.Interfaces;
using OdixPay.Notifications.Infrastructure.Constants;
using OdixPay.Notifications.Infrastructure.Data;

namespace OdixPay.Notifications.Infrastructure.Repositories;

public class NotificationRepository(IConnectionFactory connectionFactory) : INotificationRepository
{
    private readonly IConnectionFactory _connectionFactory = connectionFactory;

    public async Task<Notification> CreateNotificationAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new
        {
            notification.Id,
            notification.UserId,
            notification.Type,
            notification.Title,
            notification.Message,
            notification.Data,
            notification.Priority,
            notification.Recipient,
            notification.CreatedAt,
            notification.Sender,
            notification.ScheduledAt,
            notification.MaxRetries,
            notification.TemplateId,
            notification.TemplateVariables,
        };

        cancellationToken.ThrowIfCancellationRequested();

        var created = await connection.QueryFirstOrDefaultAsync<Notification>(
            StoredProcedures.Notification.Create,
            parameters,
            commandType: CommandType.StoredProcedure);
        return created!;
    }

    public async Task<Notification?> GetNotificationByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        var result = await connection.QueryFirstOrDefaultAsync<Notification>(
            StoredProcedures.Notification.GetById, new { Id = id }, commandType: CommandType.StoredProcedure);

        return result;
    }

    public async Task<IEnumerable<Notification>> GetNotificationsAsync(QueryNotifications query, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        var result = await connection.QueryAsync<Notification>(
            StoredProcedures.Notification.Query, new
            {
                query.UserId,
                query.Status,
                query.Type,
                query.Id,
                query.Priority,
                query.Recipient,
                query.Sender,
                query.TemplateId,
                query.Search,
                query.IsRead,
                query.Page,
                query.Limit
            }, commandType: CommandType.StoredProcedure);
        return result;
    }

    public async Task<int> GetNotificationsCountAsync(QueryNotifications query, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new
        {
            query.UserId,
            query.Status,
            query.Type,
            query.Id,
            query.Priority,
            query.Recipient,
            query.Sender,
            query.TemplateId,
            query.Search,
            query.IsRead,
        };

        cancellationToken.ThrowIfCancellationRequested();

        var result = await connection.QuerySingleAsync<int>(
            StoredProcedures.Notification.Count, parameters, commandType: CommandType.StoredProcedure);

        return result;
    }

    public async Task<IEnumerable<Notification>> GetPendingNotificationsAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        var result = await connection.QueryAsync<Notification>(
            StoredProcedures.Notification.GetPending, commandType: CommandType.StoredProcedure);

        return result;
    }

    public async Task UpdateNotificationStatusAsync(Guid id, NotificationStatus status, string? errorMessage = null, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(StoredProcedures.Notification.UpdateStatus, new { Id = id, Status = (int)status, ErrorMessage = errorMessage }, commandType: CommandType.StoredProcedure);
    }

    public async Task UpdateNotificationSentAsync(Guid id, DateTime sentAt, string? externalId = null, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(StoredProcedures.Notification.UpdateSent, new { Id = id, SentAt = sentAt, ExternalId = externalId }, commandType: CommandType.StoredProcedure);
    }

    public async Task UpdateNotificationDeliveredAsync(Guid id, DateTime deliveredAt, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(StoredProcedures.Notification.UpdateDelivered, new { Id = id, DeliveredAt = deliveredAt }, commandType: CommandType.StoredProcedure);
    }

    public async Task IncrementRetryCountAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(StoredProcedures.Notification.IncrementRetryCount, new { Id = id }, commandType: CommandType.StoredProcedure);
    }

    public async Task<int> GetUnreadCountAsync(string userId, CancellationToken cancellationToken = default)
    {

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<int>(StoredProcedures.Notification.GetUnreadCount, new { UserId = userId }, commandType: CommandType.StoredProcedure);
    }

    public async Task MarkAsReadAsync(Guid id, CancellationToken cancellationToken = default)
    {

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(StoredProcedures.Notification.MarkAsRead, new { Id = id }, commandType: CommandType.StoredProcedure);
    }
}
