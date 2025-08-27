namespace OdixPay.Notifications.Infrastructure.Configuration;

public class TwilioConfig
{
    public string Url { get; set; }
    public string AccountSid { get; set; }
    public string AuthToken { get; set; }
    public string ServiceId { get; set; }
    public string Channel { get; set; }
    public string Locale { get; set; }
    public string DefaultSenderName { get; set; }
    public string DefaultSender { get; set; }
}