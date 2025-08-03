using System.Data;
using Microsoft.Data.SqlClient;
using TakiWebApi.Models;

namespace TakiWebApi.Data;

public class UserAddressRepository : IUserAddressRepository
{
    private readonly string _connectionString;

    public UserAddressRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("SqlServerConnection") 
            ?? throw new ArgumentNullException(nameof(configuration), "Connection string cannot be null");
    }

    public async Task<IEnumerable<UserAddress>> GetAllUserAddressesAsync()
    {
        var userAddresses = new List<UserAddress>();
        const string sql = @"
            SELECT AddressID, UserID, Title, AddressText, Latitude, Longitude,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM UserAddresses
            ORDER BY CreatedDate DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            userAddresses.Add(MapUserAddressFromReader(reader));
        }

        return userAddresses;
    }

    public async Task<UserAddress?> GetUserAddressByIdAsync(int addressId)
    {
        const string sql = @"
            SELECT AddressID, UserID, Title, AddressText, Latitude, Longitude,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM UserAddresses
            WHERE AddressID = @AddressId";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@AddressId", addressId);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapUserAddressFromReader(reader);
        }

        return null;
    }

    public async Task<IEnumerable<UserAddress>> GetUserAddressesByUserIdAsync(int userId)
    {
        var userAddresses = new List<UserAddress>();
        const string sql = @"
            SELECT AddressID, UserID, Title, AddressText, Latitude, Longitude,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM UserAddresses
            WHERE UserID = @UserId AND IsDeleted = 0
            ORDER BY CreatedDate DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@UserId", userId);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            userAddresses.Add(MapUserAddressFromReader(reader));
        }

        return userAddresses;
    }

    public async Task<IEnumerable<UserAddress>> GetActiveUserAddressesAsync()
    {
        var userAddresses = new List<UserAddress>();
        const string sql = @"
            SELECT AddressID, UserID, Title, AddressText, Latitude, Longitude,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM UserAddresses
            WHERE IsDeleted = 0
            ORDER BY CreatedDate DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            userAddresses.Add(MapUserAddressFromReader(reader));
        }

        return userAddresses;
    }

    public async Task<IEnumerable<UserAddress>> GetUserAddressesPaginatedAsync(int pageNumber, int pageSize)
    {
        var userAddresses = new List<UserAddress>();
        const string sql = @"
            SELECT AddressID, UserID, Title, AddressText, Latitude, Longitude,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM UserAddresses
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
            userAddresses.Add(MapUserAddressFromReader(reader));
        }

        return userAddresses;
    }

    public async Task<int> GetTotalUserAddressesCountAsync()
    {
        const string sql = "SELECT COUNT(*) FROM UserAddresses";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<int> GetActiveUserAddressesCountAsync()
    {
        const string sql = "SELECT COUNT(*) FROM UserAddresses WHERE IsDeleted = 0";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<IEnumerable<UserAddress>> SearchUserAddressesByTitleAsync(string searchTerm)
    {
        var userAddresses = new List<UserAddress>();
        const string sql = @"
            SELECT AddressID, UserID, Title, AddressText, Latitude, Longitude,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM UserAddresses
            WHERE Title LIKE @SearchTerm OR AddressText LIKE @SearchTerm
            ORDER BY CreatedDate DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            userAddresses.Add(MapUserAddressFromReader(reader));
        }

        return userAddresses;
    }

    public async Task<IEnumerable<UserAddress>> GetUserAddressesByCreatedDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var userAddresses = new List<UserAddress>();
        const string sql = @"
            SELECT AddressID, UserID, Title, AddressText, Latitude, Longitude,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM UserAddresses
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
            userAddresses.Add(MapUserAddressFromReader(reader));
        }

        return userAddresses;
    }

    public async Task<int> CreateUserAddressAsync(UserAddress userAddress)
    {
        const string sql = @"
            INSERT INTO UserAddresses (UserID, Title, AddressText, Latitude, Longitude, CreatedBy, CreatedDate, IsDeleted)
            VALUES (@UserID, @Title, @AddressText, @Latitude, @Longitude, @CreatedBy, @CreatedDate, @IsDeleted);
            SELECT SCOPE_IDENTITY();";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        
        command.Parameters.AddWithValue("@UserID", userAddress.UserID ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Title", userAddress.Title ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@AddressText", userAddress.AddressText ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Latitude", userAddress.Latitude ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Longitude", userAddress.Longitude ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@CreatedBy", userAddress.CreatedBy ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@CreatedDate", userAddress.CreatedDate);
        command.Parameters.AddWithValue("@IsDeleted", userAddress.IsDeleted);

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<bool> UpdateUserAddressAsync(UserAddress userAddress)
    {
        const string sql = @"
            UPDATE UserAddresses 
            SET UserID = @UserID, Title = @Title, AddressText = @AddressText, 
                Latitude = @Latitude, Longitude = @Longitude, UpdatedBy = @UpdatedBy, UpdatedDate = @UpdatedDate
            WHERE AddressID = @AddressID";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        
        command.Parameters.AddWithValue("@AddressID", userAddress.AddressID);
        command.Parameters.AddWithValue("@UserID", userAddress.UserID ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Title", userAddress.Title ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@AddressText", userAddress.AddressText ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Latitude", userAddress.Latitude ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Longitude", userAddress.Longitude ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@UpdatedBy", userAddress.UpdatedBy ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@UpdatedDate", userAddress.UpdatedDate ?? (object)DBNull.Value);

        await connection.OpenAsync();
        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteUserAddressAsync(int addressId)
    {
        const string sql = @"
            UPDATE UserAddresses 
            SET IsDeleted = 1, DeletedDate = @DeletedDate 
            WHERE AddressID = @AddressID";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@AddressID", addressId);
        command.Parameters.AddWithValue("@DeletedDate", DateTime.UtcNow);

        await connection.OpenAsync();
        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    private static UserAddress MapUserAddressFromReader(SqlDataReader reader)
    {
        return new UserAddress
        {
            AddressID = reader.GetInt32("AddressID"),
            UserID = reader.IsDBNull("UserID") ? null : reader.GetInt32("UserID"),
            Title = reader.IsDBNull("Title") ? null : reader.GetString("Title"),
            AddressText = reader.IsDBNull("AddressText") ? null : reader.GetString("AddressText"),
            Latitude = reader.IsDBNull("Latitude") ? null : reader.GetDouble("Latitude"),
            Longitude = reader.IsDBNull("Longitude") ? null : reader.GetDouble("Longitude"),
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
