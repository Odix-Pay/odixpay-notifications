using Microsoft.Extensions.DependencyInjection;
using OdixPay.Notifications.Application.Handlers.MessageBrokerEvents;
using OdixPay.Notifications.Application.Mappings;
using OdixPay.Notifications.Application.Services;
using OdixPay.Notifications.Application.UseCases;
using OdixPay.Notifications.Domain.Interfaces;
using System.Reflection;

namespace OdixPay.Notifications.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Add MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

        // Register application use cases
        services.AddScoped<INotificationCommandHandler, NotificationCreatedCommandHandler>();
        services.AddScoped<ValidateTemplateVariables, ValidateTemplateVariables>();

        // Add Template Engine
        services.AddScoped<ITemplateEngine, TemplateEngine>();


        return services;
    }
}
