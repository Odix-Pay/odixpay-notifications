namespace OdixPay.Notifications.Infrastructure.Constants;

public static class StoredProcedures
{
    public static class Notification
    {
        public const string Create = "notifications.sp_CreateNotification";
        public const string GetById = "notifications.sp_GetNotificationById";
        public const string GetByUserId = "notifications.sp_GetNotificationsByUserId";
        public const string GetPending = "notifications.sp_GetPendingNotifications";
        public const string UpdateStatus = "notifications.sp_UpdateNotificationStatus";
        public const string UpdateSent = "notifications.sp_UpdateNotificationSent";
        public const string UpdateDelivered = "notifications.sp_UpdateNotificationDelivered";
        public const string IncrementRetryCount = "notifications.sp_IncrementNotificationRetryCount";
        public const string GetUnreadCount = "notifications.sp_GetUnreadNotificationCount";
        public const string MarkAsRead = "notifications.sp_MarkNotificationAsRead";
        public const string MarkAllAsRead = "notifications.sp_MarkAllNotificationsAsRead";
        public const string Count = "notifications.sp_GetNotificationCount";
        public const string Query = "notifications.sp_GetNotifications";
    }

    public static class Template
    {
        public const string Create = "notifications.sp_CreateNotificationTemplate";
        public const string GetById = "notifications.sp_GetNotificationTemplateById";
        public const string GetBySlug = "notifications.sp_GetNotificationTemplateBySlug";
        public const string GetActive = "notifications.sp_GetActiveNotificationTemplates";
        public const string Update = "notifications.sp_UpdateNotificationTemplate";
        public const string Delete = "notifications.sp_DeleteNotificationTemplate";
        public const string GetTemplatesCount = "notifications.sp_GetNotificationTemplatesCount";
        public const string GetTemplates = "notifications.sp_GetNotificationTemplates";
    }

    public static class NotificationRecipient
    {
        public const string Create = "notifications.sp_AddNotificationRecipient";
        public const string GetById = "notifications.sp_GetNotificationRecipientById";
        public const string GetByUserIdAndType = "notifications.sp_GetNotificationRecipientsByUserIdAndType";
        public const string Update = "notifications.sp_UpdateNotificationRecipient";
        public const string UpdateLanguage = "notifications.sp_UpdateNotificationRecipientLanguage";
        public const string Delete = "notifications.sp_DeleteNotificationRecipient";
        public const string Query = "notifications.sp_QueryNotificationRecipients";
        public const string Count = "notifications.sp_GetNotificationRecipientsCount";
    }
}