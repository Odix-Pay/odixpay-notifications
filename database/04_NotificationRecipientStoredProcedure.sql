USE ODIXPAY_NOTIFICATIONS;
GO

-- Stored Procedure: Add Notification Recipient
CREATE OR ALTER PROCEDURE notifications.sp_AddNotificationRecipient
    @Id UNIQUEIDENTIFIER = NULL,
    @Name NVARCHAR(100) = NULL,
    @UserId NVARCHAR(100),
    @Type INT,
    @Recipient NVARCHAR(MAX),
    @IsActive BIT = 1,
    @DefaultLanguage NVARCHAR(10) = "en"
AS
BEGIN
    SET NOCOUNT ON;
    -- Set Id if not passed
    IF @Id IS NULL
    BEGIN
        SET @Id = NEWID();
    END

    INSERT INTO notifications.TBL_NotificationRecipients (Id, Name, UserId, [Type], Recipient, IsActive, DefaultLanguage)
    VALUES (@Id, @Name, @UserId, @Type, @Recipient, @IsActive, @DefaultLanguage);
END
GO

-- Stored Procedure: Get Notification Recipient By Id
CREATE OR ALTER PROCEDURE notifications.sp_GetNotificationRecipientById
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM notifications.TBL_NotificationRecipients
    WHERE Id = @Id;
END
GO

-- Stored Procedure: Query Notification Recipients
CREATE OR ALTER PROCEDURE notifications.sp_QueryNotificationRecipients
    @Id UNIQUEIDENTIFIER = NULL,
    @UserId NVARCHAR(100) = NULL,
    @Type INT = NULL,
    @Search NVARCHAR(100) = NULL,
    @IsActive BIT = NULL,
    @IsDeleted BIT = NULL,
    @DefaultLanguage NVARCHAR(10) = NULL,
    @Page INT = 1,
    @Limit INT = 100
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@Page - 1) * @Limit;

    SELECT *
    FROM notifications.TBL_NotificationRecipients
    WHERE 
        1=1
        AND (@UserId IS NULL OR UserId = @UserId)
        AND (@Type IS NULL OR [Type] = @Type)
        AND (@IsActive IS NULL OR [IsActive] = @IsActive)
        AND (@IsDeleted IS NULL OR [IsDeleted] = @IsDeleted)
        AND (@DefaultLanguage IS NULL OR DefaultLanguage = @DefaultLanguage)
        AND (@Search IS NULL OR [Recipient] LIKE '%' + @Search + '%' OR [Name] LIKE '%' + @Search + '%' OR DefaultLanguage LIKE '%' + @Search + '%')
    ORDER BY CreatedAt DESC
    OFFSET @Offset ROWS
    FETCH NEXT @Limit ROWS ONLY;
END
GO

-- Stored Procedure: Delete Notification Recipient
CREATE OR ALTER PROCEDURE notifications.sp_DeleteNotificationRecipient
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM notifications.TBL_NotificationRecipients
    WHERE Id = @Id;
END
GO

-- Stored Procedure: Update Notification Recipient
CREATE OR ALTER PROCEDURE notifications.sp_UpdateNotificationRecipient
    @Id UNIQUEIDENTIFIER,
    @Name NVARCHAR(100) = NULL,
    @UserId NVARCHAR(100) = NULL,
    @Recipient NVARCHAR(MAX) = NULL,
    @IsActive BIT = NULL,
    @IsDeleted BIT = NULL,
    @DefaultLanguage NVARCHAR(10) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE notifications.TBL_NotificationRecipients
    SET
        Name = COALESCE(@Name, Name),
        UserId = COALESCE(@UserId, UserId),
        Recipient = COALESCE(@Recipient, Recipient),
        IsActive = COALESCE(@IsActive, IsActive),
        IsDeleted = COALESCE(@IsDeleted, IsDeleted),
        DefaultLanguage = COALESCE(@DefaultLanguage, DefaultLanguage)
    WHERE Id = @Id;
END
GO

-- Stored Procedure: Get Notification Recipient By UserId and Type
CREATE OR ALTER PROCEDURE notifications.sp_GetNotificationRecipientsByUserIdAndType
    @UserId NVARCHAR(100),
    @Type INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1 *
    FROM notifications.TBL_NotificationRecipients
    WHERE UserId = @UserId AND [Type] = @Type;
END
GO

-- Stored Procedure: Count Notification Recipients
CREATE OR ALTER PROCEDURE notifications.sp_GetNotificationRecipientsCount
    @Id UNIQUEIDENTIFIER = NULL,
    @UserId NVARCHAR(100) = NULL,
    @Type INT = NULL,
    @Search NVARCHAR(100) = NULL,
    @IsActive BIT = NULL,
    @IsDeleted BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT COUNT(*)
    FROM notifications.TBL_NotificationRecipients
    WHERE 
        (@UserId IS NULL OR UserId = @UserId)
        AND (@Type IS NULL OR [Type] = @Type)
        AND (@Search IS NULL OR [Recipient] LIKE '%' + @Search + '%' OR [Name] LIKE '%' + @Search + '%')
        AND (@IsActive IS NULL OR [IsActive] = @IsActive)
        AND (@IsDeleted IS NULL OR [IsDeleted] = @IsDeleted);
END
GO

-- Stored Procedure: Update Notification Recipient Language
CREATE OR ALTER PROCEDURE notifications.sp_UpdateNotificationRecipientLanguage
    @UserId NVARCHAR(100),
    @DefaultLanguage NVARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE notifications.TBL_NotificationRecipients
    SET DefaultLanguage = @DefaultLanguage
    WHERE UserId = @UserId;
END
GO  