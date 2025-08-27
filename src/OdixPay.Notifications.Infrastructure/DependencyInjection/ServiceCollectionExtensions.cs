using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OdixPay.Notifications.Domain.Interfaces;
using OdixPay.Notifications.Infrastructure.Configuration;
using OdixPay.Notifications.Infrastructure.Data;
using OdixPay.Notifications.Infrastructure.Repositories;
using OdixPay.Notifications.Infrastructure.Services;
using StackExchange.Redis;
using OdixPay.Notifications.Infrastructure.Services.StartupTasks;
using Microsoft.Extensions.Options;
using OdixPay.Notifications.Infrastructure.Services.BackgroundTasks;
using OdixPay.Notifications.Infrastructure.Services.StartupTasks.InterlaceNotifications;

namespace OdixPay.Notifications.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Db Configuration
        services.Configure<DbConfig>(configuration.GetSection("DbConfig"));
        services.Configure<RoleServiceHTTPClientConfig>(configuration.GetSection("RoleService"));
        services.Configure<AuthServiceHTTPClientConfig>(configuration.GetSection("AuthService"));
        services.Configure<RabbitMqConfig>(configuration.GetSection("RabbitMq"));
        services.Configure<BrevoConfig>(configuration.GetSection("Brevo"));
        services.Configure<TwilioConfig>(configuration.GetSection("Twilio"));
        services.Configure<FirebaseConfig>(configuration.GetSection("FirebaseConfig"));

        // Add Db Connection Factory
        services.AddSingleton<IConnectionFactory, ConnectionFactory>();

        // Add AuthService HTTP Client
        services.AddHttpClient<IAuthServiceClient, AuthServiceClient>((provider, client) =>
        {
            var config = provider.GetRequiredService<IOptions<AuthServiceHTTPClientConfig>>().Value;
            client.BaseAddress = new Uri(config.BaseUrl);
            client.Timeout = config.Timeout;
        }).ConfigurePrimaryHttpMessageHandler(() =>
        {
            var handler = new HttpClientHandler();

            // Only for development - skip SSL validation
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            }

            return handler;
        });

        // Add Roles microservice HTTP client
        services.AddHttpClient<IRoleServiceHTTPClient, RoleServiceHTTPClient>((provider, client) =>
        {
            var config = provider.GetRequiredService<IOptions<RoleServiceHTTPClientConfig>>().Value;
            client.BaseAddress = new Uri(config.BaseUrl);
            client.Timeout = config.Timeout;
        });

        // Add Template Engine
        services.AddScoped<ITemplateEngine, TemplateEngine>();

        // Add Brevo email service
        services.AddHttpClient<IBrevoClient, BrevoClient>((provider, client) =>
        {
            var config = provider.GetRequiredService<IOptions<BrevoConfig>>().Value;
            client.BaseAddress = new Uri(config.BaseUrl);
            client.Timeout = config.Timeout;
            client.DefaultRequestHeaders.Add("api-key", config.ApiKey);
        }).ConfigurePrimaryHttpMessageHandler(() =>
        {
            var handler = new HttpClientHandler();

            // Only for development - skip SSL validation
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            }

            return handler;
        });

        // Add Twilio SMS service
        services.AddScoped<ISmsService, SmsService>();

        // Add Email service
        services.AddScoped<IEmailService, EmailService>();

        // Add Push Notification service
        services.AddScoped<IPushNotificationService, PushNotificationService>();

        // Add Notification Service
        services.AddScoped<INotificationService, NotificationService>();


        // Add Repositories
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INotificationTemplateRepository, NotificationTemplateRepository>();
        services.AddScoped<INotificationRecipientRepository, NotificationRecipientRepository>();

        // Register authentication and authorization services
        services.AddScoped<IAuthenticationService, AuthenticationService>();

        // Add Distributed Cache Service
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {

            Console.WriteLine("Initializing Redis connection...");
            var configuration = sp.GetRequiredService<IConfiguration>();
            if (configuration == null)
            {
                throw new InvalidOperationException("Configuration is not available. Ensure it is properly set up in the application.");
            }
            // Get Redis connection string from configuration
            var connectionString = configuration.GetConnectionString("Redis")
                ?? throw new ArgumentNullException("Redis connection string is not configured. Please set it in the application settings.");

            if (string.IsNullOrEmpty(connectionString))
            {
                Console.WriteLine("Redis connection string is not configured. Please set it in the application settings.");
                throw new ArgumentNullException("Redis connection string is not configured. Please set it in the application settings.");
            }

            Console.WriteLine("Connecting to Redis with connection string: ");

            return ConnectionMultiplexer.Connect(connectionString);
        });
        services.AddSingleton<IDistributedCacheService, RedisCacheService>();

        // Register RabbitMQ service
        services.AddSingleton<IEventBusService, RabbitMQService>();

        // Startup tasks
        services.AddHostedService<InitPublickeys>();
        services.AddHostedService<NotificationProcessorService>();
        services.AddHostedService<Interlace3dSecureOtpSubscriber>();
        services.AddHostedService<Interlace3dsOtpSubscriber>();
        services.AddHostedService<InterlaceNotificationsSubscriber>();


        return services;
    }
}
