using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using ShahdCooperative.NotificationService.Domain.Interfaces;

namespace ShahdCooperative.NotificationService.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"NotificationServiceTestDb_{Guid.NewGuid()}";
    private string _connectionString = string.Empty;

    public string ConnectionString => _connectionString;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Build test connection string using LocalDB
            var testConnectionString = $"Server=(localdb)\\mssqllocaldb;Database={_databaseName};Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true";

            _connectionString = testConnectionString;

            // Override configuration
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = testConnectionString,
                ["RabbitMQ:HostName"] = "localhost-test",
                ["RabbitMQ:UserName"] = "guest",
                ["RabbitMQ:Password"] = "guest",
                ["EmailSettings:Provider"] = "mock",
                ["SmsSettings:Provider"] = "mock",
                ["PushNotificationSettings:FirebaseCredentialsPath"] = ""
            }!);
        });

        builder.ConfigureServices(services =>
        {
            // Remove background services for testing
            services.RemoveAll<IHostedService>();

            // Ensure mock notification senders are used
            services.RemoveAll<INotificationSender>();
        });
    }

    public async Task InitializeDatabaseAsync()
    {
        // First create the database if it doesn't exist (connect to master)
        var masterConnectionString = _connectionString.Replace($"Database={_databaseName}", "Database=master");
        using var masterConnection = new SqlConnection(masterConnectionString);
        await masterConnection.OpenAsync();

        await using var createDbCommand = masterConnection.CreateCommand();
        createDbCommand.CommandText = $@"
            IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'{_databaseName}')
            BEGIN
                CREATE DATABASE [{_databaseName}]
            END";
        await createDbCommand.ExecuteNonQueryAsync();

        // Now connect to the newly created database
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        // Create schema
        await using var schemaCommand = connection.CreateCommand();
        schemaCommand.CommandText = "IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Notification') EXEC('CREATE SCHEMA Notification')";
        await schemaCommand.ExecuteNonQueryAsync();

        // Create NotificationTemplates table
        await using var templatesCommand = connection.CreateCommand();
        templatesCommand.CommandText = @"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Notification].[NotificationTemplates]') AND type in (N'U'))
            BEGIN
                CREATE TABLE [Notification].[NotificationTemplates] (
                    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
                    [TemplateKey] NVARCHAR(100) NOT NULL UNIQUE,
                    [TemplateName] NVARCHAR(200) NOT NULL,
                    [Subject] NVARCHAR(500),
                    [BodyTemplate] NVARCHAR(MAX) NOT NULL,
                    [NotificationType] NVARCHAR(50) NOT NULL,
                    [IsActive] BIT NOT NULL DEFAULT 1,
                    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                    [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
                )
            END";
        await templatesCommand.ExecuteNonQueryAsync();

        // Create NotificationQueue table
        await using var queueCommand = connection.CreateCommand();
        queueCommand.CommandText = @"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Notification].[NotificationQueue]') AND type in (N'U'))
            BEGIN
                CREATE TABLE [Notification].[NotificationQueue] (
                    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
                    [NotificationType] NVARCHAR(50) NOT NULL,
                    [Recipient] NVARCHAR(500) NOT NULL,
                    [Subject] NVARCHAR(500),
                    [Body] NVARCHAR(MAX) NOT NULL,
                    [Priority] INT NOT NULL DEFAULT 0,
                    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Pending',
                    [RetryCount] INT NOT NULL DEFAULT 0,
                    [MaxRetries] INT NOT NULL DEFAULT 3,
                    [ScheduledFor] DATETIME2,
                    [ProcessedAt] DATETIME2,
                    [ErrorMessage] NVARCHAR(MAX),
                    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                    [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
                )
            END";
        await queueCommand.ExecuteNonQueryAsync();

        // Create InAppNotifications table
        await using var inAppCommand = connection.CreateCommand();
        inAppCommand.CommandText = @"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Notification].[InAppNotifications]') AND type in (N'U'))
            BEGIN
                CREATE TABLE [Notification].[InAppNotifications] (
                    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
                    [UserId] UNIQUEIDENTIFIER NOT NULL,
                    [Title] NVARCHAR(200) NOT NULL,
                    [Message] NVARCHAR(MAX) NOT NULL,
                    [Type] NVARCHAR(50) NOT NULL,
                    [IsRead] BIT NOT NULL DEFAULT 0,
                    [ReadAt] DATETIME2,
                    [ActionUrl] NVARCHAR(500),
                    [ExpiresAt] DATETIME2,
                    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
                )
            END";
        await inAppCommand.ExecuteNonQueryAsync();

        // Create NotificationPreferences table
        await using var preferencesCommand = connection.CreateCommand();
        preferencesCommand.CommandText = @"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Notification].[NotificationPreferences]') AND type in (N'U'))
            BEGIN
                CREATE TABLE [Notification].[NotificationPreferences] (
                    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
                    [UserId] UNIQUEIDENTIFIER NOT NULL UNIQUE,
                    [EmailNotifications] BIT NOT NULL DEFAULT 1,
                    [SmsNotifications] BIT NOT NULL DEFAULT 1,
                    [PushNotifications] BIT NOT NULL DEFAULT 1,
                    [InAppNotifications] BIT NOT NULL DEFAULT 1,
                    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                    [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
                )
            END";
        await preferencesCommand.ExecuteNonQueryAsync();

        // Create NotificationLogs table
        await using var logsCommand = connection.CreateCommand();
        logsCommand.CommandText = @"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Notification].[NotificationLogs]') AND type in (N'U'))
            BEGIN
                CREATE TABLE [Notification].[NotificationLogs] (
                    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
                    [NotificationType] NVARCHAR(50) NOT NULL,
                    [Recipient] NVARCHAR(500) NOT NULL,
                    [Subject] NVARCHAR(500),
                    [Body] NVARCHAR(MAX),
                    [Status] NVARCHAR(50) NOT NULL,
                    [SentAt] DATETIME2,
                    [ErrorMessage] NVARCHAR(MAX),
                    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
                )
            END";
        await logsCommand.ExecuteNonQueryAsync();
    }

    public async Task CleanupDatabaseAsync()
    {
        try
        {
            using var connection = new SqlConnection(_connectionString.Replace(_databaseName, "master"));
            await connection.OpenAsync();

            await using var command = connection.CreateCommand();
            command.CommandText = $@"
                IF EXISTS (SELECT name FROM sys.databases WHERE name = N'{_databaseName}')
                BEGIN
                    ALTER DATABASE [{_databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                    DROP DATABASE [{_databaseName}];
                END";
            await command.ExecuteNonQueryAsync();
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            CleanupDatabaseAsync().GetAwaiter().GetResult();
        }
        base.Dispose(disposing);
    }
}
