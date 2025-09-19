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
using OdixPay.Notifications.Infrastructure.Filters;
using Microsoft.AspNetCore.SignalR;
using OdixPay.Notifications.Contracts.Interfaces;
using OdixPay.Notifications.Infrastructure.Services.StartupTasks.TransactionDetectionEvents;
using OdixPay.Notifications.Infrastructure.Services.StartupTasks.AppSettingsChanged;
using OdixPay.Notifications.Infrastructure.Services.StartupTasks.RealtimeNotifications;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

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
        services.Configure<RedisConfig>(configuration.GetSection("Redis"));

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
            var redisConfig = sp.GetRequiredService<IOptions<RedisConfig>>().Value ?? throw new ArgumentNullException(nameof(RedisConfig), "Redis configuration is not provided.");

            Console.WriteLine($"Initializing Redis connection...: {redisConfig.GetMaskedConnectionString()}");

            return ConnectionMultiplexer.Connect(redisConfig.GetConnectionString());
        });
        services.AddSingleton<IDistributedCacheService, RedisCacheService>();

        // Add SignalR
        services.AddSignalR(opts =>
        {
            opts.EnableDetailedErrors = true;
            // Add our custom authorization filter to the hub pipeline
            opts.AddFilter<HubAuthorizationFilter>();
        }) // Include Redis backplane for SignalR - This enables scaling out to multiple servers (replica sets).
        .AddStackExchangeRedis(opts =>
        {
            var redisConfig = services.BuildServiceProvider().GetRequiredService<IOptions<RedisConfig>>().Value ?? throw new ArgumentNullException(nameof(RedisConfig), "Redis configuration is not provided.");

            Console.WriteLine($"Initializing Redis connection...: {redisConfig.GetMaskedConnectionString()}");

            var cfg = ConfigurationOptions.Parse(redisConfig.GetConnectionString());
            cfg.AbortOnConnectFail = false;
            cfg.ConnectRetry = 5;
            cfg.ReconnectRetryPolicy = new ExponentialRetry(5000);
            cfg.ClientName = "odixpay-signalr";
            cfg.DefaultDatabase = 1; // isolate from your app cache (usually DB0)

            // Let SignalR create/own its pub-sub connection:
            opts.ConnectionFactory = async writer => await ConnectionMultiplexer.ConnectAsync(cfg, writer);

        });
        // Add realtime notification service
        services.AddScoped<IRealtimeNotifier, RealtimeNotificationService>();
        // Realtime hub authorizer
        services.AddScoped<IHubAuthorizer, HubAuthorizer>();


        // Register RabbitMQ service
        services.AddSingleton<IEventBusService, RabbitMQService>();

        // Startup tasks
        services.AddHostedService<NotificationProcessorService>();
        services.AddHostedService<InitPublickeys>();
        services.AddHostedService<NotificationProcessorService>();
        services.AddHostedService<Interlace3dSecureOtpSubscriber>();
        services.AddHostedService<Interlace3dsOtpSubscriber>();
        services.AddHostedService<InterlaceNotificationsSubscriber>();
        services.AddHostedService<SubscribeToScanningEvents>();
        services.AddHostedService<AppSettingsChangedEventsHandler>();
        services.AddHostedService<RealtimeNotificationsHandler>();

        // Configure firebase instance
        var firebaseConfig = configuration.GetSection("FirebaseConfig").Get<FirebaseConfig>() ?? throw new ArgumentNullException(nameof(FirebaseConfig), "Firebase configuration is not provided.");
        FirebaseApp.Create(new AppOptions()
        {
            Credential = GoogleCredential.FromFile(firebaseConfig.ServiceAccountKeyPath),
            ProjectId = firebaseConfig.ProjectId
        });


        return services;
    }
}
