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
            SELECT UserID, FullName, PhoneNumber, Email, IsActive, CreatedBy, CreatedDate, 
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
        const string sql = @"
            SELECT UserID, FullName, PhoneNumber, Email, IsActive, CreatedBy, CreatedDate, 
                   UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM Users
            WHERE PhoneNumber = @PhoneNumber";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@PhoneNumber", phoneNumber);

        await connection.OpenAsync();
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
