using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OdixPay.Notifications.Domain.DTO.Requests;
using OdixPay.Notifications.Domain.DTO.Responses;
using OdixPay.Notifications.Domain.Interfaces;
using OdixPay.Notifications.Infrastructure.Configuration;

namespace OdixPay.Notifications.Infrastructure.Services;

public class EmailService(ILogger<EmailService> logger, IBrevoClient brevoClient, IOptions<BrevoConfig> brevoOptions) : IEmailService
{
    private readonly ILogger<EmailService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly BrevoConfig _brevoConfig = brevoOptions.Value ?? throw new ArgumentNullException(nameof(brevoOptions));
    private readonly IBrevoClient _brevoClient = brevoClient ?? throw new ArgumentNullException(nameof(brevoClient));

    public async Task<SendNotificationResult> SendEmailAsync(SendNotificationRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            _logger.LogError("SendNotificationRequest is null.");
            throw new ArgumentNullException(nameof(request), "SendNotificationRequest cannot be null.");
        }

        var to = request.Recipient;
        var from = request.Sender ?? _brevoConfig.DefaultSender;
        var subject = request.Title ?? "No Subject";
        var body = request.Message ?? "No Message";

        var sent = await _brevoClient.SendEmailAsync(new BrevoSendEmailRequest
        {
            To =
            [
                new ()
                    {
                        Email = to,
                        Name = to
                    }
            ],
            Sender = new()
            {
                Email = from,
                Name = _brevoConfig.DefaultSenderName
            },
            Subject = subject,
            HtmlContent = body
        }, cancellationToken);

        return new()
        {
            Success = sent != null,
            ExternalId = sent?.MessageId,
            ErrorMessage = sent == null ? "Failed to send email." : null,
            SentAt = DateTime.UtcNow
        };
    }

}