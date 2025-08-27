using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using OdixPay.Notifications.API.Constants;
using OdixPay.Notifications.API.Middleware;
using OdixPay.Notifications.API.Models.Response;
using OdixPay.Notifications.Application.DependencyInjection;
using OdixPay.Notifications.Infrastructure.DependencyInjection;
using OdixPay.Notifications.API.Auth;

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

// Configure Authorization
// Add authorization with global policy
// Add Authorization without fallback policy
builder.Services.AddAuthorizationBuilder();

// Configure Authentication
builder.Services.AddAuthentication(ApiConstants.Authentication.CustomAuthScheme)
    .AddScheme<AuthenticationSchemeOptions, CustomAuthHandler>(ApiConstants.Authentication.CustomAuthScheme, null);


// Add services to the container.
// Add layer services
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration).AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(ApiConstants.APIVersion.Version, 0); // Default to v1.0
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
            ContentTypes = { ApiConstants.Response.ContentType }
        };

        return objectResult;
    };
});

// Add controllers and OpenAPI support
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// CORS middleware
app.UseCors(corsPolicyName);


// Configure the HTTP request pipeline
// Apply middlewares except for webhook endpoints
app.UseWhen(context => !context.Request.Path.StartsWithSegments($"/api/{ApiConstants.APIVersion.VersionName}/webhooks"), app =>
{
    app.UseAuthentication();
    app.UseAuthorization();
    // Standard response builder middleware
    app.UseStandardResponse();
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseGlobalExceptionHandling();

app.MapControllers();

app.Run();
