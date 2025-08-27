USE ODIXPAY_NOTIFICATIONS;
GO

-- Stored Procedure: Create Notification
CREATE OR ALTER PROCEDURE notifications.sp_CreateNotification
    @Id UNIQUEIDENTIFIER = NULL,
    @UserId NVARCHAR(100) = NULL,
    @Type INT,
    @Title NVARCHAR(200),
    @Message NVARCHAR(MAX),
    @Data NVARCHAR(MAX) = NULL,
    @Priority INT = 1,
    @Recipient NVARCHAR(200),
    @CreatedAt DATETIME2 = NULL,
    @Sender NVARCHAR(100) = NULL, -- Optional sender field
    @ScheduledAt DATETIME2 = NULL,
    @MaxRetries INT = NULL,
    @TemplateId UNIQUEIDENTIFIER = NULL, -- Foreign key to Notification Template
    @TemplateVariables NVARCHAR(MAX) = NULL -- JSON or XML to store dynamic variables for the template
AS
BEGIN
    SET NOCOUNT ON;
    IF @Id IS NULL
        SET @Id = NEWID(); -- Generate a new ID if not provided
    IF @CreatedAt IS NULL
        SET @CreatedAt = GETUTCDATE(); -- Default to current UTC time if not provided
    IF @ScheduledAt IS NULL
        SET @ScheduledAt = GETUTCDATE(); -- Default to current UTC time if not provided

    
    INSERT INTO notifications.TBL_Notifications (
        Id,
        UserId,
        Type,
        Title,
        Message,
        Data,
        Priority,
        Recipient,
        CreatedAt,
        Sender,
        ScheduledAt,
        MaxRetries,
        TemplateId,
        TemplateVariables
    )
    VALUES (
        @Id,
        @UserId,
        @Type,
        @Title,
        @Message,
        @Data,
        @Priority,
        @Recipient,
        @CreatedAt,
        @Sender,
        @ScheduledAt,
        @MaxRetries,
        @TemplateId,
        @TemplateVariables
    );
    -- Return the newly created notification.
    SELECT * FROM notifications.TBL_Notifications WHERE Id = @Id;
END
GO

-- Stored Procedure: Get Notification by ID
CREATE OR ALTER PROCEDURE notifications.sp_GetNotificationById
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT *
    FROM notifications.TBL_Notifications
    WHERE Id = @Id;
END
GO

-- Stored Procedure: Get Notifications count
CREATE OR ALTER PROCEDURE notifications.sp_GetNotificationCount
    -- Query Parameters
    @UserId NVARCHAR(100) = NULL,
    @Status INT = NULL,
    @Type INT = NULL,
    @Id UNIQUEIDENTIFIER = NULL,
    @Priority INT = NULL,
    @Recipient NVARCHAR(100) = NULL,
    @Sender NVARCHAR(100) = NULL,
    @TemplateId UNIQUEIDENTIFIER = NULL,
    @Search NVARCHAR(200) = NULL,
    @IsRead BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT COUNT(*) AS TotalCount
    FROM notifications.TBL_Notifications
    WHERE (@UserId IS NULL OR UserId = @UserId)
      AND (@Status IS NULL OR [Status] = @Status)
      AND (@Type IS NULL OR [Type] = @Type)
      AND (@Id IS NULL OR Id = @Id)
      AND (@Priority IS NULL OR [Priority] = @Priority)
      AND (@Recipient IS NULL OR [Recipient] = @Recipient)
      AND (@Sender IS NULL OR [Sender] = @Sender)
      AND (@TemplateId IS NULL OR [TemplateId] = @TemplateId)
      AND (@IsRead IS NULL OR [IsRead] = @IsRead)
      AND (@Search IS NULL OR [Title] LIKE '%' + @Search + '%' OR [Message] LIKE '%' + @Search + '%');
END
GO

-- Stored Procedure: Get Pending Notifications
CREATE OR ALTER PROCEDURE notifications.sp_GetPendingNotifications
    @Page INT = 1,
    @Limit INT = 20
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Offset INT = (@Page - 1) * @Limit;
    
    SELECT *
    FROM notifications.TBL_Notifications
    WHERE [Status] = 1 -- Pending
        AND (ScheduledAt IS NULL OR ScheduledAt <= GETUTCDATE())
        AND RetryCount < MaxRetries
    ORDER BY Priority DESC, CreatedAt ASC --gets higher priority first
    OFFSET @Offset ROWS
    FETCH NEXT @Limit ROWS ONLY;
END
GO

-- Stored Procedure: Update Notification Status
CREATE OR ALTER PROCEDURE notifications.sp_UpdateNotificationStatus
    @Id UNIQUEIDENTIFIER,
    @Status INT,
    @ErrorMessage NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE notifications.TBL_Notifications
    SET [Status] = @Status,
        ErrorMessage = @ErrorMessage
    WHERE Id = @Id;
END
GO

-- Stored Procedure: Update Notification Sent
CREATE OR ALTER PROCEDURE notifications.sp_UpdateNotificationSent
    @Id UNIQUEIDENTIFIER,
    @SentAt DATETIME2 = NULL,
    @ExternalId NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF @SentAt IS NULL
        SET @SentAt = GETUTCDATE(); -- Default to current UTC time if not provided
    
    UPDATE notifications.TBL_Notifications
    SET [Status] = 2, -- Sent
        SentAt = @SentAt,
        ExternalId = @ExternalId
    WHERE Id = @Id;
END
GO

-- Stored Procedure: Update Notification Delivered
CREATE OR ALTER PROCEDURE notifications.sp_UpdateNotificationDelivered
    @Id UNIQUEIDENTIFIER,
    @DeliveredAt DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF @DeliveredAt IS NULL
        SET @DeliveredAt = GETUTCDATE(); -- Default to current UTC time if not provided
    
    UPDATE notifications.TBL_Notifications
    SET [Status] = 3, -- Delivered
        DeliveredAt = @DeliveredAt
    WHERE Id = @Id;
END
GO

-- Stored Procedure: Increment Retry Count
CREATE OR ALTER PROCEDURE notifications.sp_IncrementNotificationRetryCount
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE notifications.TBL_Notifications
    SET RetryCount = RetryCount + 1
    WHERE Id = @Id;
END
GO

-- Stored Procedure: Get Unread Notification Count
CREATE OR ALTER PROCEDURE notifications.sp_GetUnreadNotificationCount
    @UserId NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT COUNT(*)
    FROM notifications.TBL_Notifications
    WHERE UserId = @UserId
        AND IsRead = 0
        AND [Type] = 4; -- InApp notifications only
END
GO

-- Stored Procedure: Mark Notification as Read
CREATE OR ALTER PROCEDURE notifications.sp_MarkNotificationAsRead
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE notifications.TBL_Notifications
    SET IsRead = 1
    WHERE Id = @Id;
END
GO

-- Stored Procedure: Get Notifications Count
CREATE OR ALTER PROCEDURE notifications.sp_GetNotificationCount
    @UserId NVARCHAR(100) = NULL,
    @Status INT = NULL,
    @Type INT = NULL,
    @Id UNIQUEIDENTIFIER = NULL,
    @Priority INT = NULL,
    @Recipient NVARCHAR(100) = NULL,
    @Sender NVARCHAR(100) = NULL,
    @TemplateId UNIQUEIDENTIFIER = NULL,
    @Search NVARCHAR(100) = NULL,
    @IsRead BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT COUNT(*) AS TotalCount
    FROM notifications.TBL_Notifications
    WHERE 
        1=1
        AND (@UserId IS NULL OR UserId = @UserId)
        AND (@Status IS NULL OR [Status] = @Status)
        AND (@Type IS NULL OR [Type] = @Type)
        AND (@Id IS NULL OR Id = @Id)
        AND (@Priority IS NULL OR [Priority] = @Priority)
        AND (@Recipient IS NULL OR [Recipient] = @Recipient)
        AND (@Sender IS NULL OR [Sender] = @Sender)
        AND (@TemplateId IS NULL OR [TemplateId] = @TemplateId)
        AND (@IsRead IS NULL OR [IsRead] = @IsRead)
        AND (@Search IS NULL OR [Title] LIKE '%' + @Search + '%' OR [Message] LIKE '%' + @Search + '%');
END
GO

-- Stored Procedure: Get Notifications
CREATE OR ALTER PROCEDURE notifications.sp_GetNotifications
    @UserId NVARCHAR(100) = NULL,
    @Status INT = NULL,
    @Type INT = NULL,
    @Id UNIQUEIDENTIFIER = NULL,
    @Priority INT = NULL,
    @Recipient NVARCHAR(100) = NULL,
    @Sender NVARCHAR(100) = NULL,
    @TemplateId UNIQUEIDENTIFIER = NULL,
    @Search NVARCHAR(100) = NULL,
    @IsRead BIT = NULL,
    @Page INT = 1,
    @Limit INT = 100
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@Page - 1) * @Limit;

    SELECT *
    FROM notifications.TBL_Notifications
    WHERE 
        1=1
        AND (@UserId IS NULL OR UserId = @UserId)
        AND (@Status IS NULL OR [Status] = @Status)
        AND (@Type IS NULL OR [Type] = @Type)
        AND (@Id IS NULL OR Id = @Id)
        AND (@Priority IS NULL OR [Priority] = @Priority)
        AND (@Recipient IS NULL OR [Recipient] = @Recipient)
        AND (@Sender IS NULL OR [Sender] = @Sender)
        AND (@TemplateId IS NULL OR [TemplateId] = @TemplateId)
        AND (@IsRead IS NULL OR [IsRead] = @IsRead)
        AND (@Search IS NULL OR [Title] LIKE '%' + @Search + '%' OR [Message] LIKE '%' + @Search + '%')
    ORDER BY CreatedAt DESC
    OFFSET @Offset ROWS
    FETCH NEXT @Limit ROWS ONLY;
END
GO