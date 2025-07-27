using System.Data;
using Microsoft.Data.SqlClient;
using TakiWebApi.Models;

namespace TakiWebApi.Data;

public class DriverRepository : IDriverRepository
{
    private readonly string _connectionString;

    public DriverRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("SqlServerConnection") 
            ?? throw new ArgumentNullException(nameof(configuration), "Connection string cannot be null");
    }

    public async Task<IEnumerable<Driver>> GetAllDriversAsync()
    {
        var drivers = new List<Driver>();
        const string sql = @"
            SELECT DriverID, FullName, PhoneNumber, Email, PasswordHash, VehiclePlate, VehicleModel, VehicleColor,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM Drivers
            ORDER BY CreatedDate DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            drivers.Add(MapDriverFromReader(reader));
        }

        return drivers;
    }

    public async Task<Driver?> GetDriverByIdAsync(int driverId)
    {
        const string sql = @"
            SELECT DriverID, FullName, PhoneNumber, Email, PasswordHash, VehiclePlate, VehicleModel, VehicleColor,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM Drivers
            WHERE DriverID = @DriverId";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@DriverId", driverId);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapDriverFromReader(reader);
        }

        return null;
    }

    public async Task<Driver?> GetDriverByPhoneNumberAsync(string phoneNumber)
    {
        const string sql = @"
            SELECT DriverID, FullName, PhoneNumber, Email, VehiclePlate, VehicleModel, VehicleColor,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM Drivers
            WHERE PhoneNumber = @PhoneNumber";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@PhoneNumber", phoneNumber);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapDriverFromReader(reader);
        }

        return null;
    }

    public async Task<Driver?> GetDriverByEmailAsync(string email)
    {
        const string sql = @"
            SELECT DriverID, FullName, PhoneNumber, Email, VehiclePlate, VehicleModel, VehicleColor,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM Drivers
            WHERE Email = @Email";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Email", email);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapDriverFromReader(reader);
        }

        return null;
    }

    public async Task<IEnumerable<Driver>> GetActiveDriversAsync()
    {
        var drivers = new List<Driver>();
        const string sql = @"
            SELECT DriverID, FullName, PhoneNumber, Email, PasswordHash, VehiclePlate, VehicleModel, VehicleColor,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM Drivers
            WHERE IsDeleted = 0
            ORDER BY CreatedDate DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            drivers.Add(MapDriverFromReader(reader));
        }

        return drivers;
    }

    public async Task<IEnumerable<Driver>> GetDriversPaginatedAsync(int pageNumber, int pageSize)
    {
        var drivers = new List<Driver>();
        const string sql = @"
            SELECT DriverID, FullName, PhoneNumber, Email, VehiclePlate, VehicleModel, VehicleColor,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM Drivers
            ORDER BY CreatedDate DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Offset", (pageNumber - 1) * pageSize);
        command.Parameters.AddWithValue("@PageSize", pageSize);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            drivers.Add(MapDriverFromReader(reader));
        }

        return drivers;
    }

    public async Task<int> GetTotalDriversCountAsync()
    {
        const string sql = "SELECT COUNT(*) FROM Drivers";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();

        return Convert.ToInt32(result);
    }

    public async Task<int> GetActiveDriversCountAsync()
    {
        const string sql = "SELECT COUNT(*) FROM Drivers WHERE IsDeleted = 0";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();

        return Convert.ToInt32(result);
    }

    public async Task<IEnumerable<Driver>> SearchDriversByNameAsync(string searchTerm)
    {
        var drivers = new List<Driver>();
        const string sql = @"
            SELECT DriverID, FullName, PhoneNumber, Email, VehiclePlate, VehicleModel, VehicleColor,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM Drivers
            WHERE FullName LIKE @SearchTerm
            ORDER BY FullName";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            drivers.Add(MapDriverFromReader(reader));
        }

        return drivers;
    }

    public async Task<IEnumerable<Driver>> GetDriversByCreatedDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var drivers = new List<Driver>();
        const string sql = @"
            SELECT DriverID, FullName, PhoneNumber, Email, VehiclePlate, VehicleModel, VehicleColor,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM Drivers
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
            drivers.Add(MapDriverFromReader(reader));
        }

        return drivers;
    }

    public async Task<IEnumerable<Driver>> GetDriversByVehiclePlateAsync(string vehiclePlate)
    {
        var drivers = new List<Driver>();
        const string sql = @"
            SELECT DriverID, FullName, PhoneNumber, Email, VehiclePlate, VehicleModel, VehicleColor,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM Drivers
            WHERE VehiclePlate LIKE @VehiclePlate
            ORDER BY FullName";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@VehiclePlate", $"%{vehiclePlate}%");

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            drivers.Add(MapDriverFromReader(reader));
        }

        return drivers;
    }

    private static Driver MapDriverFromReader(SqlDataReader reader)
    {
        return new Driver
        {
            DriverID = reader.GetInt32("DriverID"),
            FullName = reader.GetString("FullName"),
            PhoneNumber = reader.GetString("PhoneNumber"),
            Email = reader.IsDBNull("Email") ? null : reader.GetString("Email"),
            PasswordHash = reader.IsDBNull("PasswordHash") ? null : reader.GetString("PasswordHash"),
            VehiclePlate = reader.IsDBNull("VehiclePlate") ? null : reader.GetString("VehiclePlate"),
            VehicleModel = reader.IsDBNull("VehicleModel") ? null : reader.GetString("VehicleModel"),
            VehicleColor = reader.IsDBNull("VehicleColor") ? null : reader.GetString("VehicleColor"),
            CreatedBy = reader.IsDBNull("CreatedBy") ? null : reader.GetInt32("CreatedBy"),
            CreatedDate = reader.GetDateTime("CreatedDate"),
            UpdatedBy = reader.IsDBNull("UpdatedBy") ? null : reader.GetInt32("UpdatedBy"),
            UpdatedDate = reader.IsDBNull("UpdatedDate") ? null : reader.GetDateTime("UpdatedDate"),
            DeletedBy = reader.IsDBNull("DeletedBy") ? null : reader.GetInt32("DeletedBy"),
            DeletedDate = reader.IsDBNull("DeletedDate") ? null : reader.GetDateTime("DeletedDate"),
            IsDeleted = reader.GetBoolean("IsDeleted")
        };
    }

    public async Task<Driver> AddDriverAsync(Driver driver)
    {
        const string sql = @"
            INSERT INTO Drivers (FullName, PhoneNumber, Email, PasswordHash, VehiclePlate, VehicleModel, VehicleColor, CreatedDate, IsDeleted)
            OUTPUT INSERTED.DriverID, INSERTED.FullName, INSERTED.PhoneNumber, INSERTED.Email, INSERTED.PasswordHash,
                   INSERTED.VehiclePlate, INSERTED.VehicleModel, INSERTED.VehicleColor, INSERTED.CreatedBy, INSERTED.CreatedDate,
                   INSERTED.UpdatedBy, INSERTED.UpdatedDate, INSERTED.DeletedBy, INSERTED.DeletedDate, INSERTED.IsDeleted
            VALUES (@FullName, @PhoneNumber, @Email, @PasswordHash, @VehiclePlate, @VehicleModel, @VehicleColor, @CreatedDate, @IsDeleted)";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        
        command.Parameters.AddWithValue("@FullName", driver.FullName);
        command.Parameters.AddWithValue("@PhoneNumber", driver.PhoneNumber);
        command.Parameters.AddWithValue("@Email", (object?)driver.Email ?? DBNull.Value);
        command.Parameters.AddWithValue("@PasswordHash", (object?)driver.PasswordHash ?? DBNull.Value);
        command.Parameters.AddWithValue("@VehiclePlate", (object?)driver.VehiclePlate ?? DBNull.Value);
        command.Parameters.AddWithValue("@VehicleModel", (object?)driver.VehicleModel ?? DBNull.Value);
        command.Parameters.AddWithValue("@VehicleColor", (object?)driver.VehicleColor ?? DBNull.Value);
        command.Parameters.AddWithValue("@CreatedDate", driver.CreatedDate);
        command.Parameters.AddWithValue("@IsDeleted", driver.IsDeleted);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        
        if (await reader.ReadAsync())
        {
            return MapDriverFromReader(reader);
        }
        
        throw new InvalidOperationException("Failed to create driver");
    }

    public async Task<Driver> UpdateDriverAsync(Driver driver)
    {
        const string sql = @"
            UPDATE Drivers 
            SET FullName = @FullName, PhoneNumber = @PhoneNumber, Email = @Email, PasswordHash = @PasswordHash,
                VehiclePlate = @VehiclePlate, VehicleModel = @VehicleModel, VehicleColor = @VehicleColor,
                UpdatedDate = @UpdatedDate, IsDeleted = @IsDeleted
            OUTPUT INSERTED.DriverID, INSERTED.FullName, INSERTED.PhoneNumber, INSERTED.Email, INSERTED.PasswordHash,
                   INSERTED.VehiclePlate, INSERTED.VehicleModel, INSERTED.VehicleColor, INSERTED.CreatedBy, INSERTED.CreatedDate,
                   INSERTED.UpdatedBy, INSERTED.UpdatedDate, INSERTED.DeletedBy, INSERTED.DeletedDate, INSERTED.IsDeleted
            WHERE DriverID = @DriverID";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        
        command.Parameters.AddWithValue("@DriverID", driver.DriverID);
        command.Parameters.AddWithValue("@FullName", driver.FullName);
        command.Parameters.AddWithValue("@PhoneNumber", driver.PhoneNumber);
        command.Parameters.AddWithValue("@Email", (object?)driver.Email ?? DBNull.Value);
        command.Parameters.AddWithValue("@PasswordHash", (object?)driver.PasswordHash ?? DBNull.Value);
        command.Parameters.AddWithValue("@VehiclePlate", (object?)driver.VehiclePlate ?? DBNull.Value);
        command.Parameters.AddWithValue("@VehicleModel", (object?)driver.VehicleModel ?? DBNull.Value);
        command.Parameters.AddWithValue("@VehicleColor", (object?)driver.VehicleColor ?? DBNull.Value);
        command.Parameters.AddWithValue("@UpdatedDate", DateTime.UtcNow);
        command.Parameters.AddWithValue("@IsDeleted", driver.IsDeleted);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        
        if (await reader.ReadAsync())
        {
            return MapDriverFromReader(reader);
        }
        
        throw new InvalidOperationException("Failed to update driver");
    }

    public async Task<bool> DeleteDriverAsync(int driverId)
    {
        const string sql = @"
            UPDATE Drivers 
            SET IsDeleted = 1, DeletedDate = @DeletedDate
            WHERE DriverID = @DriverID";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        
        command.Parameters.AddWithValue("@DriverID", driverId);
        command.Parameters.AddWithValue("@DeletedDate", DateTime.UtcNow);

        await connection.OpenAsync();
        var rowsAffected = await command.ExecuteNonQueryAsync();
        
        return rowsAffected > 0;
    }
}
