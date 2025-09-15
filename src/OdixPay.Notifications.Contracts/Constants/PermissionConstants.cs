namespace OdixPay.Notifications.API.Constants;

public static class Permissions
{
    public static class Notification
    {
        public const string Create = "NOTIFICATION_WRITE";
        public const string Read = "NOTIFICATION_READ";
        public const string ReadAdminNotifications = "ADMIN_NOTIFICATION_READ";
        public const string Update = "NOTIFICATION_UPDATE";
        public const string Delete = "NOTIFICATION_DELETE";
    }
    public static class Template
    {
        public const string Create = "TEMPLATE_WRITE";
        public const string Read = "TEMPLATE_READ";
        public const string Update = "TEMPLATE_UPDATE";
        public const string Delete = "TEMPLATE_DELETE";
    }

    public static class Recipient
    {
        public const string Create = "NOTIFICATION_RECIPIENT_WRITE";
        public const string Read = "NOTIFICATION_RECIPIENT_READ";
        public const string Update = "NOTIFICATION_RECIPIENT_UPDATE";
        public const string Delete = "NOTIFICATION_RECIPIENT_DELETE";
    }
}