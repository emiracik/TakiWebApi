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
            SELECT NotificationID, Title, Message, UserID, DriverID, NotificationType, IsRead, ReadDate,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM Notifications
            WHERE IsDeleted = 0
            ORDER BY CreatedDate DESC";

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
            SELECT NotificationID, Title, Message, UserID, DriverID, NotificationType, IsRead, ReadDate,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM Notifications
            WHERE NotificationID = @NotificationId AND IsDeleted = 0";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@NotificationId", SqlDbType.Int).Value = notificationId;

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
            SELECT NotificationID, Title, Message, UserID, DriverID, NotificationType, IsRead, ReadDate,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM Notifications
            WHERE UserID = @UserId AND IsDeleted = 0
            ORDER BY CreatedDate DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            notifications.Add(MapNotificationFromReader(reader));
        }

        return notifications;
    }

    public async Task<IEnumerable<Notification>> GetNotificationsByDriverIdAsync(int driverId)
    {
        var notifications = new List<Notification>();
        const string sql = @"
            SELECT NotificationID, Title, Message, UserID, DriverID, NotificationType, IsRead, ReadDate,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM Notifications
            WHERE DriverID = @DriverId AND IsDeleted = 0
            ORDER BY CreatedDate DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@DriverId", SqlDbType.Int).Value = driverId;

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            notifications.Add(MapNotificationFromReader(reader));
        }

        return notifications;
    }

    public async Task<IEnumerable<Notification>> GetUnreadNotificationsAsync()
    {
        var notifications = new List<Notification>();
        const string sql = @"
            SELECT NotificationID, Title, Message, UserID, DriverID, NotificationType, IsRead, ReadDate,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM Notifications
            WHERE IsRead = 0 AND IsDeleted = 0
            ORDER BY CreatedDate DESC";

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
        var offset = (pageNumber - 1) * pageSize;

        const string sql = @"
            SELECT NotificationID, Title, Message, UserID, DriverID, NotificationType, IsRead, ReadDate,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM Notifications
            WHERE IsDeleted = 0
            ORDER BY CreatedDate DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Offset", SqlDbType.Int).Value = offset;
        command.Parameters.Add("@PageSize", SqlDbType.Int).Value = pageSize;

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
        const string sql = "SELECT COUNT(*) FROM Notifications WHERE IsDeleted = 0";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();
        return result != null ? (int)result : 0;
    }

    public async Task<int> GetUnreadNotificationsCountAsync()
    {
        const string sql = "SELECT COUNT(*) FROM Notifications WHERE IsRead = 0 AND IsDeleted = 0";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();
        return result != null ? (int)result : 0;
    }

    public async Task<IEnumerable<Notification>> SearchNotificationsByTitleAsync(string searchTerm)
    {
        var notifications = new List<Notification>();
        const string sql = @"
            SELECT NotificationID, Title, Message, UserID, DriverID, NotificationType, IsRead, ReadDate,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM Notifications
            WHERE IsDeleted = 0 AND (Title LIKE @SearchTerm OR Message LIKE @SearchTerm)
            ORDER BY CreatedDate DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@SearchTerm", SqlDbType.NVarChar, 255).Value = $"%{searchTerm}%";

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            notifications.Add(MapNotificationFromReader(reader));
        }

        return notifications;
    }

    public async Task<IEnumerable<Notification>> GetNotificationsByCreatedDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var notifications = new List<Notification>();
        const string sql = @"
            SELECT NotificationID, Title, Message, UserID, DriverID, NotificationType, IsRead, ReadDate,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM Notifications
            WHERE IsDeleted = 0 AND CreatedDate >= @StartDate AND CreatedDate <= @EndDate
            ORDER BY CreatedDate DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@StartDate", SqlDbType.DateTime).Value = startDate;
        command.Parameters.Add("@EndDate", SqlDbType.DateTime).Value = endDate;

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            notifications.Add(MapNotificationFromReader(reader));
        }

        return notifications;
    }

    public async Task<int> CreateNotificationAsync(Notification notification)
    {
        const string sql = @"
            INSERT INTO Notifications (Title, Message, UserID, DriverID, NotificationType, IsRead, CreatedBy, CreatedDate)
            OUTPUT INSERTED.NotificationID
            VALUES (@Title, @Message, @UserID, @DriverID, @NotificationType, @IsRead, @CreatedBy, @CreatedDate)";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        command.Parameters.Add("@Title", SqlDbType.NVarChar, 200).Value = (object?)notification.Title ?? DBNull.Value;
        command.Parameters.Add("@Message", SqlDbType.NText).Value = (object?)notification.Message ?? DBNull.Value;
        command.Parameters.Add("@UserID", SqlDbType.Int).Value = (object?)notification.UserID ?? DBNull.Value;
        command.Parameters.Add("@DriverID", SqlDbType.Int).Value = (object?)notification.DriverID ?? DBNull.Value;
        command.Parameters.Add("@NotificationType", SqlDbType.NVarChar, 50).Value = (object?)notification.NotificationType ?? DBNull.Value;
        command.Parameters.Add("@IsRead", SqlDbType.Bit).Value = notification.IsRead;
        command.Parameters.Add("@CreatedBy", SqlDbType.Int).Value = (object?)notification.CreatedBy ?? DBNull.Value;
        command.Parameters.Add("@CreatedDate", SqlDbType.DateTime).Value = DateTime.UtcNow;

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();
        return result != null ? (int)result : 0;
    }

    public async Task<bool> UpdateNotificationAsync(Notification notification)
    {
        const string sql = @"
            UPDATE Notifications 
            SET Title = @Title, Message = @Message, UserID = @UserID, DriverID = @DriverID,
                NotificationType = @NotificationType, IsRead = @IsRead, ReadDate = @ReadDate,
                UpdatedBy = @UpdatedBy, UpdatedDate = @UpdatedDate
            WHERE NotificationID = @NotificationID AND IsDeleted = 0";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        command.Parameters.Add("@NotificationID", SqlDbType.Int).Value = notification.NotificationID;
        command.Parameters.Add("@Title", SqlDbType.NVarChar, 200).Value = (object?)notification.Title ?? DBNull.Value;
        command.Parameters.Add("@Message", SqlDbType.NText).Value = (object?)notification.Message ?? DBNull.Value;
        command.Parameters.Add("@UserID", SqlDbType.Int).Value = (object?)notification.UserID ?? DBNull.Value;
        command.Parameters.Add("@DriverID", SqlDbType.Int).Value = (object?)notification.DriverID ?? DBNull.Value;
        command.Parameters.Add("@NotificationType", SqlDbType.NVarChar, 50).Value = (object?)notification.NotificationType ?? DBNull.Value;
        command.Parameters.Add("@IsRead", SqlDbType.Bit).Value = notification.IsRead;
        command.Parameters.Add("@ReadDate", SqlDbType.DateTime).Value = (object?)notification.ReadDate ?? DBNull.Value;
        command.Parameters.Add("@UpdatedBy", SqlDbType.Int).Value = (object?)notification.UpdatedBy ?? DBNull.Value;
        command.Parameters.Add("@UpdatedDate", SqlDbType.DateTime).Value = DateTime.UtcNow;

        await connection.OpenAsync();
        var rowsAffected = await command.ExecuteNonQueryAsync();

        return rowsAffected > 0;
    }

    public async Task<bool> MarkAsReadAsync(int notificationId)
    {
        const string sql = @"
            UPDATE Notifications 
            SET IsRead = 1, ReadDate = @ReadDate, UpdatedDate = @UpdatedDate
            WHERE NotificationID = @NotificationID AND IsDeleted = 0";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        command.Parameters.Add("@NotificationID", SqlDbType.Int).Value = notificationId;
        command.Parameters.Add("@ReadDate", SqlDbType.DateTime).Value = DateTime.UtcNow;
        command.Parameters.Add("@UpdatedDate", SqlDbType.DateTime).Value = DateTime.UtcNow;

        await connection.OpenAsync();
        var rowsAffected = await command.ExecuteNonQueryAsync();

        return rowsAffected > 0;
    }

    public async Task<bool> DeleteNotificationAsync(int notificationId)
    {
        const string sql = @"
            UPDATE Notifications 
            SET IsDeleted = 1, DeletedDate = @DeletedDate, DeletedBy = @DeletedBy
            WHERE NotificationID = @NotificationID";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        command.Parameters.Add("@NotificationID", SqlDbType.Int).Value = notificationId;
        command.Parameters.Add("@DeletedDate", SqlDbType.DateTime).Value = DateTime.UtcNow;
        command.Parameters.Add("@DeletedBy", SqlDbType.Int).Value = DBNull.Value; // Should be set based on current user

        await connection.OpenAsync();
        var rowsAffected = await command.ExecuteNonQueryAsync();

        return rowsAffected > 0;
    }

    private static Notification MapNotificationFromReader(SqlDataReader reader)
    {
        return new Notification
        {
            NotificationID = reader.GetInt32("NotificationID"),
            Title = reader.IsDBNull("Title") ? null : reader.GetString("Title"),
            Message = reader.IsDBNull("Message") ? null : reader.GetString("Message"),
            UserID = reader.IsDBNull("UserID") ? null : reader.GetInt32("UserID"),
            DriverID = reader.IsDBNull("DriverID") ? null : reader.GetInt32("DriverID"),
            NotificationType = reader.IsDBNull("NotificationType") ? null : reader.GetString("NotificationType"),
            IsRead = reader.GetBoolean("IsRead"),
            ReadDate = reader.IsDBNull("ReadDate") ? null : reader.GetDateTime("ReadDate"),
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