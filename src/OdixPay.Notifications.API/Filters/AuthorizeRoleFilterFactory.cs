using Microsoft.AspNetCore.Mvc.Filters;
using OdixPay.Notifications.Domain.Interfaces;

namespace OdixPay.Notifications.API.Filters;

/// <summary>
/// This is a factory class that instantiates AuthorizeRoleFilter so it could be used to authenticate user roles trying to access a resource.
/// </summary>
public class AuthorizeRoleFilterFactory : Attribute, IFilterFactory
{
    public string Permission { get; set; } = string.Empty;
    public bool IsReusable => false;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        return new AuthorizeRoleFilter() { Permission = Permission };
    }
}