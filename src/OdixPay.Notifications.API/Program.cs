using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using OdixPay.Notifications.API.Middleware;
using OdixPay.Notifications.API.Models.Response;
using OdixPay.Notifications.Application.DependencyInjection;
using OdixPay.Notifications.Infrastructure.DependencyInjection;
using OdixPay.Notifications.API.Auth;
using OdixPay.Notifications.Contracts.Constants;
using OdixPay.Notifications.Contracts.Hubs;
using Microsoft.AspNetCore.HttpOverrides;
using OdixPay.Notifications.API.Constants;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Load CORS settings from configuration.
var allowedOrigins = builder.Configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>() ?? [];
string corsPolicyName = "AllowSpecificOrigins";


// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicyName, policy =>
    {
        policy.WithOrigins(allowedOrigins ?? [])
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Configure forwarded headers (if behind a proxy)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // You might need to add this if your proxy is not on localhost
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Configure Authorization
// Add authorization with configured custom policies
builder.Services.AddAuthorizationBuilder().AddPolicy(Permissions.Notification.ReadAdminNotifications, policy =>
       policy.Requirements.Add(new PermissionRequirement(Permissions.Notification.ReadAdminNotifications)));

// Configure Authentication
builder.Services.AddAuthentication(APIConstants.Authentication.CustomAuthScheme)
    .AddScheme<AuthenticationSchemeOptions, CustomAuthHandler>(APIConstants.Authentication.CustomAuthScheme, null);

// Add Authorization policy
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

// Add services to the container.
// Add layer services
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration).AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(APIConstants.APIVersion.Version, 0); // Default to v1.0
        options.AssumeDefaultVersionWhenUnspecified = true; // Use default version if none specified
        options.ApiVersionReader = new UrlSegmentApiVersionReader(); // Read version from URL (e.g., /api/v1)
        options.ReportApiVersions = true; // Include API version in response headers
        options.UseApiBehavior = true; // Enforce API versioning behavior
    })
    .AddVersionedApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV"; // Format version as 'v1', 'v2', etc.
        options.SubstituteApiVersionInUrl = true; // Replace version placeholder in routes
    });

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(e => e.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
            );

        var errorResponse = StandardResponse<object, object>.ValidationError(errors);

        var objectResult = new ObjectResult(errorResponse)
        {
            StatusCode = StatusCodes.Status422UnprocessableEntity,
            ContentTypes = { APIConstants.Response.ContentType }
        };

        return objectResult;
    };
});

// Add controllers and OpenAPI support
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Enable Memory Cache
builder.Services.AddMemoryCache();


var app = builder.Build();

// Use forwarded headers (if behind a proxy) - Should be one of the first middlewares
app.UseForwardedHeaders();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// CORS middleware
app.UseCors(corsPolicyName);

// HTTPS redirection middleware
// app.UseHttpsRedirection();



// Configure the HTTP request pipeline
// Apply middlewares except for webhook endpoints
app.UseWhen(context => !context.Request.Path.StartsWithSegments($"/api/{APIConstants.APIVersion.VersionName}/webhooks"), app =>
{
    app.UseAuthentication();
    app.UseAuthorization();
    // Standard response builder middleware
    // app.UseStandardResponse();
});

app.UseGlobalExceptionHandling();

app.MapControllers();

// Map websocket connection hubs (SignalR) - on base url "/"
app.MapHub<NotificationsHub>($"/api/{APIConstants.APIVersion.VersionName}/hub");

app.Run();
