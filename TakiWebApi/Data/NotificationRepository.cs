using System.Data;
using Microsoft.Data.SqlClient;
using TakiWebApi.Models;

namespace TakiWebApi.Data;

public class NotificationRepository : INotificationRepository
{
    private readonly string _connectionString;

    public NotificationRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("SqlServerConnection") 
            ?? throw new ArgumentNullException(nameof(configuration), "Connection string cannot be null");
    }

    public async Task<IEnumerable<Notification>> GetAllNotificationsAsync()
    {
        var notifications = new List<Notification>();
        const string sql = @"
            SELECT NotificationID, UserID, Title, Message, IsRead, SentAt,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM Notifications
            ORDER BY SentAt DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            notifications.Add(MapNotificationFromReader(reader));
        }

        return notifications;
    }

    public async Task<Notification?> GetNotificationByIdAsync(int notificationId)
    {
        const string sql = @"
            SELECT NotificationID, UserID, Title, Message, IsRead, SentAt,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM Notifications
            WHERE NotificationID = @NotificationId";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@NotificationId", notificationId);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapNotificationFromReader(reader);
        }

        return null;
    }

    public async Task<IEnumerable<Notification>> GetNotificationsByUserIdAsync(int userId)
    {
        var notifications = new List<Notification>();
        const string sql = @"
            SELECT NotificationID, UserID, Title, Message, IsRead, SentAt,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM Notifications
            WHERE UserID = @UserId AND IsDeleted = 0
            ORDER BY SentAt DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@UserId", userId);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            notifications.Add(MapNotificationFromReader(reader));
        }

        return notifications;
    }

    public async Task<IEnumerable<Notification>> GetUnreadNotificationsByUserIdAsync(int userId)
    {
        var notifications = new List<Notification>();
        const string sql = @"
            SELECT NotificationID, UserID, Title, Message, IsRead, SentAt,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM Notifications
            WHERE UserID = @UserId AND IsRead = 0 AND IsDeleted = 0
            ORDER BY SentAt DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@UserId", userId);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            notifications.Add(MapNotificationFromReader(reader));
        }

        return notifications;
    }

    public async Task<IEnumerable<Notification>> GetActiveNotificationsAsync()
    {
        var notifications = new List<Notification>();
        const string sql = @"
            SELECT NotificationID, UserID, Title, Message, IsRead, SentAt,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM Notifications
            WHERE IsDeleted = 0
            ORDER BY SentAt DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            notifications.Add(MapNotificationFromReader(reader));
        }

        return notifications;
    }

    public async Task<IEnumerable<Notification>> GetNotificationsPaginatedAsync(int pageNumber, int pageSize)
    {
        var notifications = new List<Notification>();
        const string sql = @"
            SELECT NotificationID, UserID, Title, Message, IsRead, SentAt,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM Notifications
            ORDER BY SentAt DESC
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
            notifications.Add(MapNotificationFromReader(reader));
        }

        return notifications;
    }

    public async Task<int> GetTotalNotificationsCountAsync()
    {
        const string sql = "SELECT COUNT(*) FROM Notifications";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<int> GetActiveNotificationsCountAsync()
    {
        const string sql = "SELECT COUNT(*) FROM Notifications WHERE IsDeleted = 0";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<int> GetUnreadNotificationsCountByUserIdAsync(int userId)
    {
        const string sql = "SELECT COUNT(*) FROM Notifications WHERE UserID = @UserId AND IsRead = 0 AND IsDeleted = 0";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@UserId", userId);

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<IEnumerable<Notification>> SearchNotificationsByTitleAsync(string searchTerm)
    {
        var notifications = new List<Notification>();
        const string sql = @"
            SELECT NotificationID, UserID, Title, Message, IsRead, SentAt,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM Notifications
            WHERE Title LIKE @SearchTerm OR Message LIKE @SearchTerm
            ORDER BY SentAt DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            notifications.Add(MapNotificationFromReader(reader));
        }

        return notifications;
    }

    public async Task<IEnumerable<Notification>> GetNotificationsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var notifications = new List<Notification>();
        const string sql = @"
            SELECT NotificationID, UserID, Title, Message, IsRead, SentAt,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM Notifications
            WHERE SentAt >= @StartDate AND SentAt <= @EndDate
            ORDER BY SentAt DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@StartDate", startDate);
        command.Parameters.AddWithValue("@EndDate", endDate);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            notifications.Add(MapNotificationFromReader(reader));
        }

        return notifications;
    }

    public async Task<bool> MarkNotificationAsReadAsync(int notificationId)
    {
        const string sql = @"
            UPDATE Notifications 
            SET IsRead = 1, UpdatedDate = @UpdatedDate
            WHERE NotificationID = @NotificationId";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@NotificationId", notificationId);
        command.Parameters.AddWithValue("@UpdatedDate", DateTime.UtcNow);

        await connection.OpenAsync();
        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> MarkAllNotificationsAsReadByUserIdAsync(int userId)
    {
        const string sql = @"
            UPDATE Notifications 
            SET IsRead = 1, UpdatedDate = @UpdatedDate
            WHERE UserID = @UserId AND IsRead = 0";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@UserId", userId);
        command.Parameters.AddWithValue("@UpdatedDate", DateTime.UtcNow);

        await connection.OpenAsync();
        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<int> CreateNotificationAsync(Notification notification)
    {
        const string sql = @"
            INSERT INTO Notifications (UserID, Title, Message, IsRead, SentAt, CreatedBy, CreatedDate, IsDeleted)
            VALUES (@UserID, @Title, @Message, @IsRead, @SentAt, @CreatedBy, @CreatedDate, @IsDeleted);
            SELECT SCOPE_IDENTITY();";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        
        command.Parameters.AddWithValue("@UserID", notification.UserID ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Title", notification.Title ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Message", notification.Message ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@IsRead", notification.IsRead);
        command.Parameters.AddWithValue("@SentAt", notification.SentAt);
        command.Parameters.AddWithValue("@CreatedBy", notification.CreatedBy ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@CreatedDate", notification.CreatedDate);
        command.Parameters.AddWithValue("@IsDeleted", notification.IsDeleted);

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<bool> UpdateNotificationAsync(Notification notification)
    {
        const string sql = @"
            UPDATE Notifications 
            SET UserID = @UserID, Title = @Title, Message = @Message, 
                IsRead = @IsRead, UpdatedBy = @UpdatedBy, UpdatedDate = @UpdatedDate
            WHERE NotificationID = @NotificationID";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        
        command.Parameters.AddWithValue("@NotificationID", notification.NotificationID);
        command.Parameters.AddWithValue("@UserID", notification.UserID ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Title", notification.Title ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Message", notification.Message ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@IsRead", notification.IsRead);
        command.Parameters.AddWithValue("@UpdatedBy", notification.UpdatedBy ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@UpdatedDate", notification.UpdatedDate ?? (object)DBNull.Value);

        await connection.OpenAsync();
        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteNotificationAsync(int notificationId)
    {
        const string sql = @"
            UPDATE Notifications 
            SET IsDeleted = 1, DeletedDate = @DeletedDate 
            WHERE NotificationID = @NotificationID";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@NotificationID", notificationId);
        command.Parameters.AddWithValue("@DeletedDate", DateTime.UtcNow);

        await connection.OpenAsync();
        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    private static Notification MapNotificationFromReader(SqlDataReader reader)
    {
        return new Notification
        {
            NotificationID = reader.GetInt32("NotificationID"),
            UserID = reader.IsDBNull("UserID") ? null : reader.GetInt32("UserID"),
            Title = reader.IsDBNull("Title") ? null : reader.GetString("Title"),
            Message = reader.IsDBNull("Message") ? null : reader.GetString("Message"),
            IsRead = reader.GetBoolean("IsRead"),
            SentAt = reader.GetDateTime("SentAt"),
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
