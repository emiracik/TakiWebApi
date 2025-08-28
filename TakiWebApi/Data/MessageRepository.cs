using System.Data.SqlClient;
using TakiWebApi.Models;

namespace TakiWebApi.Data;

public class MessageRepository : IMessageRepository
{
    private readonly string _connectionString;
    public MessageRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection");
    }

    public async Task<IEnumerable<Message>> GetAllAsync()
    {
        var list = new List<Message>();
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("SELECT * FROM Messages", connection);
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new Message
            {
                MessageID = reader.GetInt32(reader.GetOrdinal("MessageID")),
                SenderID = reader.GetInt32(reader.GetOrdinal("SenderID")),
                ReceiverID = reader.GetInt32(reader.GetOrdinal("ReceiverID")),
                Content = reader.GetString(reader.GetOrdinal("Content")),
                SentAt = reader.GetDateTime(reader.GetOrdinal("SentAt")),
                IsRead = reader.GetBoolean(reader.GetOrdinal("IsRead"))
            });
        }
        return list;
    }

    public async Task<Message?> GetByIdAsync(int messageId)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("SELECT * FROM Messages WHERE MessageID = @MessageID", connection);
        command.Parameters.AddWithValue("@MessageID", messageId);
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Message
            {
                MessageID = reader.GetInt32(reader.GetOrdinal("MessageID")),
                SenderID = reader.GetInt32(reader.GetOrdinal("SenderID")),
                ReceiverID = reader.GetInt32(reader.GetOrdinal("ReceiverID")),
                Content = reader.GetString(reader.GetOrdinal("Content")),
                SentAt = reader.GetDateTime(reader.GetOrdinal("SentAt")),
                IsRead = reader.GetBoolean(reader.GetOrdinal("IsRead"))
            };
        }
        return null;
    }

    public async Task<IEnumerable<Message>> GetByUserIdAsync(int userId)
    {
        var list = new List<Message>();
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("SELECT * FROM Messages WHERE SenderID = @UserID OR ReceiverID = @UserID", connection);
        command.Parameters.AddWithValue("@UserID", userId);
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new Message
            {
                MessageID = reader.GetInt32(reader.GetOrdinal("MessageID")),
                SenderID = reader.GetInt32(reader.GetOrdinal("SenderID")),
                ReceiverID = reader.GetInt32(reader.GetOrdinal("ReceiverID")),
                Content = reader.GetString(reader.GetOrdinal("Content")),
                SentAt = reader.GetDateTime(reader.GetOrdinal("SentAt")),
                IsRead = reader.GetBoolean(reader.GetOrdinal("IsRead"))
            });
        }
        return list;
    }

    public async Task<int> CreateAsync(Message message)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(
            @"INSERT INTO Messages (SenderID, ReceiverID, Content, SentAt, IsRead)
              VALUES (@SenderID, @ReceiverID, @Content, @SentAt, @IsRead);
              SELECT SCOPE_IDENTITY();", connection);
        command.Parameters.AddWithValue("@SenderID", message.SenderID);
        command.Parameters.AddWithValue("@ReceiverID", message.ReceiverID);
        command.Parameters.AddWithValue("@Content", message.Content);
        command.Parameters.AddWithValue("@SentAt", message.SentAt);
        command.Parameters.AddWithValue("@IsRead", message.IsRead);
        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<bool> MarkAsReadAsync(int messageId)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("UPDATE Messages SET IsRead = 1 WHERE MessageID = @MessageID", connection);
        command.Parameters.AddWithValue("@MessageID", messageId);
        await connection.OpenAsync();
        var affected = await command.ExecuteNonQueryAsync();
        return affected > 0;
    }

    public async Task<bool> DeleteAsync(int messageId)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("DELETE FROM Messages WHERE MessageID = @MessageID", connection);
        command.Parameters.AddWithValue("@MessageID", messageId);
        await connection.OpenAsync();
        var affected = await command.ExecuteNonQueryAsync();
        return affected > 0;
    }
}
