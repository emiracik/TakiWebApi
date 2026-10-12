using System.Data;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.SqlClient;
using TakiWebApi.Models;

namespace TakiWebApi.Data;

// Refresh token'lar dbo.RefreshTokens tablosunda (Migrations/001) tutulur;
// sunucu yeniden başlasa da oturumlar korunur.
public class RefreshTokenRepository : IRefreshTokenRepository
{
    private static readonly TimeSpan Lifetime = TimeSpan.FromDays(30);
    private readonly string _connectionString;

    public RefreshTokenRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("SqlServerConnection")
            ?? throw new ArgumentNullException(nameof(configuration), "Connection string cannot be null");
    }

    public async Task<string> IssueAsync(int subjectId, string subjectType)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        return await InsertAsync(connection, null, subjectId, subjectType);
    }

    public async Task<(RefreshToken Record, string NewToken)?> RotateAsync(string rawToken)
    {
        if (string.IsNullOrWhiteSpace(rawToken))
            return null;

        var hash = Hash(rawToken);
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        RefreshToken? record;
        using (var select = new SqlCommand(@"
            SELECT RefreshTokenID, TokenHash, SubjectId, SubjectType, ExpiresAt, RevokedAt
            FROM RefreshTokens WITH (UPDLOCK, ROWLOCK)
            WHERE TokenHash = @Hash", connection, transaction))
        {
            select.Parameters.Add("@Hash", SqlDbType.Char, 64).Value = hash;
            using var reader = await select.ExecuteReaderAsync();
            record = await reader.ReadAsync()
                ? new RefreshToken
                {
                    RefreshTokenID = reader.GetInt32(0),
                    TokenHash = reader.GetString(1),
                    SubjectId = reader.GetInt32(2),
                    SubjectType = reader.GetString(3),
                    ExpiresAt = reader.GetDateTime(4),
                    RevokedAt = reader.IsDBNull(5) ? null : reader.GetDateTime(5)
                }
                : null;
        }

        if (record == null)
        {
            transaction.Rollback();
            return null;
        }

        if (record.RevokedAt != null)
        {
            // Kullanılmış token yeniden geldi: token çalınmış olabilir.
            await RevokeAllAsync(connection, transaction, record.SubjectId, record.SubjectType);
            transaction.Commit();
            return null;
        }

        if (record.ExpiresAt <= DateTime.UtcNow)
        {
            transaction.Rollback();
            return null;
        }

        var newToken = await InsertAsync(connection, transaction, record.SubjectId, record.SubjectType);
        using (var revoke = new SqlCommand(@"
            UPDATE RefreshTokens SET RevokedAt = @Now, ReplacedByHash = @NewHash
            WHERE RefreshTokenID = @Id", connection, transaction))
        {
            revoke.Parameters.Add("@Now", SqlDbType.DateTime2).Value = DateTime.UtcNow;
            revoke.Parameters.Add("@NewHash", SqlDbType.Char, 64).Value = Hash(newToken);
            revoke.Parameters.Add("@Id", SqlDbType.Int).Value = record.RefreshTokenID;
            await revoke.ExecuteNonQueryAsync();
        }

        transaction.Commit();
        return (record, newToken);
    }

    public async Task RevokeAsync(string rawToken)
    {
        if (string.IsNullOrWhiteSpace(rawToken))
            return;

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(@"
            UPDATE RefreshTokens SET RevokedAt = @Now
            WHERE TokenHash = @Hash AND RevokedAt IS NULL", connection);
        command.Parameters.Add("@Now", SqlDbType.DateTime2).Value = DateTime.UtcNow;
        command.Parameters.Add("@Hash", SqlDbType.Char, 64).Value = Hash(rawToken);
        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    public async Task RevokeAllAsync(int subjectId, string subjectType)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        await RevokeAllAsync(connection, null, subjectId, subjectType);
    }

    private static async Task RevokeAllAsync(SqlConnection connection, SqlTransaction? transaction,
        int subjectId, string subjectType)
    {
        using var command = new SqlCommand(@"
            UPDATE RefreshTokens SET RevokedAt = @Now
            WHERE SubjectId = @SubjectId AND SubjectType = @SubjectType AND RevokedAt IS NULL",
            connection, transaction);
        command.Parameters.Add("@Now", SqlDbType.DateTime2).Value = DateTime.UtcNow;
        command.Parameters.Add("@SubjectId", SqlDbType.Int).Value = subjectId;
        command.Parameters.Add("@SubjectType", SqlDbType.VarChar, 10).Value = subjectType;
        await command.ExecuteNonQueryAsync();
    }

    private static async Task<string> InsertAsync(SqlConnection connection, SqlTransaction? transaction,
        int subjectId, string subjectType)
    {
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48))
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');

        using var command = new SqlCommand(@"
            INSERT INTO RefreshTokens (TokenHash, SubjectId, SubjectType, ExpiresAt)
            VALUES (@Hash, @SubjectId, @SubjectType, @ExpiresAt)", connection, transaction);
        command.Parameters.Add("@Hash", SqlDbType.Char, 64).Value = Hash(rawToken);
        command.Parameters.Add("@SubjectId", SqlDbType.Int).Value = subjectId;
        command.Parameters.Add("@SubjectType", SqlDbType.VarChar, 10).Value = subjectType;
        command.Parameters.Add("@ExpiresAt", SqlDbType.DateTime2).Value = DateTime.UtcNow.Add(Lifetime);
        await command.ExecuteNonQueryAsync();
        return rawToken;
    }

    private static string Hash(string rawToken) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
}
