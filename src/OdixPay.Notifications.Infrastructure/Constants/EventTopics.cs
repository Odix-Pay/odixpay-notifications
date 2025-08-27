namespace OdixPay.Notifications.Infrastructure.Constants;

public static class EventTopics
{
    public partial class NotificationEvents
    {
        public const string CreateNotification = "notification.create";
        public const string CreateNotificationMany = "notification.create.many";
        public const string SendNotification = "notification.send";
        public const string MarkAsRead = "notification.markasread";
        public const string CreateTemplate = "template.create";

        public class Subscriptions
        {
            public const string Card3dOtp = "card.card3dotp"; // interlace card otp
            public const string Card3dForwardingOtp = "card.card3d.forwarding.otp"; // interlace card otp
            public const string InterlaceTransactionStatusChanged = "notifications.transaction.status";
            public const string InterlaceCardStatusChanged = "notifications.card.status";
            public const string InterlaceCardholderStatusChanged = "notifications.cardholder.status";

            public const string EmailVerified = "user.email.verified";
            public const string EmailDeleted = "user.email.deleted";
            public const string EmailUpdated = "user.email.updated";
            public const string PhoneVerified = "user.phone.verified";
            public const string PhoneDeleted = "user.phone.deleted";
            public const string PhoneUpdated = "user.phone.updated";
        }
    }
}
