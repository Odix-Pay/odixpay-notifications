using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using OdixPay.Notifications.Domain.Interfaces;
using OdixPay.Notifications.Domain.Models;
using OdixPay.Notifications.Domain.DTO.Requests;
using OdixPay.Notifications.Domain.DTO.Responses;
using System.Text.Json;

namespace OdixPay.Notifications.Infrastructure.Services;

public class RoleServiceHTTPClient(
        HttpClient httpClient,
        IDistributedCacheService cache,
        ILogger<RoleServiceHTTPClient> logger) : IRoleServiceHTTPClient
{
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly IDistributedCacheService _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    private readonly ILogger<RoleServiceHTTPClient> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly TimeSpan _cacheDuration = TimeSpan.FromHours(1);

    public async Task<List<Permission>> GetPermissionsAsync(string role)
    {
        if (string.IsNullOrEmpty(role))
            throw new ArgumentException("Role cannot be null or empty.", nameof(role));

        // Try cache first
        var cacheKey = $"permissions_{role}";
        var cachedData = await _cache.GetAsync<List<Permission>>(cacheKey);

        if (cachedData != null && cachedData.Count > 0)
        {
            _logger.LogInformation("Permissions for role {Role} retrieved from cache.", role);
            return cachedData;
        }

        // Fetch from roles service
        try
        {
            var response = await _httpClient.GetAsync($"/permissions/role/{role}");
            response.EnsureSuccessStatusCode();
            var permissions = await response.Content.ReadFromJsonAsync<List<Permission>>();
            permissions ??= [];

            // Cache the result
            _cache.SetAsync(
                cacheKey,
                permissions,
                _cacheDuration
            );

            _logger.LogInformation("Permissions for role {Role} cached.", role);

            return permissions;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch permissions for role {Role}.", role);
            return [];
        }
    }

    public async Task<UserRole> GetUserRole(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            throw new ArgumentException("User identifier cannot be empty.", nameof(userId));

        // Try cache first
        var cacheKey = $"UserRole_{userId}";
        var cachedData = await _cache.GetAsync<UserRole>(cacheKey);

        if (cachedData != null)
        {
            _logger.LogInformation("Roles for user {UserId} retrieved from cache.", userId);

            return cachedData;
        }

        // Fetch from roles service
        try
        {
            var response = await _httpClient.GetAsync($"/roles/user/{userId}");
            response.EnsureSuccessStatusCode();
            var role = await response.Content.ReadFromJsonAsync<UserRole>();
            role ??= new UserRole();

            // Cache the result
            _cache.SetAsync(
                cacheKey,
                role,
                _cacheDuration
            );

            _logger.LogInformation("Roles for role {UserId} cached.", role);

            return role;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch roles for role {UserId}.", userId);
            return new UserRole();
        }
    }

    public async Task<bool> CheckPermission(CheckUserPermissionForResourceDTO dto)
    {

        if (!dto.IsValid())
            throw new ArgumentException("Invalid data.", nameof(dto));

        // Attempt to get user permission status of resource from cache
        var cacheKey = $"UserPermission_{dto.UserId}:{dto.Permission}";

        var cachedData = await _cache.GetAsync<UserPermissionResponseDTO>(cacheKey);

        // If permission exists then user already has permision for this resource, return true
        if (cachedData != null && cachedData.Data.HasPermission)
        {
            _logger.LogInformation("Permission for resource {Resource} for user {UserId} retrieved from cache.", dto.Permission, dto.UserId);

            return true;
        }

        // Try to get permission from permissions (roles) service
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"/api/v1.0/Permission/CheckUserPermission", dto);
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Permission for resource {Resource} retrieved from roles service.", JsonSerializer.Serialize(dto));
            var content = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Response content: {Content}", content);

            var permission = JsonSerializer.Deserialize<UserPermissionResponseDTO>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            _logger.LogInformation("Data: {Data}.", permission?.Data.HasPermission);

            // If no permission, do not set anything
            if (permission == null)
            {
                return false;
            }

            _logger.LogInformation("Permission for resource {Resource} retrieved from roles service.", JsonSerializer.Serialize(permission));

            // If permission is false, do not set anything
            if (!permission.Data.HasPermission)
            {
                _logger.LogInformation("User {UserId} does not have permission for resource {Resource}.", dto.UserId, dto.Permission);
                return false;
            }

            _logger.LogInformation("User {UserId} has permission for resource {Resource}.", dto.UserId, dto.Permission);
            // Cache the result
            _cache.SetAsync(
                cacheKey,
                permission,
                _cacheDuration
            );

            _logger.LogInformation("Resource permission for resource {Resource}, user {User} cached.", dto.Permission, dto.UserId);

            return true;

        }
        catch (HttpRequestException)
        {
            _logger.LogInformation("Resource permission for resource {Resource}, user {User} failed.", dto.Permission, dto.UserId);
            return false;
        }
    }
}