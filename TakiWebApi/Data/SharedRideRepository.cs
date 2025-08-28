using System.Data.SqlClient;
using TakiWebApi.Models;

namespace TakiWebApi.Data;

public class SharedRideRepository : ISharedRideRepository
{
    private readonly string _connectionString;
    public SharedRideRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection");
    }

    public async Task<IEnumerable<SharedRide>> GetAllAsync()
    {
        var list = new List<SharedRide>();
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("SELECT * FROM SharedRides", connection);
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new SharedRide
            {
                SharedRideID = reader.GetInt32(reader.GetOrdinal("SharedRideID")),
                TripID = reader.GetInt32(reader.GetOrdinal("TripID")),
                PassengerCount = reader.GetInt32(reader.GetOrdinal("PassengerCount")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate"))
            });
        }
        return list;
    }

    public async Task<SharedRide?> GetByIdAsync(int sharedRideId)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("SELECT * FROM SharedRides WHERE SharedRideID = @SharedRideID", connection);
        command.Parameters.AddWithValue("@SharedRideID", sharedRideId);
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new SharedRide
            {
                SharedRideID = reader.GetInt32(reader.GetOrdinal("SharedRideID")),
                TripID = reader.GetInt32(reader.GetOrdinal("TripID")),
                PassengerCount = reader.GetInt32(reader.GetOrdinal("PassengerCount")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate"))
            };
        }
        return null;
    }

    public async Task<IEnumerable<SharedRide>> GetByTripIdAsync(int tripId)
    {
        var list = new List<SharedRide>();
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("SELECT * FROM SharedRides WHERE TripID = @TripID", connection);
        command.Parameters.AddWithValue("@TripID", tripId);
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new SharedRide
            {
                SharedRideID = reader.GetInt32(reader.GetOrdinal("SharedRideID")),
                TripID = reader.GetInt32(reader.GetOrdinal("TripID")),
                PassengerCount = reader.GetInt32(reader.GetOrdinal("PassengerCount")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate"))
            });
        }
        return list;
    }

    public async Task<int> CreateAsync(SharedRide sharedRide)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(
            @"INSERT INTO SharedRides (TripID, PassengerCount, Status, CreatedDate)
              VALUES (@TripID, @PassengerCount, @Status, @CreatedDate);
              SELECT SCOPE_IDENTITY();", connection);
        command.Parameters.AddWithValue("@TripID", sharedRide.TripID);
        command.Parameters.AddWithValue("@PassengerCount", sharedRide.PassengerCount);
        command.Parameters.AddWithValue("@Status", sharedRide.Status);
        command.Parameters.AddWithValue("@CreatedDate", sharedRide.CreatedDate);
        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<bool> UpdateAsync(SharedRide sharedRide)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(
            @"UPDATE SharedRides SET
                TripID = @TripID,
                PassengerCount = @PassengerCount,
                Status = @Status
              WHERE SharedRideID = @SharedRideID", connection);
        command.Parameters.AddWithValue("@SharedRideID", sharedRide.SharedRideID);
        command.Parameters.AddWithValue("@TripID", sharedRide.TripID);
        command.Parameters.AddWithValue("@PassengerCount", sharedRide.PassengerCount);
        command.Parameters.AddWithValue("@Status", sharedRide.Status);
        await connection.OpenAsync();
        var affected = await command.ExecuteNonQueryAsync();
        return affected > 0;
    }

    public async Task<bool> DeleteAsync(int sharedRideId)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("DELETE FROM SharedRides WHERE SharedRideID = @SharedRideID", connection);
        command.Parameters.AddWithValue("@SharedRideID", sharedRideId);
        await connection.OpenAsync();
        var affected = await command.ExecuteNonQueryAsync();
        return affected > 0;
    }
}
