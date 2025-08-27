namespace OdixPay.Notifications.Infrastructure.Services.StartupTasks.EmailAndPhoneVerifiedEvents;

public class UserDataChangedEvent
{
    public string RoleId { get; set; }
    public string RoleName { get; set; }
    public string UserId { get; set; }
    public string UserName { get; set; }
    public string EmailAddress { get; set; }
    public bool IsEmailConfirmed { get; set; }
    public string PhoneNumber { get; set; }
    public bool IsPhoneNumberConfirmed { get; set; }
    public bool IsAuthPinConfirmed { get; set; }
    public bool IsBiometricsConfirmed { get; set; }
    public int? LoginProviderId { get; set; }
    public string? LoginProviderName { get; set; }
    public string? MFAToken { get; set; }
    public bool? IsMFAEnabled { get; set; } = false;
    public string? GoogleProviderId { get; set; }
    public string? AppleProviderId { get; set; }
    public string? FaceTokenId { get; set; }
    public int? CountryId { get; set; }
    public string? CountryName { get; set; }
    public int? RegionId { get; set; }
    public string? RegionName { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? CreatedDate { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsKYCConfirmed { get; set; }
    public string? InterlaceCardholderId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? MiddleName { get; set; }
    public bool IsBlocked { get; set; }
    public string ReasonofBlock { get; set; }
    public string? ProfilePhotoURL { get; set; }
    public string? OdixPayID { get; set; }
    public string? LastActiveDateTime { get; set; }
    public List<object> Permissions { get; set; }
}