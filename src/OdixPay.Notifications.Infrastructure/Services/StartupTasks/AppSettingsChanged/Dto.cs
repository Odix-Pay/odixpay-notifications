namespace OdixPay.Notifications.Infrastructure.Services.StartupTasks.AppSettingsChanged;

public class AppSettingsChangedEvent
{
    public Guid CurrencyId { get; set; }
    public string UserId { get; set; }
    public string CurrencyName { get; set; }
    public string CurrencyImage { get; set; }
    public string CryptoType { get; set; }
    public Guid ThemeId { get; set; }
    public string ThemeName { get; set; }
    public int LanguageId { get; set; }
    public string LanguageName { get; set; }
    public string LanguageSlug { get; set; }
    public string LanguageISO2 { get; set; }
    public string LanguageISO3 { get; set; }
    public bool IsBiometricEnabled { get; set; }
    public bool RememberMeEnabled { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? CreatedDate { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
}