-- Create database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'ShahdCooperative')
BEGIN
    CREATE DATABASE ShahdCooperative;
END
GO

USE ShahdCooperative;
GO

-- Create schemas
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Security')
BEGIN
    EXEC('CREATE SCHEMA Security');
END
GO

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Notification')
BEGIN
    EXEC('CREATE SCHEMA Notification');
END
GO

-- Create Security.Users table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users' AND schema_id = SCHEMA_ID('Security'))
BEGIN
    CREATE TABLE Security.Users (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        Email NVARCHAR(255) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(MAX) NOT NULL,
        PasswordSalt NVARCHAR(MAX) NOT NULL,
        Role NVARCHAR(50) NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
END
GO

-- Create Notification.NotificationTemplates table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'NotificationTemplates' AND schema_id = SCHEMA_ID('Notification'))
BEGIN
    CREATE TABLE Notification.NotificationTemplates (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        [Key] NVARCHAR(100) NOT NULL UNIQUE,
        Name NVARCHAR(255) NOT NULL,
        [Type] NVARCHAR(50) NOT NULL,
        Subject NVARCHAR(500) NULL,
        Body NVARCHAR(MAX) NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        IsDeleted BIT NOT NULL DEFAULT 0,
        CONSTRAINT CK_NotificationTemplates_Type CHECK ([Type] IN ('Email', 'SMS', 'Push', 'InApp'))
    );
END
GO

-- Create Notification.NotificationQueue table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'NotificationQueue' AND schema_id = SCHEMA_ID('Notification'))
BEGIN
    CREATE TABLE Notification.NotificationQueue (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        NotificationType NVARCHAR(50) NOT NULL,
        Recipient NVARCHAR(255) NOT NULL,
        Subject NVARCHAR(500) NULL,
        Body NVARCHAR(MAX) NOT NULL,
        TemplateKey NVARCHAR(100) NULL,
        TemplateData NVARCHAR(MAX) NULL,
        Priority NVARCHAR(50) NOT NULL,
        Status NVARCHAR(50) NOT NULL,
        ScheduledFor DATETIME2 NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        ProcessedAt DATETIME2 NULL,
        RetryCount INT NOT NULL DEFAULT 0,
        ErrorMessage NVARCHAR(MAX) NULL,
        CONSTRAINT CK_NotificationQueue_NotificationType CHECK (NotificationType IN ('Email', 'SMS', 'Push', 'InApp')),
        CONSTRAINT CK_NotificationQueue_Priority CHECK (Priority IN ('Low', 'Normal', 'High', 'Urgent')),
        CONSTRAINT CK_NotificationQueue_Status CHECK (Status IN ('Pending', 'Scheduled', 'Processing', 'Sent', 'Failed', 'Cancelled'))
    );
END
GO

-- Create Notification.NotificationLogs table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'NotificationLogs' AND schema_id = SCHEMA_ID('Notification'))
BEGIN
    CREATE TABLE Notification.NotificationLogs (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        UserId UNIQUEIDENTIFIER NULL,
        RecipientEmail NVARCHAR(255) NULL,
        RecipientPhone NVARCHAR(50) NULL,
        [Type] NVARCHAR(50) NOT NULL,
        Subject NVARCHAR(500) NULL,
        Message NVARCHAR(MAX) NOT NULL,
        Status NVARCHAR(50) NOT NULL,
        SentAt DATETIME2 NULL,
        ErrorMessage NVARCHAR(MAX) NULL,
        RetryCount INT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT CK_NotificationLogs_Type CHECK ([Type] IN ('Email', 'SMS', 'Push', 'InApp')),
        CONSTRAINT CK_NotificationLogs_Status CHECK (Status IN ('Pending', 'Sent', 'Failed', 'Cancelled'))
    );
END
GO

-- Create Notification.NotificationPreferences table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'NotificationPreferences' AND schema_id = SCHEMA_ID('Notification'))
BEGIN
    CREATE TABLE Notification.NotificationPreferences (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        UserId UNIQUEIDENTIFIER NOT NULL,
        NotificationType NVARCHAR(50) NOT NULL,
        IsEnabled BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_NotificationPreferences_Users FOREIGN KEY (UserId) REFERENCES Security.Users(Id) ON DELETE CASCADE,
        CONSTRAINT CK_NotificationPreferences_Type CHECK (NotificationType IN ('Email', 'SMS', 'Push', 'InApp')),
        CONSTRAINT UQ_NotificationPreferences_UserType UNIQUE (UserId, NotificationType)
    );
END
GO

-- Create Notification.InAppNotifications table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'InAppNotifications' AND schema_id = SCHEMA_ID('Notification'))
BEGIN
    CREATE TABLE Notification.InAppNotifications (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        UserId UNIQUEIDENTIFIER NOT NULL,
        Title NVARCHAR(255) NOT NULL,
        Message NVARCHAR(MAX) NOT NULL,
        [Type] NVARCHAR(50) NOT NULL,
        IsRead BIT NOT NULL DEFAULT 0,
        ReadAt DATETIME2 NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        ExpiresAt DATETIME2 NULL,
        CONSTRAINT FK_InAppNotifications_Users FOREIGN KEY (UserId) REFERENCES Security.Users(Id) ON DELETE CASCADE,
        CONSTRAINT CK_InAppNotifications_Type CHECK ([Type] IN ('Info', 'Warning', 'Error', 'Success'))
    );
END
GO

-- Create Notification.DeviceTokens table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DeviceTokens' AND schema_id = SCHEMA_ID('Notification'))
BEGIN
    CREATE TABLE Notification.DeviceTokens (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        UserId UNIQUEIDENTIFIER NOT NULL,
        Token NVARCHAR(500) NOT NULL UNIQUE,
        DeviceType NVARCHAR(50) NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_DeviceTokens_Users FOREIGN KEY (UserId) REFERENCES Security.Users(Id) ON DELETE CASCADE,
        CONSTRAINT CK_DeviceTokens_DeviceType CHECK (DeviceType IN ('iOS', 'Android', 'Web'))
    );
END
GO

PRINT 'Database schema initialized successfully';
