using Dapper;
using Microsoft.Data.SqlClient;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Interfaces;

namespace ShahdCooperative.NotificationService.Infrastructure.Repositories;

public class NotificationPreferenceRepository : INotificationPreferenceRepository
{
    private readonly string _connectionString;

    public NotificationPreferenceRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task<NotificationPreference?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, UserId, EmailNotifications, SmsNotifications, PushNotifications, InAppNotifications,
                   MarketingEmails, OrderUpdates, SecurityAlerts, NewsletterSubscription, CreatedAt, UpdatedAt
            FROM Notification.NotificationPreferences
            WHERE UserId = @UserId";

        using var connection = new SqlConnection(_connectionString);
        return await connection.QuerySingleOrDefaultAsync<NotificationPreference>(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));
    }

    public async Task<Guid> CreateAsync(NotificationPreference preference, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO Notification.NotificationPreferences
                (Id, UserId, EmailNotifications, SmsNotifications, PushNotifications, InAppNotifications,
                 MarketingEmails, OrderUpdates, SecurityAlerts, NewsletterSubscription, CreatedAt, UpdatedAt)
            VALUES
                (@Id, @UserId, @EmailNotifications, @SmsNotifications, @PushNotifications, @InAppNotifications,
                 @MarketingEmails, @OrderUpdates, @SecurityAlerts, @NewsletterSubscription, @CreatedAt, @UpdatedAt)";

        preference.Id = preference.Id == Guid.Empty ? Guid.NewGuid() : preference.Id;
        preference.CreatedAt = DateTime.UtcNow;
        preference.UpdatedAt = DateTime.UtcNow;

        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync(
            new CommandDefinition(sql, preference, cancellationToken: cancellationToken));

        return preference.Id;
    }

    public async Task UpdateAsync(NotificationPreference preference, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE Notification.NotificationPreferences
            SET EmailNotifications = @EmailNotifications,
                SmsNotifications = @SmsNotifications,
                PushNotifications = @PushNotifications,
                InAppNotifications = @InAppNotifications,
                MarketingEmails = @MarketingEmails,
                OrderUpdates = @OrderUpdates,
                SecurityAlerts = @SecurityAlerts,
                NewsletterSubscription = @NewsletterSubscription,
                UpdatedAt = @UpdatedAt
            WHERE UserId = @UserId";

        preference.UpdatedAt = DateTime.UtcNow;

        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync(
            new CommandDefinition(sql, preference, cancellationToken: cancellationToken));
    }

    public async Task DeleteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM Notification.NotificationPreferences WHERE UserId = @UserId";

        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));
    }
}
