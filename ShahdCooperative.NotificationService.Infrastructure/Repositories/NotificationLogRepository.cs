using Dapper;
using Microsoft.Data.SqlClient;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Interfaces;

namespace ShahdCooperative.NotificationService.Infrastructure.Repositories;

public class NotificationLogRepository : INotificationLogRepository
{
    private readonly string _connectionString;

    public NotificationLogRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task<Guid> CreateAsync(NotificationLog log, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO Notification.NotificationLogs
                (Id, Type, Recipient, Subject, Message, Status, SentAt, CreatedAt)
            VALUES
                (@Id, @Type, @Recipient, @Subject, @Message, @Status, @SentAt, @CreatedAt)";

        log.Id = log.Id == Guid.Empty ? Guid.NewGuid() : log.Id;
        log.CreatedAt = DateTime.UtcNow;

        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync(
            new CommandDefinition(sql, log, cancellationToken: cancellationToken));

        return log.Id;
    }

    public async Task<IEnumerable<NotificationLog>> GetLogsAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, Type, Recipient, Subject, Message, Status, SentAt, CreatedAt
            FROM Notification.NotificationLogs
            ORDER BY CreatedAt DESC
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY";

        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryAsync<NotificationLog>(
            new CommandDefinition(sql, new
            {
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            }, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<NotificationLog>> GetLogsByRecipientAsync(string recipient, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, Type, Recipient, Subject, Message, Status, SentAt, CreatedAt
            FROM Notification.NotificationLogs
            WHERE Recipient = @Recipient
            ORDER BY CreatedAt DESC";

        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryAsync<NotificationLog>(
            new CommandDefinition(sql, new { Recipient = recipient }, cancellationToken: cancellationToken));
    }

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT COUNT(*) FROM Notification.NotificationLogs";

        using var connection = new SqlConnection(_connectionString);
        return await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));
    }
}
