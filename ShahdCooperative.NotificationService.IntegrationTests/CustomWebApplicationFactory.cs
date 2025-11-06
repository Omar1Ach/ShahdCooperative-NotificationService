using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using ShahdCooperative.NotificationService.Domain.Interfaces;
using Testcontainers.MsSql;

namespace ShahdCooperative.NotificationService.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _msSqlContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2019-latest")
        .WithPassword("YourStrong@Passw0rd123")
        .Build();

    private string _connectionString = string.Empty;

    public string ConnectionString => _connectionString;

    async Task IAsyncLifetime.InitializeAsync()
    {
        await _msSqlContainer.StartAsync();
        _connectionString = _msSqlContainer.GetConnectionString();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _msSqlContainer.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Override configuration to use existing SQL Server database
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _connectionString,
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
        // Initialize database schema
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        // Run the initialization script
        await InitializeDatabaseSchemaAsync(connection);

        // Clean all tables before running tests for test isolation
        await CleanDatabaseTablesAsync(connection);

        // Create test users if they don't exist (needed for foreign keys)
        await CreateTestUsersAsync(connection);
    }

    private async Task InitializeDatabaseSchemaAsync(SqlConnection connection)
    {
        var scriptPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "scripts", "init-db.sql");

        if (!File.Exists(scriptPath))
        {
            // Fallback: create schemas manually
            await CreateSchemasManuallyAsync(connection);
            return;
        }

        var script = await File.ReadAllTextAsync(scriptPath);
        var batches = script.Split(new[] { "GO", "go" }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var batch in batches)
        {
            if (string.IsNullOrWhiteSpace(batch)) continue;

            try
            {
                await using var command = connection.CreateCommand();
                command.CommandText = batch;
                await command.ExecuteNonQueryAsync();
            }
            catch
            {
                // Ignore errors (schema might already exist)
            }
        }
    }

    private async Task CreateSchemasManuallyAsync(SqlConnection connection)
    {
        var commands = new[]
        {
            "IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Security') EXEC('CREATE SCHEMA Security')",
            "IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Notification') EXEC('CREATE SCHEMA Notification')",
            @"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users' AND schema_id = SCHEMA_ID('Security'))
              CREATE TABLE Security.Users (
                  Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
                  Email NVARCHAR(255) NOT NULL UNIQUE,
                  PasswordHash NVARCHAR(MAX) NOT NULL,
                  PasswordSalt NVARCHAR(MAX) NOT NULL,
                  Role NVARCHAR(50) NOT NULL,
                  CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                  UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
              )",
            @"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'NotificationTemplates' AND schema_id = SCHEMA_ID('Notification'))
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
                  IsDeleted BIT NOT NULL DEFAULT 0
              )",
            @"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'NotificationQueue' AND schema_id = SCHEMA_ID('Notification'))
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
                  ErrorMessage NVARCHAR(MAX) NULL
              )",
            @"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'NotificationLogs' AND schema_id = SCHEMA_ID('Notification'))
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
                  UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                  IsDeleted BIT NOT NULL DEFAULT 0
              )",
            @"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'NotificationPreferences' AND schema_id = SCHEMA_ID('Notification'))
              CREATE TABLE Notification.NotificationPreferences (
                  Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
                  UserId UNIQUEIDENTIFIER NOT NULL,
                  NotificationType NVARCHAR(50) NOT NULL,
                  IsEnabled BIT NOT NULL DEFAULT 1,
                  CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                  UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                  CONSTRAINT FK_NotificationPreferences_Users FOREIGN KEY (UserId) REFERENCES Security.Users(Id) ON DELETE CASCADE,
                  CONSTRAINT UQ_NotificationPreferences_UserType UNIQUE (UserId, NotificationType)
              )",
            @"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'InAppNotifications' AND schema_id = SCHEMA_ID('Notification'))
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
                  CONSTRAINT FK_InAppNotifications_Users FOREIGN KEY (UserId) REFERENCES Security.Users(Id) ON DELETE CASCADE
              )",
            @"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DeviceTokens' AND schema_id = SCHEMA_ID('Notification'))
              CREATE TABLE Notification.DeviceTokens (
                  Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
                  UserId UNIQUEIDENTIFIER NOT NULL,
                  Token NVARCHAR(500) NOT NULL UNIQUE,
                  DeviceType NVARCHAR(50) NOT NULL,
                  IsActive BIT NOT NULL DEFAULT 1,
                  CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                  UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                  CONSTRAINT FK_DeviceTokens_Users FOREIGN KEY (UserId) REFERENCES Security.Users(Id) ON DELETE CASCADE
              )"
        };

        foreach (var commandText in commands)
        {
            try
            {
                await using var command = connection.CreateCommand();
                command.CommandText = commandText;
                await command.ExecuteNonQueryAsync();
            }
            catch
            {
                // Ignore errors (objects might already exist)
            }
        }
    }

    private async Task CreateTestUsersAsync(SqlConnection connection)
    {
        // Create a few test users that can be used by tests
        var testUserIds = new[]
        {
            Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Guid.Parse("00000000-0000-0000-0000-000000000002"),
            Guid.Parse("00000000-0000-0000-0000-000000000003")
        };

        foreach (var userId in testUserIds)
        {
            try
            {
                await using var command = connection.CreateCommand();
                command.CommandText = @"
                    IF NOT EXISTS (SELECT 1 FROM [Security].[Users] WHERE Id = @UserId)
                    BEGIN
                        INSERT INTO [Security].[Users] (Id, Email, PasswordHash, PasswordSalt, Role, CreatedAt, UpdatedAt)
                        VALUES (@UserId, @Email, @PasswordHash, @PasswordSalt, @Role, GETUTCDATE(), GETUTCDATE())
                    END";
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@Email", $"testuser{userId.ToString().Substring(0, 8)}@test.com");
                command.Parameters.AddWithValue("@PasswordHash", "HASHED_PASSWORD_FOR_TESTING");
                command.Parameters.AddWithValue("@PasswordSalt", "SALT_FOR_TESTING");
                command.Parameters.AddWithValue("@Role", "Customer");
                await command.ExecuteNonQueryAsync();
            }
            catch
            {
                // Ignore if user creation fails
            }
        }
    }

    private async Task CleanDatabaseTablesAsync(SqlConnection connection)
    {
        var tables = new[]
        {
            "[Notification].[DeviceTokens]",
            "[Notification].[NotificationLogs]",
            "[Notification].[InAppNotifications]",
            "[Notification].[NotificationQueue]",
            "[Notification].[NotificationPreferences]",
            "[Notification].[NotificationTemplates]"
        };

        foreach (var table in tables)
        {
            try
            {
                await using var command = connection.CreateCommand();
                command.CommandText = $"DELETE FROM {table}";
                await command.ExecuteNonQueryAsync();
            }
            catch
            {
                // Table might not exist yet, ignore
            }
        }
    }

    public async Task CleanupDatabaseAsync()
    {
        // Clean tables after tests
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        await CleanDatabaseTablesAsync(connection);
    }
}
