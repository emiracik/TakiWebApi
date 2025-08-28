using System.Data;
using Microsoft.Data.SqlClient;
using TakiWebApi.Models;

namespace TakiWebApi.Data;

public class DriverRatingRepository : IDriverRatingRepository
{
    private readonly string _connectionString;

    public DriverRatingRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("SqlServerConnection") 
            ?? throw new ArgumentNullException(nameof(configuration), "Connection string cannot be null");
    }

    public async Task<IEnumerable<DriverRating>> GetAllDriverRatingsAsync()
    {
        var driverRatings = new List<DriverRating>();
        const string sql = @"
            SELECT DriverRatingID, TripID, DriverID, UserID, Rating, Comment,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM DriverRatings
            ORDER BY CreatedDate DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            driverRatings.Add(MapDriverRatingFromReader(reader));
        }

        return driverRatings;
    }

    public async Task<DriverRating?> GetDriverRatingByIdAsync(int ratingId)
    {
        const string sql = @"
            SELECT DriverRatingID, TripID, DriverID, UserID, Rating, Comment,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM DriverRatings
            WHERE DriverRatingID = @RatingId";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@RatingId", ratingId);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapDriverRatingFromReader(reader);
        }

        return null;
    }

    public async Task<IEnumerable<DriverRating>> GetDriverRatingsByDriverIdAsync(int driverId)
    {
        var driverRatings = new List<DriverRating>();
        const string sql = @"
            SELECT RatingID, TripID, DriverID, UserID, Rating, RatedAt,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM DriverRatings
            WHERE DriverID = @DriverId AND IsDeleted = 0
            ORDER BY CreatedDate DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@DriverId", driverId);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            driverRatings.Add(MapDriverRatingFromReader(reader));
        }

        return driverRatings;
    }

    public async Task<IEnumerable<DriverRating>> GetDriverRatingsByUserIdAsync(int userId)
    {
        var driverRatings = new List<DriverRating>();
        const string sql = @"
            SELECT RatingID, TripID, DriverID, UserID, Rating, RatedAt,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM DriverRatings
            WHERE UserID = @UserId AND IsDeleted = 0
            ORDER BY RatedAt DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@UserId", userId);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            driverRatings.Add(MapDriverRatingFromReader(reader));
        }

        return driverRatings;
    }

    public async Task<IEnumerable<DriverRating>> GetDriverRatingsByTripIdAsync(int tripId)
    {
        var driverRatings = new List<DriverRating>();
        const string sql = @"
            SELECT RatingID, TripID, DriverID, UserID, Rating, RatedAt,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM DriverRatings
            WHERE TripID = @TripId AND IsDeleted = 0
            ORDER BY RatedAt DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@TripId", tripId);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            driverRatings.Add(MapDriverRatingFromReader(reader));
        }

        return driverRatings;
    }

    public async Task<IEnumerable<DriverRating>> GetActiveDriverRatingsAsync()
    {
        var driverRatings = new List<DriverRating>();
        const string sql = @"
            SELECT DriverRatingID, TripID, DriverID, UserID, Rating, Comment,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM DriverRatings
            WHERE IsDeleted = 0
            ORDER BY CreatedDate DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            driverRatings.Add(MapDriverRatingFromReader(reader));
        }

        return driverRatings;
    }

    public async Task<IEnumerable<DriverRating>> GetDriverRatingsPaginatedAsync(int pageNumber, int pageSize)
    {
        var driverRatings = new List<DriverRating>();
        const string sql = @"
            SELECT DriverRatingID, TripID, DriverID, UserID, Rating, Comment,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM DriverRatings
            ORDER BY CreatedDate DESC
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Offset", (pageNumber - 1) * pageSize);
        command.Parameters.AddWithValue("@PageSize", pageSize);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            driverRatings.Add(MapDriverRatingFromReader(reader));
        }

        return driverRatings;
    }

    public async Task<int> GetTotalDriverRatingsCountAsync()
    {
        const string sql = "SELECT COUNT(*) FROM DriverRatings";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<int> GetActiveDriverRatingsCountAsync()
    {
        const string sql = "SELECT COUNT(*) FROM DriverRatings WHERE IsDeleted = 0";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<decimal> GetAverageRatingByDriverIdAsync(int driverId)
    {
        const string sql = "SELECT AVG(CAST(Rating AS FLOAT)) FROM DriverRatings WHERE DriverID = @DriverId AND IsDeleted = 0";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@DriverId", driverId);

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();
        return result == DBNull.Value ? 0.0m : Convert.ToDecimal(result);
    }

    public async Task<IEnumerable<DriverRating>> GetDriverRatingsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var driverRatings = new List<DriverRating>();
        const string sql = @"
            SELECT DriverRatingID, TripID, DriverID, UserID, Rating, Comment,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM DriverRatings
            WHERE CreatedDate >= @StartDate AND CreatedDate <= @EndDate
            ORDER BY CreatedDate DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@StartDate", startDate);
        command.Parameters.AddWithValue("@EndDate", endDate);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            driverRatings.Add(MapDriverRatingFromReader(reader));
        }

        return driverRatings;
    }

    public async Task<IEnumerable<DriverRating>> GetDriverRatingsByRatingValueAsync(decimal rating)
    {
        var driverRatings = new List<DriverRating>();
        const string sql = @"
            SELECT DriverRatingID, TripID, DriverID, UserID, Rating, Comment,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM DriverRatings
            WHERE Rating = @Rating AND IsDeleted = 0
            ORDER BY CreatedDate DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Rating", rating);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            driverRatings.Add(MapDriverRatingFromReader(reader));
        }

        return driverRatings;
    }

    public async Task<int> CreateDriverRatingAsync(DriverRating driverRating)
    {
        const string sql = @"
            INSERT INTO DriverRatings (TripID, DriverID, UserID, Rating, Comment, CreatedBy, CreatedDate, IsDeleted)
            VALUES (@TripID, @DriverID, @UserID, @Rating, @Comment, @CreatedBy, @CreatedDate, @IsDeleted);
            SELECT SCOPE_IDENTITY();";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        
        command.Parameters.AddWithValue("@TripID", driverRating.TripID ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@DriverID", driverRating.DriverID ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@UserID", driverRating.UserID ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Rating", driverRating.Rating ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Comment", driverRating.Comment ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@CreatedBy", driverRating.CreatedBy ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@CreatedDate", driverRating.CreatedDate);
        command.Parameters.AddWithValue("@IsDeleted", driverRating.IsDeleted);

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<bool> UpdateDriverRatingAsync(DriverRating driverRating)
    {
        const string sql = @"
            UPDATE DriverRatings 
            SET TripID = @TripID, DriverID = @DriverID, UserID = @UserID, 
                Rating = @Rating, Comment = @Comment, UpdatedBy = @UpdatedBy, UpdatedDate = @UpdatedDate
            WHERE DriverRatingID = @DriverRatingID";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        
        command.Parameters.AddWithValue("@DriverRatingID", driverRating.DriverRatingID);
        command.Parameters.AddWithValue("@TripID", driverRating.TripID ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@DriverID", driverRating.DriverID ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@UserID", driverRating.UserID ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Rating", driverRating.Rating ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Comment", driverRating.Comment ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@UpdatedBy", driverRating.UpdatedBy ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@UpdatedDate", driverRating.UpdatedDate ?? (object)DBNull.Value);

        await connection.OpenAsync();
        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteDriverRatingAsync(int ratingId)
    {
        const string sql = @"
            UPDATE DriverRatings 
            SET IsDeleted = 1, DeletedDate = @DeletedDate 
            WHERE DriverRatingID = @DriverRatingID";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@DriverRatingID", ratingId);
        command.Parameters.AddWithValue("@DeletedDate", DateTime.UtcNow);

        await connection.OpenAsync();
        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    private static DriverRating MapDriverRatingFromReader(SqlDataReader reader)
    {
        return new DriverRating
        {
            DriverRatingID = reader.GetInt32("DriverRatingID"),
            TripID = reader.IsDBNull("TripID") ? null : reader.GetInt32("TripID"),
            DriverID = reader.IsDBNull("DriverID") ? null : reader.GetInt32("DriverID"),
            UserID = reader.IsDBNull("UserID") ? null : reader.GetInt32("UserID"),
            Rating = reader.IsDBNull("Rating") ? null : reader.GetDecimal("Rating"),
            Comment = reader.IsDBNull("Comment") ? null : reader.GetString("Comment"),
            CreatedBy = reader.IsDBNull("CreatedBy") ? null : reader.GetInt32("CreatedBy"),
            CreatedDate = reader.GetDateTime("CreatedDate"),
            UpdatedBy = reader.IsDBNull("UpdatedBy") ? null : reader.GetInt32("UpdatedBy"),
            UpdatedDate = reader.IsDBNull("UpdatedDate") ? null : reader.GetDateTime("UpdatedDate"),
            DeletedBy = reader.IsDBNull("DeletedBy") ? null : reader.GetInt32("DeletedBy"),
            DeletedDate = reader.IsDBNull("DeletedDate") ? null : reader.GetDateTime("DeletedDate"),
            IsDeleted = reader.GetBoolean("IsDeleted")
        };
    }
}
