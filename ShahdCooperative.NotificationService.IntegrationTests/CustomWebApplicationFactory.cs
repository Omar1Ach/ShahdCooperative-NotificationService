using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using ShahdCooperative.NotificationService.Domain.Interfaces;

namespace ShahdCooperative.NotificationService.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly string _databaseName = $"TestDb_{Guid.NewGuid():N}";
    private readonly string _connectionString;
    private readonly bool _isCI;
    private bool _isInitialized = false;

    public string ConnectionString => _connectionString;

    public CustomWebApplicationFactory()
    {
        // Detect CI environment (GitHub Actions)
        _isCI = Environment.GetEnvironmentVariable("CI") == "true" ||
                Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true";

        if (_isCI)
        {
            // GitHub Actions - use service container
            _connectionString = $"Server=localhost,1433;Database={_databaseName};User Id=sa;Password=Your_password123;TrustServerCertificate=true;Connection Timeout=30";
            Console.WriteLine("[CI] Using GitHub Actions service container");
        }
        else
        {
            // Local development - use LocalDB
            _connectionString = $"Server=(localdb)\\MSSQLLocalDB;Database={_databaseName};Integrated Security=true;MultipleActiveResultSets=true;TrustServerCertificate=true;Connection Timeout=30";
            Console.WriteLine("[LOCAL] Using LocalDB");
        }
    }

    async Task IAsyncLifetime.InitializeAsync()
    {
        if (_isInitialized) return;

        // Create the database
        await CreateDatabaseAsync();

        // Initialize the database schema
        await InitializeDatabaseAsync();

        _isInitialized = true;
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        // Clean up the test database
        await DropDatabaseAsync();
    }

    private async Task CreateDatabaseAsync()
    {
        var masterConnectionString = GetMasterConnectionString();
        await using var connection = new SqlConnection(masterConnectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = $"IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = N'{_databaseName}') CREATE DATABASE [{_databaseName}]";
        await command.ExecuteNonQueryAsync();
    }

    private async Task DropDatabaseAsync()
    {
        try
        {
            var masterConnectionString = GetMasterConnectionString();
            await using var connection = new SqlConnection(masterConnectionString);
            await connection.OpenAsync();

            await using var command = connection.CreateCommand();
            command.CommandText = $@"
                IF EXISTS (SELECT * FROM sys.databases WHERE name = N'{_databaseName}')
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

    private string GetMasterConnectionString()
    {
        if (_isCI)
        {
            // GitHub Actions service container
            return "Server=localhost,1433;Database=master;User Id=sa;Password=Your_password123;TrustServerCertificate=true;Connection Timeout=30";
        }
        else
        {
            // Local LocalDB
            return "Server=(localdb)\\MSSQLLocalDB;Database=master;Integrated Security=true;TrustServerCertificate=true;Connection Timeout=30";
        }
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
        await using var connection = new SqlConnection(_connectionString);
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
        // Create schemas and tables
        await CreateSchemasAndTablesAsync(connection);
    }

    private async Task CreateSchemasAndTablesAsync(SqlConnection connection)
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
                  Category NVARCHAR(50) NULL,
                  ActionUrl NVARCHAR(500) NULL,
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
            catch (Exception ex)
            {
                // Log but don't fail
                Console.WriteLine($"Error creating schema/table: {ex.Message}");
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
            catch (Exception ex)
            {
                // Log but don't fail
                Console.WriteLine($"Error creating test user: {ex.Message}");
            }
        }
    }

    private async Task CleanDatabaseTablesAsync(SqlConnection connection)
    {
        // Clean tables in correct order to handle foreign key constraints
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
                var rowsAffected = await command.ExecuteNonQueryAsync();
                if (rowsAffected > 0)
                {
                    Console.WriteLine($"[CLEANUP] Deleted {rowsAffected} rows from {table}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CLEANUP ERROR] Failed to clean {table}: {ex.Message}");
            }
        }
    }

    public async Task CleanupDatabaseAsync()
    {
        // Clean tables after tests with retry logic
        const int maxRetries = 3;
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                await using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                await CleanDatabaseTablesAsync(connection);
                break; // Success
            }
            catch (Exception ex)
            {
                if (i == maxRetries - 1)
                {
                    Console.WriteLine($"Failed to cleanup database after {maxRetries} attempts: {ex.Message}");
                }
                await Task.Delay(100); // Wait before retry
            }
        }
    }
}
