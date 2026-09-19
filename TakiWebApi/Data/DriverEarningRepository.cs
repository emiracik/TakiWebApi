using System.Data;
using Microsoft.Data.SqlClient;
using TakiWebApi.Models;

namespace TakiWebApi.Data;

public class DriverEarningRepository : IDriverEarningRepository
{
    private readonly string _connectionString;

    public DriverEarningRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("SqlServerConnection")
            ?? throw new ArgumentNullException(nameof(configuration), "Connection string cannot be null");
    }

    public async Task<DriverEarning> CreateAsync(DriverEarning earning)
    {
        const string sql = @"
            INSERT INTO DriverEarnings (DriverID, TripID, Amount, EarnedAt, CreatedDate, IsDeleted)
            VALUES (@DriverID, @TripID, @Amount, @EarnedAt, @CreatedDate, 0);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@DriverID", (object?)earning.DriverID ?? DBNull.Value);
        command.Parameters.AddWithValue("@TripID", (object?)earning.TripID ?? DBNull.Value);
        command.Parameters.AddWithValue("@Amount", (object?)earning.Amount ?? DBNull.Value);
        command.Parameters.AddWithValue("@EarnedAt", earning.EarnedAt);
        command.Parameters.AddWithValue("@CreatedDate", DateTime.UtcNow);

        await connection.OpenAsync();
        var newId = (int)(await command.ExecuteScalarAsync() ?? 0);
        earning.EarningID = newId;
        return earning;
    }

    public async Task<IEnumerable<DriverEarning>> GetByDriverIdAsync(int driverId)
    {
        var earnings = new List<DriverEarning>();
        const string sql = @"
            SELECT EarningID, DriverID, TripID, Amount, EarnedAt, CreatedDate
            FROM DriverEarnings
            WHERE DriverID = @DriverId AND IsDeleted = 0
            ORDER BY EarnedAt DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@DriverId", driverId);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            earnings.Add(new DriverEarning
            {
                EarningID = reader.GetInt32("EarningID"),
                DriverID = reader.IsDBNull("DriverID") ? null : reader.GetInt32("DriverID"),
                TripID = reader.IsDBNull("TripID") ? null : reader.GetInt32("TripID"),
                Amount = reader.IsDBNull("Amount") ? null : reader.GetDecimal("Amount"),
                EarnedAt = reader.GetDateTime("EarnedAt"),
                CreatedDate = reader.GetDateTime("CreatedDate"),
            });
        }
        return earnings;
    }

    public async Task<bool> ExistsForTripAsync(int tripId)
    {
        const string sql = "SELECT COUNT(1) FROM DriverEarnings WHERE TripID = @TripId AND IsDeleted = 0";
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@TripId", tripId);
        await connection.OpenAsync();
        var count = (int)(await command.ExecuteScalarAsync() ?? 0);
        return count > 0;
    }

    public async Task<decimal> GetTotalByDriverIdAsync(int driverId)
    {
        const string sql = @"
            SELECT ISNULL(SUM(Amount), 0)
            FROM DriverEarnings
            WHERE DriverID = @DriverId AND IsDeleted = 0";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@DriverId", driverId);

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();
        return result != null && result != DBNull.Value ? Convert.ToDecimal(result) : 0m;
    }
}
