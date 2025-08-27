using OdixPay.Notifications.Domain.DTO.Requests;
using OdixPay.Notifications.Domain.Models;

namespace OdixPay.Notifications.Domain.Interfaces;

public interface IRoleServiceHTTPClient
{
    Task<UserRole> GetUserRole(string userId);
    Task<List<Permission>> GetPermissionsAsync(string role);
    Task<bool> CheckPermission(CheckUserPermissionForResourceDTO dto);
}