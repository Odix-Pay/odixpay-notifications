using System.Data;
using Dapper;
using OdixPay.Notifications.Domain.DTO.Requests;
using OdixPay.Notifications.Domain.Entities;
using OdixPay.Notifications.Domain.Interfaces;
using OdixPay.Notifications.Domain.Utils;
using OdixPay.Notifications.Infrastructure.Constants;
using OdixPay.Notifications.Infrastructure.Data;

namespace OdixPay.Notifications.Infrastructure.Repositories;

public class NotificationTemplateRepository(IConnectionFactory connectionFactory) : INotificationTemplateRepository
{
    private readonly IConnectionFactory _connectionFactory = connectionFactory;

    public async Task<NotificationTemplate> CreateTemplateAsync(NotificationTemplate template, CancellationToken cancellationToken = default)
    {

        using var connection = _connectionFactory.CreateConnection();


        template.GenerateSlug(); // Ensure slug is generated before saving


        var parameters = new
        {
            template.Id,
            template.Name,
            template.Slug,
            Type = (int)template.Type,
            template.Subject,
            template.Body,
            template.Variables,
            template.IsActive,
            template.CreatedAt,
        };

        cancellationToken.ThrowIfCancellationRequested();

        var created = await connection.QueryFirstOrDefaultAsync<NotificationTemplate>(
            StoredProcedures.Template.Create,
            parameters,
            commandType: CommandType.StoredProcedure);

        return created!;
    }

    public async Task<NotificationTemplate?> GetTemplateByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryFirstOrDefaultAsync<NotificationTemplate>(
            StoredProcedures.Template.GetById, new { Id = id });
        return result;
    }

    public async Task<NotificationTemplate?> GetTemplateByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var connection = _connectionFactory.CreateConnection();

        var result = await connection.QueryFirstOrDefaultAsync<NotificationTemplate>(
            StoredProcedures.Template.GetBySlug, new { Slug = SlugifyString.Slugify(name) },
            commandType: CommandType.StoredProcedure);

        return result;
    }

    public async Task<NotificationTemplate?> GetTemplateBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var connection = _connectionFactory.CreateConnection();

        var result = await connection.QueryFirstOrDefaultAsync<NotificationTemplate>(
            StoredProcedures.Template.GetBySlug, new { Slug = SlugifyString.Slugify(slug) },
            commandType: CommandType.StoredProcedure);

        return result;
    }

    public async Task<IEnumerable<NotificationTemplate>> GetActiveTemplatesAsync(int Page = 1, int Limit = 20, CancellationToken cancellationToken = default)
    {
        // This method can be extended to support pagination if needed
        using var connection = _connectionFactory.CreateConnection();

        if (Page <= 0)
        {
            Page = 1; // Default to page 1 if invalid
        }

        if (Limit <= 0)
        {
            Limit = 20; // Default to 20 if invalid
        }
        if (Limit > 100)
        {
            Limit = 100; // Cap limit to prevent excessive load
        }

        cancellationToken.ThrowIfCancellationRequested();

        var parameters = new
        {
            Page,
            Limit
        };

        var result = await connection.QueryAsync<NotificationTemplate>(
            StoredProcedures.Template.GetActive, parameters, commandType: CommandType.StoredProcedure);

        return result;
    }

    public async Task UpdateTemplateAsync(NotificationTemplate template, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var connection = _connectionFactory.CreateConnection();

        var parameters = new
        {
            template.Id,
            template.Name,
            template.Slug,
            Type = (int)template.Type,
            template.Subject,
            template.Body,
            template.Variables,
            template.IsActive,
            template.IsDeleted,
            template.UpdatedAt,
        };

        await connection.ExecuteAsync(
            StoredProcedures.Template.Update,
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task DeleteTemplateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            StoredProcedures.Template.Delete,
            new { Id = id },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> GetTemplatesCountAsync(GetTemplatesQueryDTO query, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var connection = _connectionFactory.CreateConnection();

        var parameters = new
        {
            query.Id,
            query.Type,
            query.Name,
            query.Subject,
            query.Slug,
            query.Search,

        };

        var result = await connection.QuerySingleAsync<int>(
            StoredProcedures.Template.GetTemplatesCount, parameters, commandType: CommandType.StoredProcedure);

        return result;
    }

    public async Task<IEnumerable<NotificationTemplate>> GetTemplatesAsync(GetTemplatesQueryDTO query, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var connection = _connectionFactory.CreateConnection();

        var parameters = new
        {
            query.Id,
            query.Type,
            query.Name,
            query.Subject,
            query.Slug,
            query.Search,
            query.Page,
            query.Limit
        };

        var result = await connection.QueryAsync<NotificationTemplate>(
            StoredProcedures.Template.GetTemplates, parameters, commandType: CommandType.StoredProcedure);

        return result;
    }

}
