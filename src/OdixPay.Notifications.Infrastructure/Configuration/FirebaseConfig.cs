namespace OdixPay.Notifications.Infrastructure.Configuration;

public class FirebaseConfig
{
    public string ServiceAccountKeyPath { get; set; }
    public string ProjectId { get; set; }
    public string DefaultIconUrl { get; set; }
    public string DefaultClickAction { get; set; }
    public bool ValidateOnly { get; set; }
}
