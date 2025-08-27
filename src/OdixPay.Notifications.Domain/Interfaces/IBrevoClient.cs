using OdixPay.Notifications.Domain.DTO.Requests;
using OdixPay.Notifications.Domain.DTO.Responses;

namespace OdixPay.Notifications.Domain.Interfaces;

public interface IBrevoClient
{
    Task<BrevoSendEmailResponse> SendEmailAsync(BrevoSendEmailRequest request, CancellationToken cancellationToken = default);
}