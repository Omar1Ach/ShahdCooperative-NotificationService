using Dapper;
using Microsoft.Data.SqlClient;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Enums;
using ShahdCooperative.NotificationService.Domain.Interfaces;

namespace ShahdCooperative.NotificationService.Infrastructure.Repositories;

public class NotificationQueueRepository : INotificationQueueRepository
{
    private readonly string _connectionString;

    public NotificationQueueRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task<Guid> EnqueueAsync(NotificationQueue notification, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO Notification.NotificationQueue
                (Id, NotificationType, Recipient, Subject, Body, TemplateKey, TemplateData,
                 Priority, Status, AttemptCount, MaxRetries, NextRetryAt, ErrorMessage, CreatedAt, ProcessedAt)
            VALUES
                (@Id, @NotificationType, @Recipient, @Subject, @Body, @TemplateKey, @TemplateData,
                 @Priority, @Status, @AttemptCount, @MaxRetries, @NextRetryAt, @ErrorMessage, @CreatedAt, @ProcessedAt)";

        notification.Id = notification.Id == Guid.Empty ? Guid.NewGuid() : notification.Id;
        notification.CreatedAt = DateTime.UtcNow;

        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync(
            new CommandDefinition(sql, notification, cancellationToken: cancellationToken));

        return notification.Id;
    }

    public async Task<IEnumerable<NotificationQueue>> GetPendingNotificationsAsync(
        int batchSize, int maxRetries, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT TOP(@BatchSize)
                Id, NotificationType, Recipient, Subject, Body, TemplateKey, TemplateData,
                Priority, Status, AttemptCount, MaxRetries, NextRetryAt, ErrorMessage, CreatedAt, ProcessedAt
            FROM Notification.NotificationQueue
            WHERE Status = @PendingStatus
              AND AttemptCount < MaxRetries
              AND (NextRetryAt IS NULL OR NextRetryAt <= GETUTCDATE())
            ORDER BY Priority ASC, CreatedAt ASC";

        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryAsync<NotificationQueue>(
            new CommandDefinition(sql, new
            {
                BatchSize = batchSize,
                PendingStatus = NotificationStatus.Pending.ToString()
            }, cancellationToken: cancellationToken));
    }

    public async Task UpdateStatusAsync(Guid id, NotificationStatus status, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE Notification.NotificationQueue
            SET Status = @Status,
                ProcessedAt = CASE WHEN @Status IN ('Sent', 'Failed', 'Cancelled') THEN GETUTCDATE() ELSE ProcessedAt END
            WHERE Id = @Id";

        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync(
            new CommandDefinition(sql, new { Id = id, Status = status.ToString() }, cancellationToken: cancellationToken));
    }

    public async Task IncrementAttemptAsync(Guid id, string? errorMessage, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE Notification.NotificationQueue
            SET AttemptCount = AttemptCount + 1,
                ErrorMessage = @ErrorMessage,
                Status = CASE
                    WHEN AttemptCount + 1 >= MaxRetries THEN 'Failed'
                    ELSE 'Pending'
                END
            WHERE Id = @Id";

        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync(
            new CommandDefinition(sql, new { Id = id, ErrorMessage = errorMessage }, cancellationToken: cancellationToken));
    }

    public async Task SetNextRetryAsync(Guid id, DateTime nextRetryAt, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE Notification.NotificationQueue
            SET NextRetryAt = @NextRetryAt
            WHERE Id = @Id";

        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync(
            new CommandDefinition(sql, new { Id = id, NextRetryAt = nextRetryAt }, cancellationToken: cancellationToken));
    }

    public async Task<NotificationQueue?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, NotificationType, Recipient, Subject, Body, TemplateKey, TemplateData,
                   Priority, Status, AttemptCount, MaxRetries, NextRetryAt, ErrorMessage, CreatedAt, ProcessedAt
            FROM Notification.NotificationQueue
            WHERE Id = @Id";

        using var connection = new SqlConnection(_connectionString);
        return await connection.QuerySingleOrDefaultAsync<NotificationQueue>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
    }
}
