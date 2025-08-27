using System.Net.Http.Json;
using System.Text.Json;
using OdixPay.Notifications.Domain.DTO.Requests;
using OdixPay.Notifications.Domain.DTO.Responses;
using OdixPay.Notifications.Domain.Interfaces;

namespace OdixPay.Notifications.Infrastructure.Services;

public class BrevoClient(HttpClient httpClient) : IBrevoClient
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<BrevoSendEmailResponse> SendEmailAsync(BrevoSendEmailRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request), "BrevoSendEmailRequest cannot be null.");
        }

        if (string.IsNullOrEmpty(request.Sender?.Email) || string.IsNullOrEmpty(request.Sender.Name) || string.IsNullOrWhiteSpace(request.Sender.Name))
        {
            throw new ArgumentException("Sender email cannot be null or empty.", nameof(request.Sender));
        }

        var res = await _httpClient.PostAsJsonAsync("/v3/smtp/email", request, cancellationToken);


        var txt = await res.Content.ReadAsStringAsync(cancellationToken);

        System.Console.WriteLine("Response text: " + txt);

        if (!res.IsSuccessStatusCode)
        {
            System.Console.WriteLine("Error response text: " + txt);
            res.EnsureSuccessStatusCode();
        }

        var jsonDate = await res.Content.ReadFromJsonAsync<BrevoSendEmailResponse>(cancellationToken: cancellationToken) ?? throw new InvalidOperationException("Failed to deserialize BrevoSendEmailResponse.");

        return JsonSerializer.Deserialize<BrevoSendEmailResponse>(txt) ?? throw new InvalidOperationException("Failed to deserialize BrevoSendEmailResponse.");
    }
}
