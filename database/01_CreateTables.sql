-- Create Database
CREATE DATABASE ODIXPAY_NOTIFICATIONS;
GO

USE ODIXPAY_NOTIFICATIONS;
GO

-- Create Schema
CREATE SCHEMA notifications;

-- Create Notifications Table
CREATE TABLE notifications.TBL_Notifications (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    UserId NVARCHAR(100) NOT NULL,
    [Type] INT NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    [Message] NVARCHAR(MAX) NOT NULL,
    [Data] NVARCHAR(MAX) NULL,
    [Status] INT NOT NULL DEFAULT 1,
    Priority INT NOT NULL DEFAULT 2,
    Recipient NVARCHAR(200) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0,
    Sender NVARCHAR(100) NULL, -- Optional sender field
    ScheduledAt DATETIME2 NULL,
    SentAt DATETIME2 NULL,
    DeliveredAt DATETIME2 NULL,
    ErrorMessage NVARCHAR(500) NULL,
    RetryCount INT NOT NULL DEFAULT 0,
    MaxRetries INT NOT NULL DEFAULT 3,
    ExternalId NVARCHAR(100) NULL,
    IsRead BIT NOT NULL DEFAULT 0,
    DefaultLocale NVARCHAR(10) NOT NULL DEFAULT "en",
    TemplateId UNIQUEIDENTIFIER NULL, -- Foreign key to Notification Template
    TemplateVariables NVARCHAR(MAX) NULL -- JSON or XML to store dynamic variables for the template
);
GO

-- Create NotificationTemplates Table
CREATE TABLE notifications.TBL_NotificationTemplates (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    [Name] NVARCHAR(100) NOT NULL,
    Slug NVARCHAR(100) NOT NULL,
    [Type] INT NOT NULL,
    Subject NVARCHAR(200) NOT NULL,
    Body NVARCHAR(MAX) NOT NULL,
    Locale NVARCHAR(10) NOT NULL DEFAULT "en",
    Variables NVARCHAR(MAX) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
GO

-- Create NotificationRecipients Table
CREATE TABLE notifications.TBL_NotificationRecipients (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    UserId NVARCHAR(100) NOT NULL,
    [Type] INT NOT NULL,
    Recipient NVARCHAR(MAX) NOT NULL,
    Name NVARCHAR(100) NULL,
    DefaultLanguage NVARCHAR(10) NOT NULL DEFAULT "en",
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0
);

-- Define foreign key constraint if needed
ALTER TABLE notifications.TBL_Notifications
ADD CONSTRAINT FK_Notifications_Template
FOREIGN KEY (TemplateId) REFERENCES notifications.TBL_NotificationTemplates(Id);

-- Create Indexes
CREATE INDEX IX_Notifications_UserId ON notifications.TBL_Notifications(UserId);
CREATE INDEX IX_Notifications_Status ON notifications.TBL_Notifications([Status]);
CREATE INDEX IX_Notifications_CreatedAt ON notifications.TBL_Notifications(CreatedAt);
CREATE INDEX IX_Notifications_ScheduledAt ON notifications.TBL_Notifications(ScheduledAt);
CREATE INDEX IX_NotificationTemplates_Name ON notifications.TBL_NotificationTemplates([Name]);
CREATE INDEX IX_NotificationTemplates_IsActive ON notifications.TBL_NotificationTemplates(IsActive);
CREATE INDEX IX_NotificationRecipients_UserId ON notifications.TBL_NotificationRecipients(UserId);
CREATE INDEX IX_NotificationRecipients_Type ON notifications.TBL_NotificationRecipients([Type]);
CREATE INDEX IX_NotificationRecipients_UserIdType ON notifications.TBL_NotificationRecipients(UserId, [Type]);

-- Create unique index on slug; Name & Locale
CREATE UNIQUE INDEX IX_NotificationTemplates_Name_Locale ON notifications.TBL_NotificationTemplates(Name, Locale);
CREATE UNIQUE INDEX IX_NotificationTemplates_Slug_Locale ON notifications.TBL_NotificationTemplates(Slug, Locale);
GO
