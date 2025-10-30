using System.Data;
using Microsoft.Data.SqlClient;
using TakiWebApi.Models;

namespace TakiWebApi.Data;

public class UserRepository : IUserRepository
{
    private readonly string _connectionString;

    public UserRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("SqlServerConnection") 
            ?? throw new ArgumentNullException(nameof(configuration), "Connection string cannot be null");
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        var users = new List<User>();
        const string sql = @"
            SELECT UserID, FullName, PhoneNumber, Email, IsActive, CreatedBy, CreatedDate, 
                   UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM Users
            ORDER BY CreatedDate DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            users.Add(MapUserFromReader(reader));
        }

        return users;
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        const string sql = @"
            SELECT UserID, FullName, PhoneNumber, Email, PasswordHash, IsActive, CreatedBy, CreatedDate, 
                   UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM Users
            WHERE UserID = @UserId";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@UserId", userId);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapUserFromReader(reader);
        }

        return null;
    }

    public async Task<User?> GetUserByPhoneNumberAsync(string phoneNumber)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        string query = "SELECT UserID, FullName, PhoneNumber, Email, IsActive, CreatedDate, IsDeleted, PasswordHash FROM Users WHERE PhoneNumber = @PhoneNumber";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@PhoneNumber", phoneNumber);

        using var reader = await command.ExecuteReaderAsync();
        
        if (await reader.ReadAsync())
        {
            return MapUserFromReader(reader);
        }

        return null;
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        const string sql = @"
            SELECT UserID, FullName, PhoneNumber, Email, IsActive, CreatedBy, CreatedDate, 
                   UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM Users
            WHERE Email = @Email";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Email", email);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapUserFromReader(reader);
        }

        return null;
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        var users = new List<User>();
        const string sql = @"
            SELECT UserID, FullName, PhoneNumber, Email, IsActive, CreatedBy, CreatedDate, 
                   UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM Users
            WHERE IsActive = 1 AND IsDeleted = 0
            ORDER BY CreatedDate DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            users.Add(MapUserFromReader(reader));
        }

        return users;
    }

    public async Task<IEnumerable<User>> GetUsersPaginatedAsync(int pageNumber, int pageSize)
    {
        var users = new List<User>();
        const string sql = @"
            SELECT UserID, FullName, PhoneNumber, Email, IsActive, CreatedBy, CreatedDate, 
                   UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM Users
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
            users.Add(MapUserFromReader(reader));
        }

        return users;
    }

    public async Task<int> GetTotalUsersCountAsync()
    {
        const string sql = "SELECT COUNT(*) FROM Users";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        await connection.OpenAsync();
        var count = await command.ExecuteScalarAsync();

        return Convert.ToInt32(count);
    }

    public async Task<int> GetActiveUsersCountAsync()
    {
        const string sql = "SELECT COUNT(*) FROM Users WHERE IsActive = 1 AND IsDeleted = 0";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        await connection.OpenAsync();
        var count = await command.ExecuteScalarAsync();

        return Convert.ToInt32(count);
    }

    public async Task<IEnumerable<User>> SearchUsersByNameAsync(string searchTerm)
    {
        var users = new List<User>();
        const string sql = @"
            SELECT UserID, FullName, PhoneNumber, Email, IsActive, CreatedBy, CreatedDate, 
                   UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM Users
            WHERE FullName LIKE @SearchTerm
            ORDER BY FullName";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            users.Add(MapUserFromReader(reader));
        }

        return users;
    }

    public async Task<IEnumerable<User>> GetUsersByCreatedDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var users = new List<User>();
        const string sql = @"
            SELECT UserID, FullName, PhoneNumber, Email, IsActive, CreatedBy, CreatedDate, 
                   UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM Users
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
            users.Add(MapUserFromReader(reader));
        }

        return users;
    }

    private static User MapUserFromReader(SqlDataReader reader)
    {
        return new User
        {
            UserID = reader.GetInt32("UserID"),
            FullName = reader.GetString("FullName"),
            PhoneNumber = reader.GetString("PhoneNumber"),
            Email = reader.IsDBNull("Email") ? null : reader.GetString("Email"),
            IsActive = reader.GetBoolean("IsActive"),
            CreatedDate = reader.GetDateTime("CreatedDate"),
            IsDeleted = reader.GetBoolean("IsDeleted"),
            PasswordHash = HasColumn(reader, "PasswordHash") && !reader.IsDBNull(reader.GetOrdinal("PasswordHash"))
                ? reader.GetString(reader.GetOrdinal("PasswordHash"))
                : null
        };
    }

    private static bool HasColumn(SqlDataReader reader, string columnName)
    {
        for (int i = 0; i < reader.FieldCount; i++)
        {
            if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    public async Task<int> CreateUserAsync(User user)
    {
        const string sql = @"
            INSERT INTO Users (FullName, PhoneNumber, Email, IsActive, CreatedDate, IsDeleted)
            VALUES (@FullName, @PhoneNumber, @Email, @IsActive, @CreatedDate, @IsDeleted);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        
        command.Parameters.AddWithValue("@FullName", user.FullName ?? string.Empty);
        command.Parameters.AddWithValue("@PhoneNumber", user.PhoneNumber ?? string.Empty);
        command.Parameters.AddWithValue("@Email", user.Email ?? string.Empty);
        command.Parameters.AddWithValue("@IsActive", user.IsActive);
        command.Parameters.AddWithValue("@CreatedDate", user.CreatedDate);
        command.Parameters.AddWithValue("@IsDeleted", user.IsDeleted);

        await connection.OpenAsync();
        var newUserId = await command.ExecuteScalarAsync();
        return Convert.ToInt32(newUserId);
    }

    public async Task<bool> UpdateUserAsync(User user)
    {
        const string sql = @"
            UPDATE Users 
            SET FullName = @FullName, 
                PhoneNumber = @PhoneNumber, 
                Email = @Email, 
                IsActive = @IsActive,
                UpdatedDate = @UpdatedDate
            WHERE UserID = @UserID";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        
        command.Parameters.AddWithValue("@UserID", user.UserID);
        command.Parameters.AddWithValue("@FullName", user.FullName ?? string.Empty);
        command.Parameters.AddWithValue("@PhoneNumber", user.PhoneNumber ?? string.Empty);
        command.Parameters.AddWithValue("@Email", user.Email ?? string.Empty);
        command.Parameters.AddWithValue("@IsActive", user.IsActive);
        command.Parameters.AddWithValue("@UpdatedDate", DateTime.UtcNow);

        await connection.OpenAsync();
        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteUserAsync(int userId)
    {
        const string sql = @"
            UPDATE Users 
            SET IsDeleted = 1, 
                DeletedDate = @DeletedDate
            WHERE UserID = @UserID";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        
        command.Parameters.AddWithValue("@UserID", userId);
        command.Parameters.AddWithValue("@DeletedDate", DateTime.UtcNow);

        await connection.OpenAsync();
        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> UpdateUserPasswordAsync(int userId, string newPasswordHash)
    {
        const string sql = @"
            UPDATE Users 
            SET PasswordHash = @PasswordHash, UpdatedDate = @UpdatedDate 
            WHERE UserID = @UserID AND IsDeleted = 0";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        
        command.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
        command.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 500).Value = newPasswordHash;
        command.Parameters.Add("@UpdatedDate", SqlDbType.DateTime).Value = DateTime.UtcNow;

        await connection.OpenAsync();
        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }
}
