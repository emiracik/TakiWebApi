using System.Data;
using Microsoft.Data.SqlClient;

namespace TakiWebApi.Data;

// Cihaz token'ları dbo.DeviceTokens tablosunda (Migrations/002).
public class DeviceTokenRepository : IDeviceTokenRepository
{
    private readonly string _connectionString;

    public DeviceTokenRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("SqlServerConnection")
            ?? throw new ArgumentNullException(nameof(configuration), "Connection string cannot be null");
    }

    public async Task UpsertAsync(string token, int subjectId, string subjectType, string? platform)
    {
        const string sql = @"
            MERGE DeviceTokens WITH (HOLDLOCK) AS target
            USING (SELECT @Token AS Token) AS source ON target.Token = source.Token
            WHEN MATCHED THEN
                UPDATE SET SubjectId = @SubjectId, SubjectType = @SubjectType,
                           Platform = @Platform, UpdatedAt = SYSUTCDATETIME()
            WHEN NOT MATCHED THEN
                INSERT (Token, SubjectId, SubjectType, Platform)
                VALUES (@Token, @SubjectId, @SubjectType, @Platform);";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Token", SqlDbType.NVarChar, 512).Value = token;
        command.Parameters.Add("@SubjectId", SqlDbType.Int).Value = subjectId;
        command.Parameters.Add("@SubjectType", SqlDbType.VarChar, 10).Value = subjectType;
        command.Parameters.Add("@Platform", SqlDbType.VarChar, 10).Value = (object?)platform ?? DBNull.Value;
        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(string token)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("DELETE FROM DeviceTokens WHERE Token = @Token", connection);
        command.Parameters.Add("@Token", SqlDbType.NVarChar, 512).Value = token;
        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteAllForSubjectAsync(int subjectId, string subjectType)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(
            "DELETE FROM DeviceTokens WHERE SubjectId = @SubjectId AND SubjectType = @SubjectType", connection);
        command.Parameters.Add("@SubjectId", SqlDbType.Int).Value = subjectId;
        command.Parameters.Add("@SubjectType", SqlDbType.VarChar, 10).Value = subjectType;
        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    public async Task<IReadOnlyList<string>> GetTokensAsync(int subjectId, string subjectType)
    {
        var tokens = new List<string>();
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(@"
            SELECT Token FROM DeviceTokens
            WHERE SubjectId = @SubjectId AND SubjectType = @SubjectType", connection);
        command.Parameters.Add("@SubjectId", SqlDbType.Int).Value = subjectId;
        command.Parameters.Add("@SubjectType", SqlDbType.VarChar, 10).Value = subjectType;
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            tokens.Add(reader.GetString(0));
        return tokens;
    }
}
