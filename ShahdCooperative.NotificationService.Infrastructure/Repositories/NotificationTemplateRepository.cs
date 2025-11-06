using Dapper;
using Microsoft.Data.SqlClient;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Enums;
using ShahdCooperative.NotificationService.Domain.Interfaces;

namespace ShahdCooperative.NotificationService.Infrastructure.Repositories;

public class NotificationTemplateRepository : INotificationTemplateRepository
{
    private readonly string _connectionString;

    public NotificationTemplateRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task<NotificationTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, [Key], Name, Subject, Body,
                   Type, IsActive, CreatedAt, UpdatedAt, IsDeleted
            FROM Notification.NotificationTemplates
            WHERE Id = @Id AND IsDeleted = 0";

        using var connection = new SqlConnection(_connectionString);
        return await connection.QuerySingleOrDefaultAsync<NotificationTemplate>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
    }

    public async Task<NotificationTemplate?> GetByKeyAsync(string templateKey, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, [Key], Name, Subject, Body,
                   Type, IsActive, CreatedAt, UpdatedAt, IsDeleted
            FROM Notification.NotificationTemplates
            WHERE [Key] = @TemplateKey AND IsDeleted = 0";

        using var connection = new SqlConnection(_connectionString);
        return await connection.QuerySingleOrDefaultAsync<NotificationTemplate>(
            new CommandDefinition(sql, new { TemplateKey = templateKey }, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<NotificationTemplate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, [Key], Name, Subject, Body,
                   Type, IsActive, CreatedAt, UpdatedAt, IsDeleted
            FROM Notification.NotificationTemplates
            WHERE IsDeleted = 0
            ORDER BY Name";

        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryAsync<NotificationTemplate>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<NotificationTemplate>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, [Key], Name, Subject, Body,
                   Type, IsActive, CreatedAt, UpdatedAt, IsDeleted
            FROM Notification.NotificationTemplates
            WHERE IsActive = 1 AND IsDeleted = 0
            ORDER BY Name";

        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryAsync<NotificationTemplate>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));
    }

    public async Task<Guid> CreateAsync(NotificationTemplate template, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO Notification.NotificationTemplates
                (Id, [Key], Name, Subject, Body, Type, IsActive, CreatedAt, UpdatedAt, IsDeleted)
            VALUES
                (@Id, @Key, @Name, @Subject, @Body, @Type, @IsActive, @CreatedAt, @UpdatedAt, @IsDeleted)";

        template.Id = template.Id == Guid.Empty ? Guid.NewGuid() : template.Id;
        template.CreatedAt = DateTime.UtcNow;
        template.UpdatedAt = DateTime.UtcNow;

        var parameters = new DynamicParameters();
        parameters.Add("@Id", template.Id);
        parameters.Add("@Key", template.Key);
        parameters.Add("@Name", template.Name);
        parameters.Add("@Subject", template.Subject);
        parameters.Add("@Body", template.Body);
        parameters.Add("@Type", template.Type.ToString());
        parameters.Add("@IsActive", template.IsActive);
        parameters.Add("@CreatedAt", template.CreatedAt);
        parameters.Add("@UpdatedAt", template.UpdatedAt);
        parameters.Add("@IsDeleted", template.IsDeleted);

        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));

        return template.Id;
    }

    public async Task UpdateAsync(NotificationTemplate template, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE Notification.NotificationTemplates
            SET [Key] = @Key,
                Name = @Name,
                Subject = @Subject,
                Body = @Body,
                Type = @Type,
                IsActive = @IsActive,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id AND IsDeleted = 0";

        template.UpdatedAt = DateTime.UtcNow;

        var parameters = new DynamicParameters();
        parameters.Add("@Id", template.Id);
        parameters.Add("@Key", template.Key);
        parameters.Add("@Name", template.Name);
        parameters.Add("@Subject", template.Subject);
        parameters.Add("@Body", template.Body);
        parameters.Add("@Type", template.Type.ToString());
        parameters.Add("@IsActive", template.IsActive);
        parameters.Add("@UpdatedAt", template.UpdatedAt);

        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE Notification.NotificationTemplates
            SET IsDeleted = 1, UpdatedAt = GETUTCDATE()
            WHERE Id = @Id";

        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
    }
}
