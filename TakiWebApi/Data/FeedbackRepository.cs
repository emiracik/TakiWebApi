using System.Data.SqlClient;
using TakiWebApi.Models;

namespace TakiWebApi.Data;

public class FeedbackRepository : IFeedbackRepository
{
    private readonly string _connectionString;
    public FeedbackRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection");
    }

    public async Task<IEnumerable<Feedback>> GetAllAsync()
    {
        var list = new List<Feedback>();
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("SELECT * FROM Feedbacks", connection);
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new Feedback
            {
                FeedbackID = reader.GetInt32(reader.GetOrdinal("FeedbackID")),
                UserID = reader.IsDBNull(reader.GetOrdinal("UserID")) ? null : reader.GetInt32(reader.GetOrdinal("UserID")),
                DriverID = reader.IsDBNull(reader.GetOrdinal("DriverID")) ? null : reader.GetInt32(reader.GetOrdinal("DriverID")),
                TripID = reader.IsDBNull(reader.GetOrdinal("TripID")) ? null : reader.GetInt32(reader.GetOrdinal("TripID")),
                Rating = reader.GetDecimal(reader.GetOrdinal("Rating")),
                Comment = reader.IsDBNull(reader.GetOrdinal("Comment")) ? null : reader.GetString(reader.GetOrdinal("Comment")),
                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate"))
            });
        }
        return list;
    }

    public async Task<Feedback?> GetByIdAsync(int feedbackId)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("SELECT * FROM Feedbacks WHERE FeedbackID = @FeedbackID", connection);
        command.Parameters.AddWithValue("@FeedbackID", feedbackId);
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Feedback
            {
                FeedbackID = reader.GetInt32(reader.GetOrdinal("FeedbackID")),
                UserID = reader.IsDBNull(reader.GetOrdinal("UserID")) ? null : reader.GetInt32(reader.GetOrdinal("UserID")),
                DriverID = reader.IsDBNull(reader.GetOrdinal("DriverID")) ? null : reader.GetInt32(reader.GetOrdinal("DriverID")),
                TripID = reader.IsDBNull(reader.GetOrdinal("TripID")) ? null : reader.GetInt32(reader.GetOrdinal("TripID")),
                Rating = reader.GetDecimal(reader.GetOrdinal("Rating")),
                Comment = reader.IsDBNull(reader.GetOrdinal("Comment")) ? null : reader.GetString(reader.GetOrdinal("Comment")),
                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate"))
            };
        }
        return null;
    }

    public async Task<IEnumerable<Feedback>> GetByUserIdAsync(int userId)
    {
        var list = new List<Feedback>();
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("SELECT * FROM Feedbacks WHERE UserID = @UserID", connection);
        command.Parameters.AddWithValue("@UserID", userId);
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new Feedback
            {
                FeedbackID = reader.GetInt32(reader.GetOrdinal("FeedbackID")),
                UserID = reader.IsDBNull(reader.GetOrdinal("UserID")) ? null : reader.GetInt32(reader.GetOrdinal("UserID")),
                DriverID = reader.IsDBNull(reader.GetOrdinal("DriverID")) ? null : reader.GetInt32(reader.GetOrdinal("DriverID")),
                TripID = reader.IsDBNull(reader.GetOrdinal("TripID")) ? null : reader.GetInt32(reader.GetOrdinal("TripID")),
                Rating = reader.GetDecimal(reader.GetOrdinal("Rating")),
                Comment = reader.IsDBNull(reader.GetOrdinal("Comment")) ? null : reader.GetString(reader.GetOrdinal("Comment")),
                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate"))
            });
        }
        return list;
    }

    public async Task<IEnumerable<Feedback>> GetByDriverIdAsync(int driverId)
    {
        var list = new List<Feedback>();
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("SELECT * FROM Feedbacks WHERE DriverID = @DriverID", connection);
        command.Parameters.AddWithValue("@DriverID", driverId);
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new Feedback
            {
                FeedbackID = reader.GetInt32(reader.GetOrdinal("FeedbackID")),
                UserID = reader.IsDBNull(reader.GetOrdinal("UserID")) ? null : reader.GetInt32(reader.GetOrdinal("UserID")),
                DriverID = reader.IsDBNull(reader.GetOrdinal("DriverID")) ? null : reader.GetInt32(reader.GetOrdinal("DriverID")),
                TripID = reader.IsDBNull(reader.GetOrdinal("TripID")) ? null : reader.GetInt32(reader.GetOrdinal("TripID")),
                Rating = reader.GetDecimal(reader.GetOrdinal("Rating")),
                Comment = reader.IsDBNull(reader.GetOrdinal("Comment")) ? null : reader.GetString(reader.GetOrdinal("Comment")),
                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate"))
            });
        }
        return list;
    }

    public async Task<IEnumerable<Feedback>> GetByTripIdAsync(int tripId)
    {
        var list = new List<Feedback>();
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("SELECT * FROM Feedbacks WHERE TripID = @TripID", connection);
        command.Parameters.AddWithValue("@TripID", tripId);
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new Feedback
            {
                FeedbackID = reader.GetInt32(reader.GetOrdinal("FeedbackID")),
                UserID = reader.IsDBNull(reader.GetOrdinal("UserID")) ? null : reader.GetInt32(reader.GetOrdinal("UserID")),
                DriverID = reader.IsDBNull(reader.GetOrdinal("DriverID")) ? null : reader.GetInt32(reader.GetOrdinal("DriverID")),
                TripID = reader.IsDBNull(reader.GetOrdinal("TripID")) ? null : reader.GetInt32(reader.GetOrdinal("TripID")),
                Rating = reader.GetDecimal(reader.GetOrdinal("Rating")),
                Comment = reader.IsDBNull(reader.GetOrdinal("Comment")) ? null : reader.GetString(reader.GetOrdinal("Comment")),
                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate"))
            });
        }
        return list;
    }

    public async Task<int> CreateAsync(Feedback feedback)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(
            @"INSERT INTO Feedbacks (UserID, DriverID, TripID, Rating, Comment, CreatedDate)
              VALUES (@UserID, @DriverID, @TripID, @Rating, @Comment, @CreatedDate);
              SELECT SCOPE_IDENTITY();", connection);
        command.Parameters.AddWithValue("@UserID", (object?)feedback.UserID ?? DBNull.Value);
        command.Parameters.AddWithValue("@DriverID", (object?)feedback.DriverID ?? DBNull.Value);
        command.Parameters.AddWithValue("@TripID", (object?)feedback.TripID ?? DBNull.Value);
        command.Parameters.AddWithValue("@Rating", feedback.Rating);
        command.Parameters.AddWithValue("@Comment", (object?)feedback.Comment ?? DBNull.Value);
        command.Parameters.AddWithValue("@CreatedDate", feedback.CreatedDate);
        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<bool> DeleteAsync(int feedbackId)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("DELETE FROM Feedbacks WHERE FeedbackID = @FeedbackID", connection);
        command.Parameters.AddWithValue("@FeedbackID", feedbackId);
        await connection.OpenAsync();
        var affected = await command.ExecuteNonQueryAsync();
        return affected > 0;
    }
}
