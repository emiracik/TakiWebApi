using System.Data;
using Microsoft.Data.SqlClient;
using TakiWebApi.Models;

namespace TakiWebApi.Data;

public interface IPromotionRepository
{
    /// <summary>Şu an geçerli, kodu olan kampanyalar.</summary>
    Task<IEnumerable<Promotion>> GetActiveAsync();

    /// <summary>Kod geçerliyse kampanyayı döner (tarih, aktiflik, kullanım limiti).</summary>
    Task<Promotion?> GetValidByCodeAsync(string code);

    Task<bool> HasUserUsedAsync(int promotionId, int userId);

    /// <summary>Kullanımı kaydeder ve sayacı artırır; kullanıcı kodu daha önce kullandıysa false.</summary>
    Task<bool> RecordUsageAsync(int promotionId, int userId, int? matchingRequestId, decimal discount);
}

public class PromotionRepository : IPromotionRepository
{
    private readonly string _connectionString;

    public PromotionRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("SqlServerConnection")
            ?? throw new ArgumentNullException(nameof(configuration));
    }

    private const string Columns = @"PromotionID, Code, Title, CAST(Description AS nvarchar(max)) AS Description,
        DiscountType, DiscountValue, MinimumAmount, MaximumDiscount, StartDate, EndDate,
        UsageLimit, UsedCount, IsActive";

    private const string ValidWhere = @"IsActive = 1 AND Code IS NOT NULL
        AND StartDate <= GETDATE() AND EndDate >= GETDATE()
        AND (UsageLimit IS NULL OR UsedCount < UsageLimit)";

    public async Task<IEnumerable<Promotion>> GetActiveAsync()
    {
        var list = new List<Promotion>();
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(
            $"SELECT {Columns} FROM Promotions WHERE {ValidWhere} ORDER BY EndDate", connection);
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) list.Add(Map(reader));
        return list;
    }

    public async Task<Promotion?> GetValidByCodeAsync(string code)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(
            $"SELECT TOP 1 {Columns} FROM Promotions WHERE Code = @Code AND {ValidWhere}", connection);
        command.Parameters.AddWithValue("@Code", code.Trim().ToUpperInvariant());
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? Map(reader) : null;
    }

    public async Task<bool> HasUserUsedAsync(int promotionId, int userId)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(
            "SELECT 1 FROM PromotionUsages WHERE PromotionID = @P AND UserID = @U", connection);
        command.Parameters.AddWithValue("@P", promotionId);
        command.Parameters.AddWithValue("@U", userId);
        await connection.OpenAsync();
        return await command.ExecuteScalarAsync() is not null;
    }

    public async Task<bool> RecordUsageAsync(int promotionId, int userId, int? matchingRequestId, decimal discount)
    {
        const string sql = @"
            IF NOT EXISTS (SELECT 1 FROM PromotionUsages WHERE PromotionID = @P AND UserID = @U)
            BEGIN
                INSERT INTO PromotionUsages (PromotionID, UserID, MatchingRequestID, DiscountAmount)
                VALUES (@P, @U, @M, @D);
                UPDATE Promotions SET UsedCount = UsedCount + 1 WHERE PromotionID = @P;
                SELECT 1;
            END";
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@P", promotionId);
        command.Parameters.AddWithValue("@U", userId);
        command.Parameters.AddWithValue("@M", (object?)matchingRequestId ?? DBNull.Value);
        command.Parameters.AddWithValue("@D", discount);
        await connection.OpenAsync();
        try
        {
            return await command.ExecuteScalarAsync() is not null;
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            return false; // eşzamanlı ikinci kullanım (unique index)
        }
    }

    private static Promotion Map(SqlDataReader r) => new()
    {
        PromotionID = r.GetInt32("PromotionID"),
        Code = r.IsDBNull("Code") ? null : r.GetString("Code"),
        Title = r.GetString("Title"),
        Description = r.IsDBNull("Description") ? null : r.GetString("Description"),
        DiscountType = r.GetString("DiscountType"),
        DiscountValue = r.GetDecimal("DiscountValue"),
        MinimumAmount = r.IsDBNull("MinimumAmount") ? null : r.GetDecimal("MinimumAmount"),
        MaximumDiscount = r.IsDBNull("MaximumDiscount") ? null : r.GetDecimal("MaximumDiscount"),
        StartDate = r.GetDateTime("StartDate"),
        EndDate = r.GetDateTime("EndDate"),
        UsageLimit = r.IsDBNull("UsageLimit") ? null : r.GetInt32("UsageLimit"),
        UsedCount = r.GetInt32("UsedCount"),
        IsActive = r.GetBoolean("IsActive"),
    };
}
