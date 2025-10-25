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
            SELECT Id, TemplateKey, TemplateName, Subject, BodyTemplate,
                   NotificationType, IsActive, CreatedAt, UpdatedAt
            FROM Notification.NotificationTemplates
            WHERE Id = @Id";

        using var connection = new SqlConnection(_connectionString);
        return await connection.QuerySingleOrDefaultAsync<NotificationTemplate>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
    }

    public async Task<NotificationTemplate?> GetByKeyAsync(string templateKey, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, TemplateKey, TemplateName, Subject, BodyTemplate,
                   NotificationType, IsActive, CreatedAt, UpdatedAt
            FROM Notification.NotificationTemplates
            WHERE TemplateKey = @TemplateKey";

        using var connection = new SqlConnection(_connectionString);
        return await connection.QuerySingleOrDefaultAsync<NotificationTemplate>(
            new CommandDefinition(sql, new { TemplateKey = templateKey }, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<NotificationTemplate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, TemplateKey, TemplateName, Subject, BodyTemplate,
                   NotificationType, IsActive, CreatedAt, UpdatedAt
            FROM Notification.NotificationTemplates
            ORDER BY TemplateName";

        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryAsync<NotificationTemplate>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<NotificationTemplate>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, TemplateKey, TemplateName, Subject, BodyTemplate,
                   NotificationType, IsActive, CreatedAt, UpdatedAt
            FROM Notification.NotificationTemplates
            WHERE IsActive = 1
            ORDER BY TemplateName";

        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryAsync<NotificationTemplate>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));
    }

    public async Task<Guid> CreateAsync(NotificationTemplate template, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO Notification.NotificationTemplates
                (Id, TemplateKey, TemplateName, Subject, BodyTemplate, NotificationType, IsActive, CreatedAt, UpdatedAt)
            VALUES
                (@Id, @TemplateKey, @TemplateName, @Subject, @BodyTemplate, @NotificationType, @IsActive, @CreatedAt, @UpdatedAt)";

        template.Id = template.Id == Guid.Empty ? Guid.NewGuid() : template.Id;
        template.CreatedAt = DateTime.UtcNow;
        template.UpdatedAt = DateTime.UtcNow;

        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync(
            new CommandDefinition(sql, template, cancellationToken: cancellationToken));

        return template.Id;
    }

    public async Task UpdateAsync(NotificationTemplate template, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE Notification.NotificationTemplates
            SET TemplateKey = @TemplateKey,
                TemplateName = @TemplateName,
                Subject = @Subject,
                BodyTemplate = @BodyTemplate,
                NotificationType = @NotificationType,
                IsActive = @IsActive,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id";

        template.UpdatedAt = DateTime.UtcNow;

        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync(
            new CommandDefinition(sql, template, cancellationToken: cancellationToken));
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM Notification.NotificationTemplates WHERE Id = @Id";

        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
    }
}
