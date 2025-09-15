namespace OdixPay.Notifications.Contracts.Constants;

public static class APIConstants
{
    /// <summary>
    /// Constants related to HTTP headers used in authentication and authorization.
    /// </summary>
    public static class Headers
    {
        public const string Authorization = "Authorization";
        public const string BearerPrefix = "Bearer ";
        public const string XUserId = "XUserId";
        public const string XRegion = "X-Region";
        public const string XUserRole = "XUserRole";
        public const string KeyId = "KeyId";
        public const string PublicKey = "PublicKey";
    }

    /// <summary>
    /// Constants for standard response formatting.
    /// </summary>
    public static class Response
    {
        public const string SuccessStatus = "success";
        public const string ErrorStatus = "error";
        public const string ContentType = "application/json";
    }

    /// <summary>
    /// Constants for error types used in StandardResponseMiddleware.
    /// </summary>
    public static class ErrorTypes
    {
        public const string Unauthorized = "Unauthorized";
        public const string Forbidden = "Forbidden";
        public const string NotFound = "NotFound";
        public const string BadRequest = "BadRequest";
        public const string ValidationError = "ValidationError";
        public const string Conflict = "Conflict";
        public const string InternalServerError = "InternalServerError";
    }

    /// <summary>
    /// Constants for middleware paths to exclude from processing authentication.
    /// </summary>
    public static class Paths
    {
        public const string Swagger = "/swagger";
        public const string VersionedSwagger = "/v1/swagger";
    }

    /// <summary>
    /// Constants for authentication scheme.
    /// </summary>
    public static class Authentication
    {
        public const string CustomAuthScheme = "Bearer";
    }

    public static class APIVersion
    {
        public const int Version = 1;
        public const string VersionString = "1.0";
        public const string VersionName = "v1";
        public const string VersionRouteName = "api/v{version:apiVersion}";
    }
}