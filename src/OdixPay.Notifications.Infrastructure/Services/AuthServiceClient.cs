using System.Net.Http.Json;
using OdixPay.Notifications.Domain.DTO.Responses;
using OdixPay.Notifications.Domain.DTO.Responses.AuthService;
using OdixPay.Notifications.Domain.Interfaces;

namespace OdixPay.Notifications.Infrastructure.Services;

public class AuthServiceClient(HttpClient client) : IAuthServiceClient
{
    private readonly HttpClient _client = client;

    public async Task<GetPublicKeyResponse?> GetPublicKeyAsync(string keyId)
    {
        var response = await _client.GetAsync($"/public-keys/{keyId}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<GetPublicKeyResponse>()
            ?? null;
    }

    public async Task<PaginatedResponseDTO<GetPublicKeyResponse>> QueryPublicKeysAsync(Dictionary<string, object>? query)
    {
        // Uncommennt below code for production mode
        var response = await _client.GetAsync($"/api/v1/auth/getpublickey");

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PaginatedResponseDTO<GetPublicKeyResponse>>()
            ?? throw new InvalidOperationException("Failed to deserialize public key response");
    }
}