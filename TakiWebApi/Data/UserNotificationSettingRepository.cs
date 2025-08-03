using System.Data;
using Microsoft.Data.SqlClient;
using TakiWebApi.Models;

namespace TakiWebApi.Data;

public class UserNotificationSettingRepository : IUserNotificationSettingRepository
{
    private readonly string _connectionString;

    public UserNotificationSettingRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("SqlServerConnection") 
            ?? throw new ArgumentNullException(nameof(configuration), "Connection string cannot be null");
    }

    public async Task<IEnumerable<UserNotificationSetting>> GetAllUserNotificationSettingsAsync()
    {
        var settings = new List<UserNotificationSetting>();
        const string sql = @"
            SELECT SettingID, UserID, AllowPromotions, AllowTripUpdates, AllowNews, CreatedDate
            FROM UserNotificationSettings
            ORDER BY CreatedDate DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            settings.Add(MapUserNotificationSettingFromReader(reader));
        }

        return settings;
    }

    public async Task<UserNotificationSetting?> GetUserNotificationSettingByIdAsync(int settingId)
    {
        const string sql = @"
            SELECT SettingID, UserID, AllowPromotions, AllowTripUpdates, AllowNews, CreatedDate
            FROM UserNotificationSettings
            WHERE SettingID = @SettingId";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@SettingId", settingId);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapUserNotificationSettingFromReader(reader);
        }

        return null;
    }

    public async Task<UserNotificationSetting?> GetUserNotificationSettingByUserIdAsync(int userId)
    {
        const string sql = @"
            SELECT SettingID, UserID, AllowPromotions, AllowTripUpdates, AllowNews, CreatedDate
            FROM UserNotificationSettings
            WHERE UserID = @UserId";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@UserId", userId);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapUserNotificationSettingFromReader(reader);
        }

        return null;
    }

    public async Task<IEnumerable<UserNotificationSetting>> GetUserNotificationSettingsPaginatedAsync(int pageNumber, int pageSize)
    {
        var settings = new List<UserNotificationSetting>();
        const string sql = @"
            SELECT SettingID, UserID, AllowPromotions, AllowTripUpdates, AllowNews, CreatedDate
            FROM UserNotificationSettings
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
            settings.Add(MapUserNotificationSettingFromReader(reader));
        }

        return settings;
    }

    public async Task<int> GetTotalUserNotificationSettingsCountAsync()
    {
        const string sql = "SELECT COUNT(*) FROM UserNotificationSettings";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<int> CreateUserNotificationSettingAsync(UserNotificationSetting userNotificationSetting)
    {
        const string sql = @"
            INSERT INTO UserNotificationSettings (UserID, AllowPromotions, AllowTripUpdates, AllowNews, CreatedDate)
            VALUES (@UserID, @AllowPromotions, @AllowTripUpdates, @AllowNews, @CreatedDate);
            SELECT SCOPE_IDENTITY();";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        
        command.Parameters.AddWithValue("@UserID", userNotificationSetting.UserID ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@AllowPromotions", userNotificationSetting.AllowPromotions);
        command.Parameters.AddWithValue("@AllowTripUpdates", userNotificationSetting.AllowTripUpdates);
        command.Parameters.AddWithValue("@AllowNews", userNotificationSetting.AllowNews);
        command.Parameters.AddWithValue("@CreatedDate", userNotificationSetting.CreatedDate);

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<bool> UpdateUserNotificationSettingAsync(UserNotificationSetting userNotificationSetting)
    {
        const string sql = @"
            UPDATE UserNotificationSettings 
            SET UserID = @UserID, AllowPromotions = @AllowPromotions, 
                AllowTripUpdates = @AllowTripUpdates, AllowNews = @AllowNews
            WHERE SettingID = @SettingID";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        
        command.Parameters.AddWithValue("@SettingID", userNotificationSetting.SettingID);
        command.Parameters.AddWithValue("@UserID", userNotificationSetting.UserID ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@AllowPromotions", userNotificationSetting.AllowPromotions);
        command.Parameters.AddWithValue("@AllowTripUpdates", userNotificationSetting.AllowTripUpdates);
        command.Parameters.AddWithValue("@AllowNews", userNotificationSetting.AllowNews);

        await connection.OpenAsync();
        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteUserNotificationSettingAsync(int settingId)
    {
        const string sql = "DELETE FROM UserNotificationSettings WHERE SettingID = @SettingID";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@SettingID", settingId);

        await connection.OpenAsync();
        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    private static UserNotificationSetting MapUserNotificationSettingFromReader(SqlDataReader reader)
    {
        return new UserNotificationSetting
        {
            SettingID = reader.GetInt32("SettingID"),
            UserID = reader.IsDBNull("UserID") ? null : reader.GetInt32("UserID"),
            AllowPromotions = reader.GetBoolean("AllowPromotions"),
            AllowTripUpdates = reader.GetBoolean("AllowTripUpdates"),
            AllowNews = reader.GetBoolean("AllowNews"),
            CreatedDate = reader.GetDateTime("CreatedDate")
        };
    }
}
