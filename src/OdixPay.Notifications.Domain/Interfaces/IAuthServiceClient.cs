using OdixPay.Notifications.Domain.DTO.Responses;
using OdixPay.Notifications.Domain.DTO.Responses.AuthService;

namespace OdixPay.Notifications.Domain.Interfaces;

public interface IAuthServiceClient
{
    Task<GetPublicKeyResponse?> GetPublicKeyAsync(string keyId);

    Task<PaginatedResponseDTO<GetPublicKeyResponse>> QueryPublicKeysAsync(Dictionary<string, object>? query);
}