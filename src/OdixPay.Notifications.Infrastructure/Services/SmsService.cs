using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OdixPay.Notifications.Domain.DTO.Requests;
using OdixPay.Notifications.Domain.DTO.Responses;
using OdixPay.Notifications.Domain.Interfaces;
using OdixPay.Notifications.Infrastructure.Configuration;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace OdixPay.Notifications.Infrastructure.Services;

public class SmsService : ISmsService
{
    private readonly ILogger<SmsService> _logger;
    private readonly TwilioConfig _twilioConfig;


    public SmsService(ILogger<SmsService> logger, IOptions<TwilioConfig> twilioOptions)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _twilioConfig = twilioOptions.Value ?? throw new ArgumentNullException(nameof(twilioOptions));

        // Initialize Twilio client
        TwilioClient.Init(_twilioConfig.AccountSid, _twilioConfig.AuthToken);
    }

    public async Task<SendNotificationResult> SendSmsAsync(SendNotificationRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            _logger.LogError("SendNotificationRequest is null.");
            throw new ArgumentNullException(nameof(request), "SendNotificationRequest cannot be null.");
        }

        _logger.LogInformation("Sending SMS to {To} from {From}", request.Recipient, request.Sender);

        var to = request.Recipient;
        var from = request.Sender ?? _twilioConfig.DefaultSender;
        var message = request.Message ?? "No Message";

        try
        {
            var sentMessage = await MessageResource.CreateAsync(
                body: message,
                // from: new PhoneNumber(from),
                messagingServiceSid: _twilioConfig.ServiceId, // Use Messaging Service SID if available
                to: new PhoneNumber(to)
            );

            if (sentMessage.Status != MessageResource.StatusEnum.Failed && sentMessage.Status != MessageResource.StatusEnum.Canceled)
            {
                // Handle successful message delivery
                _logger.LogInformation("SMS sent successfully to {To} with SID: {Sid}, with status: {Status}", to, sentMessage.Sid, sentMessage.Status);
                return new SendNotificationResult
                {
                    Success = true,
                    ExternalId = sentMessage.Sid,
                    SentAt = DateTime.UtcNow
                };
            }

            // Handle failure
            _logger.LogError("Failed to send SMS to {To}. Status: {Status}", to, sentMessage.Status);
            return new SendNotificationResult
            {
                Success = false,
                ErrorMessage = $"Failed to send SMS. Status: {sentMessage.Status}",
                SentAt = DateTime.UtcNow
            };


        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS to {To}", to);
            return new SendNotificationResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                SentAt = DateTime.UtcNow
            };
        }
    }
}