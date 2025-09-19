using System.IdentityModel.Tokens.Jwt;
using System.Security;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using OdixPay.Notifications.Domain.Models;
using OdixPay.Notifications.Domain.Common;
using OdixPay.Notifications.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using OdixPay.Notifications.Domain.DTO.Responses.AuthService;
using OdixPay.Notifications.Domain.DTO.Requests;
using FirebaseAdmin.Auth;

namespace OdixPay.Notifications.Infrastructure.Services;

public class AuthenticationService(
        IAuthServiceClient authClient,
        IDistributedCacheService cache,
        IRoleServiceHTTPClient roleServiceClient,
        ILogger<AuthenticationService> logger
        ) : IAuthenticationService
{
    private readonly IAuthServiceClient _authClient = authClient;
    private readonly IDistributedCacheService _cache = cache;
    private readonly IRoleServiceHTTPClient _roleServiceClient = roleServiceClient;
    private readonly ILogger<AuthenticationService> _logger = logger;

    public async Task<AuthenticationResult> AuthenticateAsync(string token)
    {
        try
        {
            _logger.LogInformation("Starting authentication process.");

            var (tokenType, cleanToken, keyId) = IdentifyTokenType(token);

            _logger.LogInformation("Token type gotten for key: {KeyId}", keyId);

            GetPublicKeyResponse? publicKey = tokenType != TokenType.Firebase ? await GetPublicKeyAsync(keyId) : null;

            _logger.LogInformation("Public key retrieved for key: {KeyId}", keyId);

            var validToken = await ValidateToken(cleanToken, publicKey?.PublicKey, tokenType);


            return validToken;
        }
        catch (Exception ex)
        {
            _logger.LogError("Authentication failed for token: {Token}", token);
            _logger.LogError(ex, "Exception occurred during authentication: {Message}", ex.Message);
            return new AuthenticationResult { IsAuthenticated = false };
        }
    }

    public async Task<AuthorizationResult> AuthorizeRoleAsync(string userId, string permission)
    {
        try
        {

            if (string.IsNullOrEmpty(userId))
            {
                return AuthorizationResult.Unauthorized("User is not authenticated");
            }

            // Check if the role has permission for the requested action and resource
            bool hasPermission = await _roleServiceClient.CheckPermission(new CheckUserPermissionForResourceDTO(userId, permission));

            if (!hasPermission)
            {
                return AuthorizationResult.Unauthorized("User is not authorized");
            }

            return AuthorizationResult.Success();
        }
        catch (Exception)
        {
            return new AuthorizationResult
            {
                IsAuthenticated = false,
                IsAuthorized = false
            };
        }

    }

    private (TokenType Type, string CleanToken, string KeyId) IdentifyTokenType(string token)
    {
        if (token.StartsWith("Bearer "))
        {
            var cleanToken = token["Bearer ".Length..];
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(cleanToken);
            var keyId = jwtToken.Header["kid"]?.ToString() ?? throw new SecurityException("Token is missing 'kid' header claim");
            return (TokenType.Bearer, cleanToken, keyId);
        }

        if (token.StartsWith("Signature "))
        {
            var cleanToken = token["Signature ".Length..];
            var keyId = cleanToken.Split('.')[0]; // Example: "keyId.payload.signature"
            return (TokenType.Signature, cleanToken, keyId);
        }

        if (token.StartsWith("Firebase "))
        {
            _logger.LogInformation("Identified Firebase token.");
            var cleanToken = token["Firebase ".Length..];
            return (TokenType.Firebase, cleanToken.Trim(), string.Empty);
        }

        throw new SecurityException("Invalid token format");
    }

    private async Task<GetPublicKeyResponse> GetPublicKeyAsync(string keyId)
    {
        var cacheKey = $"PublicKey_{keyId}";
        // Try to get public key from redis cache
        var publicKey = await _cache.GetAsync<GetPublicKeyResponse>(cacheKey);

        if (publicKey != null)
        {
            _logger.LogInformation("Public key for keyId {KeyId} retrieved from cache.", keyId);
            return publicKey;
        }
        // Try to get public key from authentication service
        publicKey = await _authClient.GetPublicKeyAsync(keyId);

        if (publicKey == null)
        {
            _logger.LogError("Public key for keyId {KeyId} not found in authentication service.", keyId);

            throw new SecurityException($"Public key for keyId {keyId} not found.");
        }

        _cache.SetAsync(cacheKey, publicKey, TimeSpan.FromMinutes(15));

        return publicKey;
    }

    private async Task<AuthenticationResult> ValidateToken(string token, string publicKey, TokenType tokenType)
    {
        return tokenType switch
        {
            TokenType.Bearer => await ValidateJwtToken(token, publicKey),
            TokenType.Signature => await ValidateSignatureToken(token, publicKey),
            TokenType.Firebase => await ValidateFirebaseToken(token),
            _ => throw new ArgumentOutOfRangeException(nameof(tokenType),
                $"Unsupported token type: {tokenType}")
        };
    }

    private async Task<AuthenticationResult> ValidateJwtToken(string token, string publicKey)
    {
        try
        {
            var rsa = RSA.Create();
            rsa.ImportFromPem(publicKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new RsaSecurityKey(rsa),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var keyId = jwtToken.Header["kid"]?.ToString();
            var principal = handler.ValidateToken(token, validationParameters, out _);

            _logger.LogInformation("Decoded token key: {Token}", principal.FindFirst("kid")?.Value);

            return new AuthenticationResult
            {
                IsAuthenticated = true,
                UserId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? principal.FindFirst("userId")?.Value ?? principal.FindFirst("nameid")?.Value,
                Role = principal.FindFirst(ClaimTypes.Role)?.Value,
                KeyId = principal.FindFirst("keyid")?.Value ?? keyId,
                PublicKey = publicKey,
                Region = principal.FindFirst("region")?.Value?.ToLowerInvariant()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError("Exception occured: {EX}", ex);
            return new AuthenticationResult { IsAuthenticated = false, ErrorMessage = ex.Message };
        }
    }

    private async Task<AuthenticationResult> ValidateSignatureToken(string token, string publicKey)
    {
        try
        {
            var parts = token.Split('.');
            if (parts.Length != 3) // keyId.payload.signature
                return new AuthenticationResult { IsAuthenticated = false };

            var payload = parts[1];
            var signature = Convert.FromBase64String(parts[2]);

            var rsa = RSA.Create();
            rsa.ImportFromPem(publicKey);

            var payloadBytes = Encoding.UTF8.GetBytes(payload);
            var isValid = rsa.VerifyData(
                payloadBytes,
                signature,
                HashAlgorithmName.SHA512,
                RSASignaturePadding.Pkcs1);

            return new AuthenticationResult
            {
                IsAuthenticated = isValid,
                UserId = parts[1], // Extract user ID from payload as needed
                KeyId = parts[0],
                Role = "Root", // Super admin role. Private keys must be attched to superadmin (root user) role.
                PublicKey = publicKey,
                MerchantId = parts[1] // Assuming keyId is the merchant ID
            };
        }
        catch (Exception ex)
        {
            return new AuthenticationResult { IsAuthenticated = false, ErrorMessage = ex.Message };
        }
    }

    private async Task<AuthenticationResult> ValidateFirebaseToken(string token)
    {
        try
        {
            if (string.IsNullOrEmpty(token))
                throw new ArgumentNullException(nameof(token), "Token cannot be null or empty");

            var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(token);

            // You can add more checks here if needed (audience, issuer, etc.)
            return new AuthenticationResult
            {
                IsAuthenticated = true,
                UserId = decodedToken.Subject,
                Role = string.Empty, // You can extract role from custom claims if set up
                KeyId = decodedToken.Issuer,
                PublicKey = null,
                Region = "europe" // Default to europe if not set
            };
        }
        catch (Exception ex)
        {
            _logger.LogError("Firebase token validation failed: {Message}", ex.Message);
            // Log ex if needed
            return new AuthenticationResult { IsAuthenticated = false, ErrorMessage = ex.Message };
        }
    }

    private async Task<string?> GetUserRole(string userId)
    {
        // Get user role from cache, if exists, retu
        var roleData = await _roleServiceClient.GetUserRole(userId);

        return roleData?.Name;
    }

}