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
    private readonly string _connectionString = "Server=.\\SQLEXPRESS;Database=ShahdCooperative;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";

    public string ConnectionString => _connectionString;

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
        // Clean all tables before running tests for test isolation
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        await CleanDatabaseTablesAsync(connection);

        // Create test users if they don't exist (needed for foreign keys)
        await CreateTestUsersAsync(connection);
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

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Clean up test data when done
            CleanupDatabaseAsync().GetAwaiter().GetResult();
        }
        base.Dispose(disposing);
    }
}
