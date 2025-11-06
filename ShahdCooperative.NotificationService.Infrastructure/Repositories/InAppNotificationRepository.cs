using Dapper;
using Microsoft.Data.SqlClient;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Interfaces;

namespace ShahdCooperative.NotificationService.Infrastructure.Repositories;

public class InAppNotificationRepository : IInAppNotificationRepository
{
    private readonly string _connectionString;

    public InAppNotificationRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task<Guid> CreateAsync(InAppNotification notification, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO Notification.InAppNotifications
                (Id, UserId, Title, Message, Type, Category, ActionUrl, IsRead, ReadAt, CreatedAt, ExpiresAt)
            VALUES
                (@Id, @UserId, @Title, @Message, @Type, @Category, @ActionUrl, @IsRead, @ReadAt, @CreatedAt, @ExpiresAt)";

        notification.Id = notification.Id == Guid.Empty ? Guid.NewGuid() : notification.Id;
        notification.CreatedAt = DateTime.UtcNow;

        var parameters = new DynamicParameters();
        parameters.Add("@Id", notification.Id);
        parameters.Add("@UserId", notification.UserId);
        parameters.Add("@Title", notification.Title);
        parameters.Add("@Message", notification.Message);
        parameters.Add("@Type", notification.Type.ToString());
        parameters.Add("@Category", notification.Category);
        parameters.Add("@ActionUrl", notification.ActionUrl);
        parameters.Add("@IsRead", notification.IsRead);
        parameters.Add("@ReadAt", notification.ReadAt);
        parameters.Add("@CreatedAt", notification.CreatedAt);
        parameters.Add("@ExpiresAt", notification.ExpiresAt);

        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));

        return notification.Id;
    }

    public async Task<IEnumerable<InAppNotification>> GetUserNotificationsAsync(
        Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, UserId, Title, Message, Type, Category, ActionUrl, IsRead, ReadAt, CreatedAt, ExpiresAt
            FROM Notification.InAppNotifications
            WHERE UserId = @UserId
              AND (ExpiresAt IS NULL OR ExpiresAt > GETUTCDATE())
            ORDER BY CreatedAt DESC
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY";

        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryAsync<InAppNotification>(
            new CommandDefinition(sql, new
            {
                UserId = userId,
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            }, cancellationToken: cancellationToken));
    }

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT COUNT(*)
            FROM Notification.InAppNotifications
            WHERE UserId = @UserId
              AND IsRead = 0
              AND (ExpiresAt IS NULL OR ExpiresAt > GETUTCDATE())";

        using var connection = new SqlConnection(_connectionString);
        return await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));
    }

    public async Task<int> MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE Notification.InAppNotifications
            SET IsRead = 1,
                ReadAt = GETUTCDATE()
            WHERE Id = @NotificationId";

        using var connection = new SqlConnection(_connectionString);
        return await connection.ExecuteAsync(
            new CommandDefinition(sql, new { NotificationId = notificationId }, cancellationToken: cancellationToken));
    }

    public async Task<int> MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE Notification.InAppNotifications
            SET IsRead = 1,
                ReadAt = GETUTCDATE()
            WHERE UserId = @UserId
              AND IsRead = 0";

        using var connection = new SqlConnection(_connectionString);
        return await connection.ExecuteAsync(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));
    }

    public async Task DeleteAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM Notification.InAppNotifications WHERE Id = @NotificationId";

        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync(
            new CommandDefinition(sql, new { NotificationId = notificationId }, cancellationToken: cancellationToken));
    }

    public async Task<int> GetTotalCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT COUNT(*)
            FROM Notification.InAppNotifications
            WHERE UserId = @UserId
              AND (ExpiresAt IS NULL OR ExpiresAt > GETUTCDATE())";

        using var connection = new SqlConnection(_connectionString);
        return await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));
    }

    public async Task<int> DeleteExpiredNotificationsAsync(DateTime currentTime, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            DELETE FROM Notification.InAppNotifications
            WHERE ExpiresAt IS NOT NULL
              AND ExpiresAt <= @CurrentTime";

        using var connection = new SqlConnection(_connectionString);
        return await connection.ExecuteAsync(
            new CommandDefinition(sql, new { CurrentTime = currentTime }, cancellationToken: cancellationToken));
    }
}
