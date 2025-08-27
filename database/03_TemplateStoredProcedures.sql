USE ODIXPAY_NOTIFICATIONS;
GO

-- Stored Procedure: Create Notification Template
CREATE OR ALTER PROCEDURE notifications.sp_CreateNotificationTemplate
    @Id UNIQUEIDENTIFIER = NULL,
    @Name NVARCHAR(100),
    @Slug NVARCHAR(100),
    @Type INT,
    @Subject NVARCHAR(200),
    @Body NVARCHAR(MAX),
    @Variables NVARCHAR(MAX) = NULL,
    @IsActive BIT = 1,
    @CreatedAt DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @Id IS NULL
        SET @Id = NEWID(); -- Generate a new ID if not provided

    IF @CreatedAt IS NULL
        SET @CreatedAt = GETUTCDATE(); -- Default to current UTC time if not provided
    
    INSERT INTO notifications.TBL_NotificationTemplates (
        Id, [Name], [Slug], [Type], Subject, Body, Variables, IsActive, CreatedAt
    )
    VALUES (
        @Id, @Name, @Slug, @Type, @Subject, @Body, @Variables, @IsActive, @CreatedAt
    );

    -- Return the created template
    SELECT *
    FROM notifications.TBL_NotificationTemplates
    WHERE Id = @Id;    
END
GO

-- Stored Procedure: Get Notification Template by ID
CREATE OR ALTER PROCEDURE notifications.sp_GetNotificationTemplateById
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT *
    FROM notifications.TBL_NotificationTemplates
    WHERE Id = @Id;
END
GO

-- Stored Procedure: Get Notification Template by Slug
CREATE OR ALTER PROCEDURE notifications.sp_GetNotificationTemplateBySlug
    @Slug NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT *
    FROM notifications.TBL_NotificationTemplates
    WHERE [Slug] = @Slug;
END
GO

-- Stored Procedure: Get Active Notification Templates
CREATE OR ALTER PROCEDURE notifications.sp_GetActiveNotificationTemplates
    @Page INT = 1,
    @Limit INT = 20
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@Page - 1) * @Limit;
    
    SELECT *
    FROM notifications.TBL_NotificationTemplates
    WHERE IsActive = 1
    ORDER BY [Name]
    OFFSET @Offset ROWS
    FETCH NEXT @Limit ROWS ONLY;
END
GO

-- Stored Procedure: Update Notification Template
CREATE OR ALTER PROCEDURE notifications.sp_UpdateNotificationTemplate
    @Id UNIQUEIDENTIFIER,
    @Name NVARCHAR(100) = NULL,
    @Slug NVARCHAR(100) = NULL,
    @Type INT = NULL,
    @Subject NVARCHAR(200) = NULL,
    @Body NVARCHAR(MAX) = NULL,
    @Variables NVARCHAR(MAX) = NULL,
    @IsActive BIT = NULL,
    @IsDeleted BIT = NULL,
    @UpdatedAt DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @UpdatedAt IS NULL
        SET @UpdatedAt = GETUTCDATE(); -- Default to current UTC time if not provided
    
    UPDATE notifications.TBL_NotificationTemplates
    SET 
        [Name] = COALESCE(@Name, [Name]),
        [Slug] = COALESCE(@Slug, [Slug]),
        [Type] = COALESCE(@Type, [Type]),
        Subject = COALESCE(@Subject, Subject),
        Body = COALESCE(@Body, Body),
        Variables = COALESCE(@Variables, Variables),
        IsActive = COALESCE(@IsActive, IsActive),
        IsDeleted = COALESCE(@IsDeleted, IsDeleted),
        UpdatedAt = @UpdatedAt
    WHERE Id = @Id;
END
GO

-- Stored Procedure: Delete Notification Template
CREATE OR ALTER PROCEDURE notifications.sp_DeleteNotificationTemplate
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    DELETE FROM notifications.TBL_NotificationTemplates
    WHERE Id = @Id;
END
GO

-- Stored Procedure: Get Notification Templates Count
CREATE OR ALTER PROCEDURE notifications.sp_GetNotificationTemplatesCount
    @Id UNIQUEIDENTIFIER = NULL,
    @Type INT = NULL,
    @Name NVARCHAR(100) = NULL,
    @Subject NVARCHAR(200) = NULL,
    @Slug NVARCHAR(100) = NULL,
    @Search NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT COUNT(*)
    FROM notifications.TBL_NotificationTemplates
    WHERE 1=1
      AND (@Id IS NULL OR Id = @Id)
      AND (@Type IS NULL OR [Type] = @Type)
      AND (@Name IS NULL OR [Name] LIKE '%' + @Name + '%')
      AND (@Subject IS NULL OR Subject LIKE '%' + @Subject + '%')
      AND (@Slug IS NULL OR [Slug] = @Slug)
      AND (@Search IS NULL OR (Body LIKE '%' + @Search + '%' OR [Name] LIKE '%' + @Search + '%' OR Subject LIKE '%' + @Search + '%'));
END
GO

-- Stored Procedure: Get Notification Templates
CREATE OR ALTER PROCEDURE notifications.sp_GetNotificationTemplates
    @Id UNIQUEIDENTIFIER = NULL,
    @Type INT = NULL,
    @Name NVARCHAR(100) = NULL,
    @Subject NVARCHAR(200) = NULL,
    @Slug NVARCHAR(100) = NULL,
    @Search NVARCHAR(200) = NULL,
    @Page INT = 1,
    @Limit INT = 20
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@Page - 1) * @Limit;

    SELECT *
    FROM notifications.TBL_NotificationTemplates
      (@Id IS NULL OR Id = @Id)
      AND (@Type IS NULL OR [Type] = @Type)
      AND (@Name IS NULL OR [Name] LIKE '%' + @Name + '%')
      AND (@Subject IS NULL OR Subject LIKE '%' + @Subject + '%')
      AND (@Slug IS NULL OR [Slug] = @Slug)
      AND (@Search IS NULL OR (Body LIKE '%' + @Search + '%' OR [Name] LIKE '%' + @Search + '%' OR Subject LIKE '%' + @Search + '%'))
    ORDER BY [Name]
    OFFSET @Offset ROWS
    FETCH NEXT @Limit ROWS ONLY;
END
GO