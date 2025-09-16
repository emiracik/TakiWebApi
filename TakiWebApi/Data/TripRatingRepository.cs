using System.Data;
using Microsoft.Data.SqlClient;
using TakiWebApi.Models;

namespace TakiWebApi.Data;

public class TripRatingRepository : ITripRatingRepository
{
    private readonly string _connectionString;

    public TripRatingRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("SqlServerConnection")
            ?? throw new ArgumentNullException(nameof(configuration), "Connection string cannot be null");
    }

    public async Task<IEnumerable<TripRating>> GetAllAsync()
    {
        var ratings = new List<TripRating>();
        const string sql = "SELECT * FROM TripRatings WHERE IsDeleted = 0 ORDER BY CreatedDate DESC";
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

    public async Task<TripRating?> GetByIdAsync(int id)
    {
        const string sql = "SELECT * FROM TripRatings WHERE TripRatingID = @Id AND IsDeleted = 0";
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

    public async Task<IEnumerable<TripRating>> GetByTripIdAsync(int tripId)
    {
        var ratings = new List<TripRating>();
        const string sql = "SELECT * FROM TripRatings WHERE TripID = @TripId AND IsDeleted = 0 ORDER BY CreatedDate DESC";
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

    public async Task<decimal> GetAverageByTripIdAsync(int tripId)
    {
        const string sql = "SELECT AVG(Rating) FROM TripRatings WHERE TripID = @TripId AND IsDeleted = 0";
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@TripId", tripId);
        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();
        return result != DBNull.Value ? Convert.ToDecimal(result) : 0;
    }

    public async Task AddAsync(TripRating rating)
    {
        const string sql = @"INSERT INTO TripRatings (TripID, RatedByUserID, Rating, Comment, CreatedBy, CreatedDate, IsDeleted)
            VALUES (@TripID, @RatedByUserID, @Rating, @Comment, @CreatedBy, @CreatedDate, 0)";
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@TripID", rating.TripID);
        command.Parameters.AddWithValue("@RatedByUserID", rating.RatedByUserID);
        command.Parameters.AddWithValue("@Rating", rating.Rating);
        command.Parameters.AddWithValue("@Comment", rating.Comment ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@CreatedBy", rating.CreatedBy ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@CreatedDate", rating.CreatedDate);
        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    private TripRating MapFromReader(SqlDataReader reader)
    {
        return new TripRating
        {
            TripRatingID = reader.GetInt32(reader.GetOrdinal("TripRatingID")),
            TripID = reader.GetInt32(reader.GetOrdinal("TripID")),
            RatedByUserID = reader.GetInt32(reader.GetOrdinal("RatedByUserID")),
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
