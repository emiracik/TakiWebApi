using System.Data;
using Microsoft.Data.SqlClient;
using TakiWebApi.Models;

namespace TakiWebApi.Data;

public class UserRatingRepository : IUserRatingRepository
{
    private readonly string _connectionString;

    public UserRatingRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("SqlServerConnection")
            ?? throw new ArgumentNullException(nameof(configuration), "Connection string cannot be null");
    }

    public async Task<IEnumerable<UserRating>> GetAllAsync()
    {
        var ratings = new List<UserRating>();
        const string sql = "SELECT * FROM UserRatings WHERE IsDeleted = 0 ORDER BY CreatedDate DESC";
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            ratings.Add(MapFromReader(reader));
        }
        return ratings;
    }

    public async Task<UserRating?> GetByIdAsync(int id)
    {
        const string sql = "SELECT * FROM UserRatings WHERE UserRatingID = @Id AND IsDeleted = 0";
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapFromReader(reader);
        }
        return null;
    }

    public async Task<IEnumerable<UserRating>> GetByUserIdAsync(int userId)
    {
        var ratings = new List<UserRating>();
        const string sql = "SELECT * FROM UserRatings WHERE RatedUserID = @UserId AND IsDeleted = 0 ORDER BY CreatedDate DESC";
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@UserId", userId);
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            ratings.Add(MapFromReader(reader));
        }
        return ratings;
    }

    public async Task<IEnumerable<UserRating>> GetByDriverIdAsync(int driverId)
    {
        var ratings = new List<UserRating>();
        const string sql = "SELECT * FROM UserRatings WHERE RatedByDriverID = @DriverId AND IsDeleted = 0 ORDER BY CreatedDate DESC";
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@DriverId", driverId);
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            ratings.Add(MapFromReader(reader));
        }
        return ratings;
    }

    public async Task<IEnumerable<UserRating>> GetByTripIdAsync(int tripId)
    {
        var ratings = new List<UserRating>();
        const string sql = "SELECT * FROM UserRatings WHERE TripID = @TripId AND IsDeleted = 0 ORDER BY CreatedDate DESC";
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@TripId", tripId);
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            ratings.Add(MapFromReader(reader));
        }
        return ratings;
    }

    public async Task<decimal> GetAverageByUserIdAsync(int userId)
    {
        const string sql = "SELECT AVG(Rating) FROM UserRatings WHERE RatedUserID = @UserId AND IsDeleted = 0";
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@UserId", userId);
        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();
        return result != DBNull.Value ? Convert.ToDecimal(result) : 0;
    }

    public async Task AddAsync(UserRating rating)
    {
        const string sql = @"INSERT INTO UserRatings (TripID, RatedUserID, RatedByDriverID, Rating, Comment, CreatedBy, CreatedDate, IsDeleted)
            VALUES (@TripID, @RatedUserID, @RatedByDriverID, @Rating, @Comment, @CreatedBy, @CreatedDate, 0)";
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@TripID", rating.TripID);
        command.Parameters.AddWithValue("@RatedUserID", rating.RatedUserID);
        command.Parameters.AddWithValue("@RatedByDriverID", rating.RatedByDriverID);
        command.Parameters.AddWithValue("@Rating", rating.Rating);
        command.Parameters.AddWithValue("@Comment", rating.Comment ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@CreatedBy", rating.CreatedBy ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@CreatedDate", rating.CreatedDate);
        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    private UserRating MapFromReader(SqlDataReader reader)
    {
        return new UserRating
        {
            UserRatingID = reader.GetInt32(reader.GetOrdinal("UserRatingID")),
            TripID = reader.GetInt32(reader.GetOrdinal("TripID")),
            RatedUserID = reader.GetInt32(reader.GetOrdinal("RatedUserID")),
            RatedByDriverID = reader.GetInt32(reader.GetOrdinal("RatedByDriverID")),
            Rating = reader.GetDecimal(reader.GetOrdinal("Rating")),
            Comment = reader["Comment"] as string,
            CreatedBy = reader["CreatedBy"] as int?,
            CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
            UpdatedBy = reader["UpdatedBy"] as int?,
            UpdatedDate = reader["UpdatedDate"] as DateTime?,
            DeletedBy = reader["DeletedBy"] as int?,
            DeletedDate = reader["DeletedDate"] as DateTime?,
            IsDeleted = reader.GetBoolean(reader.GetOrdinal("IsDeleted"))
        };
    }
}
