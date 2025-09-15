using OdixPay.Notifications.Domain.Interfaces;
using OdixPay.Notifications.Domain.Entities;
using OdixPay.Notifications.Infrastructure.Data;
using Dapper;
using OdixPay.Notifications.Infrastructure.Constants;
using OdixPay.Notifications.Domain.Enums;
using OdixPay.Notifications.Domain.DTO.Requests;
using System.Data;

namespace OdixPay.Notifications.Infrastructure.Repositories;


public class NotificationRecipientRepository(IConnectionFactory connectionFactory) : INotificationRecipientRepository
{

    private readonly IConnectionFactory _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));

    public async Task<NotificationRecipient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var connection = _connectionFactory.CreateConnection();

        var result = await connection.QueryFirstOrDefaultAsync<NotificationRecipient>(
            StoredProcedures.NotificationRecipient.GetById, new { Id = id });

        return result;
    }

    public async Task<(IEnumerable<NotificationRecipient> recipients, int TotalCount)> GetByUserIdAsync(string userId, int page = 1, int limit = 20, CancellationToken cancellationToken = default)
    {
        return await QueryAsync(new()
        {
            UserId = userId,
            Page = page,
            Limit = limit
        }, cancellationToken);
    }

    public async Task<NotificationRecipient?> GetByUserIdAndTypeAsync(string userId, NotificationType type, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var connection = _connectionFactory.CreateConnection();

        var result = await connection.QueryFirstOrDefaultAsync<NotificationRecipient>(
            StoredProcedures.NotificationRecipient.GetByUserIdAndType, new { UserId = userId, Type = (int)type });

        return result;
    }

    public async Task AddAsync(NotificationRecipient recipient, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var connection = _connectionFactory.CreateConnection();

        var parameters = new
        {
            recipient.Id,
            recipient.Name,
            recipient.UserId,
            Type = (int)recipient.Type,
            recipient.Recipient,
            recipient.IsActive,
            recipient.DefaultLanguage
        };

        await connection.ExecuteAsync(StoredProcedures.NotificationRecipient.Create, parameters, commandType: CommandType.StoredProcedure);
    }

    public async Task UpdateAsync(NotificationRecipient recipient, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var connection = _connectionFactory.CreateConnection();

        var parameters = new
        {
            recipient.Id,
            recipient.Name,
            recipient.UserId,
            recipient.Recipient,
            recipient.IsActive,
            recipient.IsDeleted,
            recipient.DefaultLanguage
        };

        await connection.ExecuteAsync(StoredProcedures.NotificationRecipient.Update, parameters, commandType: CommandType.StoredProcedure);
    }
    public async Task UpdateRecipientLanguageAsync(string recipientId, string language, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var connection = _connectionFactory.CreateConnection();

        var parameters = new
        {
            RecipientId = recipientId,
            Language = language
        };

        await connection.ExecuteAsync(StoredProcedures.NotificationRecipient.UpdateLanguage, parameters, commandType: CommandType.StoredProcedure);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(StoredProcedures.NotificationRecipient.Delete, new { Id = id });
    }

    public async Task<int> CountAsync(QueryNotificationRecipientsRequestDTO query, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var connection = _connectionFactory.CreateConnection();

        var parameters = new
        {
            query.Id,
            query.UserId,
            query.Type,
            query.Search,
            query.IsActive,
            query.IsDeleted,
            query.DefaultLanguage
        };

        var count = await connection.QuerySingleAsync<int>(
            StoredProcedures.NotificationRecipient.Count, parameters);

        return count;
    }

    public async Task<(IEnumerable<NotificationRecipient> Recipients, int TotalCount)> QueryAsync(QueryNotificationRecipientsRequestDTO query, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Start both database operations concurrently
        var recipientsTask = ExecuteQueryAsync(query, cancellationToken);
        var countTask = CountAsync(query, cancellationToken);

        // Wait for both operations to complete
        var recipients = await recipientsTask;
        var totalCount = await countTask;

        return (recipients, totalCount);
    }

    private async Task<IEnumerable<NotificationRecipient>> ExecuteQueryAsync(QueryNotificationRecipientsRequestDTO query, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var connection = _connectionFactory.CreateConnection();

        var parameters = new
        {
            query.Id,
            query.UserId,
            query.Type,
            query.Search,
            query.IsActive,
            query.IsDeleted,
            query.DefaultLanguage,
            query.Page,
            query.Limit,
        };

        var recipients = await connection.QueryAsync<NotificationRecipient>(
            StoredProcedures.NotificationRecipient.Query, parameters, commandType: CommandType.StoredProcedure);

        return recipients;
    }
}