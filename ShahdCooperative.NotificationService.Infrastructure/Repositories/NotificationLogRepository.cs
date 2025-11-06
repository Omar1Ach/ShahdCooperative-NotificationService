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
                (Id, UserId, RecipientEmail, RecipientPhone, Type, Subject, Message, Status, SentAt, RetryCount, Metadata, CreatedAt, UpdatedAt, IsDeleted, ErrorMessage)
            VALUES
                (@Id, @UserId, @RecipientEmail, @RecipientPhone, @Type, @Subject, @Message, @Status, @SentAt, @RetryCount, @Metadata, @CreatedAt, @UpdatedAt, @IsDeleted, @ErrorMessage)";

        log.Id = log.Id == Guid.Empty ? Guid.NewGuid() : log.Id;
        log.CreatedAt = DateTime.UtcNow;
        log.UpdatedAt = DateTime.UtcNow;

        var parameters = new DynamicParameters();
        parameters.Add("@Id", log.Id);
        parameters.Add("@UserId", log.UserId);
        parameters.Add("@RecipientEmail", log.RecipientEmail);
        parameters.Add("@RecipientPhone", log.RecipientPhone);
        parameters.Add("@Type", log.Type.ToString());
        parameters.Add("@Subject", log.Subject);
        parameters.Add("@Message", log.Message);
        parameters.Add("@Status", log.Status.ToString());
        parameters.Add("@SentAt", log.SentAt);
        parameters.Add("@RetryCount", log.RetryCount);
        parameters.Add("@Metadata", log.Metadata);
        parameters.Add("@CreatedAt", log.CreatedAt);
        parameters.Add("@UpdatedAt", log.UpdatedAt);
        parameters.Add("@IsDeleted", log.IsDeleted);
        parameters.Add("@ErrorMessage", log.ErrorMessage);

        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));

        return log.Id;
    }

    public async Task<IEnumerable<NotificationLog>> GetLogsAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, UserId, RecipientEmail, RecipientPhone, Type, Subject, Message, Status, SentAt, RetryCount, Metadata, CreatedAt, UpdatedAt, IsDeleted, ErrorMessage
            FROM Notification.NotificationLogs
            WHERE IsDeleted = 0
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
            SELECT Id, UserId, RecipientEmail, RecipientPhone, Type, Subject, Message, Status, SentAt, RetryCount, Metadata, CreatedAt, UpdatedAt, IsDeleted, ErrorMessage
            FROM Notification.NotificationLogs
            WHERE (RecipientEmail = @Recipient OR RecipientPhone = @Recipient) AND IsDeleted = 0
            ORDER BY CreatedAt DESC";

        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryAsync<NotificationLog>(
            new CommandDefinition(sql, new { Recipient = recipient }, cancellationToken: cancellationToken));
    }

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT COUNT(*) FROM Notification.NotificationLogs WHERE IsDeleted = 0";

        using var connection = new SqlConnection(_connectionString);
        return await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));
    }
}
