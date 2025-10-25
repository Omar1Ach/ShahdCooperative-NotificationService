using Dapper;
using Microsoft.Data.SqlClient;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Interfaces;

namespace ShahdCooperative.NotificationService.Infrastructure.Repositories;

public class DeviceTokenRepository : IDeviceTokenRepository
{
    private readonly string _connectionString;

    public DeviceTokenRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task<Guid> RegisterTokenAsync(DeviceToken deviceToken, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO Notification.DeviceTokens
                (Id, UserId, Token, DeviceType, Platform, IsActive, LastUsedAt, CreatedAt, UpdatedAt)
            VALUES
                (@Id, @UserId, @Token, @DeviceType, @Platform, @IsActive, @LastUsedAt, @CreatedAt, @UpdatedAt)";

        deviceToken.Id = deviceToken.Id == Guid.Empty ? Guid.NewGuid() : deviceToken.Id;
        deviceToken.CreatedAt = DateTime.UtcNow;
        deviceToken.UpdatedAt = DateTime.UtcNow;

        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync(
            new CommandDefinition(sql, deviceToken, cancellationToken: cancellationToken));

        return deviceToken.Id;
    }

    public async Task<IEnumerable<DeviceToken>> GetActiveTokensAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, UserId, Token, DeviceType, Platform, IsActive, LastUsedAt, CreatedAt, UpdatedAt
            FROM Notification.DeviceTokens
            WHERE UserId = @UserId
              AND IsActive = 1
            ORDER BY LastUsedAt DESC";

        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryAsync<DeviceToken>(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<DeviceToken>> GetUserTokensAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, UserId, Token, DeviceType, Platform, IsActive, LastUsedAt, CreatedAt, UpdatedAt
            FROM Notification.DeviceTokens
            WHERE UserId = @UserId
            ORDER BY CreatedAt DESC";

        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryAsync<DeviceToken>(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));
    }

    public async Task DeactivateTokenAsync(Guid tokenId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE Notification.DeviceTokens
            SET IsActive = 0,
                UpdatedAt = GETUTCDATE()
            WHERE Id = @TokenId";

        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync(
            new CommandDefinition(sql, new { TokenId = tokenId }, cancellationToken: cancellationToken));
    }

    public async Task DeleteTokenAsync(Guid tokenId, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM Notification.DeviceTokens WHERE Id = @TokenId";

        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync(
            new CommandDefinition(sql, new { TokenId = tokenId }, cancellationToken: cancellationToken));
    }

    public async Task UpdateLastUsedAsync(Guid tokenId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE Notification.DeviceTokens
            SET LastUsedAt = GETUTCDATE(),
                UpdatedAt = GETUTCDATE()
            WHERE Id = @TokenId";

        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync(
            new CommandDefinition(sql, new { TokenId = tokenId }, cancellationToken: cancellationToken));
    }
}
