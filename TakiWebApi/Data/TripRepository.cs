using System.Data;
using Microsoft.Data.SqlClient;
using TakiWebApi.Models;

namespace TakiWebApi.Data;

public class TripRepository : ITripRepository
{
    private readonly string _connectionString;

    public TripRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("SqlServerConnection") 
            ?? throw new ArgumentNullException(nameof(configuration), "Connection string cannot be null");
    }

    public async Task<IEnumerable<Trip>> GetAllTripsAsync()
    {
        var trips = new List<Trip>();
     const string sql = @"
         SELECT TripID, PassengerID, DriverID, StartAddress, EndAddress, StartLatitude, StartLongitude,
             EndLatitude, EndLongitude, StartTime, EndTime, Cost, PaymentMethod, Distance, Status,
             CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
         FROM Trips
         ORDER BY CreatedDate DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            trips.Add(MapTripFromReader(reader));
        }

        return trips;
    }

    public async Task<Trip?> GetTripByIdAsync(int tripId)
    {
     const string sql = @"
         SELECT TripID, PassengerID, DriverID, StartAddress, EndAddress, StartLatitude, StartLongitude,
             EndLatitude, EndLongitude, StartTime, EndTime, Cost, PaymentMethod, Distance, Status,
             CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
         FROM Trips
         WHERE TripID = @TripId";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@TripId", tripId);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapTripFromReader(reader);
        }

        return null;
    }

    public async Task<IEnumerable<Trip>> GetTripsByPassengerIdAsync(int passengerId)
    {
        var trips = new List<Trip>();
     const string sql = @"
         SELECT TripID, PassengerID, DriverID, StartAddress, EndAddress, StartLatitude, StartLongitude,
             EndLatitude, EndLongitude, StartTime, EndTime, Cost, PaymentMethod, Distance, Status,
             CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
         FROM Trips
         WHERE PassengerID = @PassengerId
         ORDER BY CreatedDate DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@PassengerId", passengerId);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            trips.Add(MapTripFromReader(reader));
        }

        return trips;
    }

    public async Task<IEnumerable<Trip>> GetTripsByDriverIdAsync(int driverId)
    {
        var trips = new List<Trip>();
     const string sql = @"
         SELECT TripID, PassengerID, DriverID, StartAddress, EndAddress, StartLatitude, StartLongitude,
             EndLatitude, EndLongitude, StartTime, EndTime, Cost, PaymentMethod, Distance, Status,
             CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
         FROM Trips
         WHERE DriverID = @DriverId
         ORDER BY CreatedDate DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@DriverId", driverId);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            trips.Add(MapTripFromReader(reader));
        }

        return trips;
    }

    public async Task<IEnumerable<Trip>> GetActiveTripsAsync()
    {
        var trips = new List<Trip>();
     const string sql = @"
         SELECT TripID, PassengerID, DriverID, StartAddress, EndAddress, StartLatitude, StartLongitude,
             EndLatitude, EndLongitude, StartTime, EndTime, Cost, PaymentMethod, Distance, Status,
             CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
         FROM Trips
         WHERE IsDeleted = 0
         ORDER BY CreatedDate DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            trips.Add(MapTripFromReader(reader));
        }

        return trips;
    }

    public async Task<IEnumerable<Trip>> GetTripsPaginatedAsync(int pageNumber, int pageSize)
    {
        var trips = new List<Trip>();
     const string sql = @"
         SELECT TripID, PassengerID, DriverID, StartAddress, EndAddress, StartLatitude, StartLongitude,
             EndLatitude, EndLongitude, StartTime, EndTime, Cost, PaymentMethod, Distance, Status,
             CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
         FROM Trips
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
            trips.Add(MapTripFromReader(reader));
        }

        return trips;
    }

    public async Task<int> GetTotalTripsCountAsync()
    {
        const string sql = "SELECT COUNT(*) FROM Trips";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();

        return Convert.ToInt32(result);
    }

    public async Task<int> GetActiveTripsCountAsync()
    {
        const string sql = "SELECT COUNT(*) FROM Trips WHERE IsDeleted = 0";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();

        return Convert.ToInt32(result);
    }

    public async Task<IEnumerable<Trip>> GetTripsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var trips = new List<Trip>();
        const string sql = @"
            SELECT TripID, PassengerID, DriverID, StartAddress, EndAddress, StartLatitude, StartLongitude,
                   EndLatitude, EndLongitude, StartTime, EndTime, Cost, PaymentMethod,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM Trips
            WHERE StartTime >= @StartDate AND StartTime <= @EndDate
            ORDER BY StartTime DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@StartDate", startDate);
        command.Parameters.AddWithValue("@EndDate", endDate);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            trips.Add(MapTripFromReader(reader));
        }

        return trips;
    }

    public async Task<IEnumerable<Trip>> GetTripsByPaymentMethodAsync(string paymentMethod)
    {
        var trips = new List<Trip>();
        const string sql = @"
            SELECT TripID, PassengerID, DriverID, StartAddress, EndAddress, StartLatitude, StartLongitude,
                   EndLatitude, EndLongitude, StartTime, EndTime, Cost, PaymentMethod,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM Trips
            WHERE PaymentMethod = @PaymentMethod
            ORDER BY CreatedDate DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@PaymentMethod", paymentMethod);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            trips.Add(MapTripFromReader(reader));
        }

        return trips;
    }

    public async Task<decimal> GetTotalTripCostByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        const string sql = @"
            SELECT ISNULL(SUM(Cost), 0) 
            FROM Trips 
            WHERE StartTime >= @StartDate AND StartTime <= @EndDate AND IsDeleted = 0";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@StartDate", startDate);
        command.Parameters.AddWithValue("@EndDate", endDate);

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();

        return Convert.ToDecimal(result);
    }

    public async Task<decimal> GetTotalTripCostByPassengerIdAsync(int passengerId)
    {
        const string sql = @"
            SELECT SUM(Cost)
            FROM Trips
            WHERE PassengerID = @PassengerId AND IsDeleted = 0";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@PassengerId", passengerId);

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();
        return result != DBNull.Value ? Convert.ToDecimal(result) : 0;
    }

    private static Trip MapTripFromReader(SqlDataReader reader)
    {
        return new Trip
        {
            TripID = reader.GetInt32("TripID"),
            PassengerID = reader.IsDBNull("PassengerID") ? null : reader.GetInt32("PassengerID"),
            DriverID = reader.IsDBNull("DriverID") ? null : reader.GetInt32("DriverID"),
            StartAddress = reader.IsDBNull("StartAddress") ? null : reader.GetString("StartAddress"),
            EndAddress = reader.IsDBNull("EndAddress") ? null : reader.GetString("EndAddress"),
            StartLatitude = reader.IsDBNull("StartLatitude") ? null : reader.GetDouble("StartLatitude"),
            StartLongitude = reader.IsDBNull("StartLongitude") ? null : reader.GetDouble("StartLongitude"),
            EndLatitude = reader.IsDBNull("EndLatitude") ? null : reader.GetDouble("EndLatitude"),
            EndLongitude = reader.IsDBNull("EndLongitude") ? null : reader.GetDouble("EndLongitude"),
            StartTime = reader.IsDBNull("StartTime") ? null : reader.GetDateTime("StartTime"),
            EndTime = reader.IsDBNull("EndTime") ? null : reader.GetDateTime("EndTime"),
            Cost = reader.IsDBNull("Cost") ? null : reader.GetDecimal("Cost"),
            PaymentMethod = reader.IsDBNull("PaymentMethod") ? null : reader.GetString("PaymentMethod"),
            Distance = reader.IsDBNull("Distance") ? null : reader.GetDouble("Distance"),
            Status = reader.IsDBNull("Status") ? null : Enum.TryParse<RideStatus>(reader.GetString("Status"), out var status) ? status : null,
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
