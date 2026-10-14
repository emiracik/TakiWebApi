using System.Data;
using Microsoft.Data.SqlClient;
using TakiWebApi.Models;

namespace TakiWebApi.Data;

public interface IScheduledRideRepository
{
    Task<int> CreateAsync(ScheduledRide ride);
    Task<ScheduledRide?> GetByIdAsync(int id);

    /// <summary>Yolcunun iptal edilmemiş, saati geçmemiş (1 saat tolerans) planları.</summary>
    Task<IEnumerable<ScheduledRide>> GetUpcomingByUserAsync(int userId);

    Task<bool> CancelAsync(int id);

    /// <summary>Saati <paramref name="within"/> içinde gelen ve henüz hatırlatılmamış planlar.</summary>
    Task<IEnumerable<ScheduledRide>> GetDueForReminderAsync(TimeSpan within);

    Task MarkRemindedAsync(int id);
}

public class ScheduledRideRepository : IScheduledRideRepository
{
    private readonly string _connectionString;

    public ScheduledRideRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("SqlServerConnection")
            ?? throw new ArgumentNullException(nameof(configuration));
    }

    private const string Columns = @"ScheduledRideID, UserID, PickupAddress, PickupLatitude, PickupLongitude,
        DropoffAddress, DropoffLatitude, DropoffLongitude, ScheduledTime, VehicleType, PaymentMethod,
        EstimatedCost, Status, CreatedDate";

    public async Task<int> CreateAsync(ScheduledRide ride)
    {
        const string sql = @"
            INSERT INTO ScheduledRides (UserID, PickupAddress, PickupLatitude, PickupLongitude,
                DropoffAddress, DropoffLatitude, DropoffLongitude, ScheduledTime, VehicleType, PaymentMethod, EstimatedCost)
            VALUES (@UserID, @PickupAddress, @PickupLatitude, @PickupLongitude,
                @DropoffAddress, @DropoffLatitude, @DropoffLongitude, @ScheduledTime, @VehicleType, @PaymentMethod, @EstimatedCost);
            SELECT CAST(SCOPE_IDENTITY() AS int);";
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@UserID", ride.UserID);
        command.Parameters.AddWithValue("@PickupAddress", ride.PickupAddress);
        command.Parameters.AddWithValue("@PickupLatitude", ride.PickupLatitude);
        command.Parameters.AddWithValue("@PickupLongitude", ride.PickupLongitude);
        command.Parameters.AddWithValue("@DropoffAddress", ride.DropoffAddress);
        command.Parameters.AddWithValue("@DropoffLatitude", ride.DropoffLatitude);
        command.Parameters.AddWithValue("@DropoffLongitude", ride.DropoffLongitude);
        command.Parameters.AddWithValue("@ScheduledTime", ride.ScheduledTime);
        command.Parameters.AddWithValue("@VehicleType", (object?)ride.VehicleType ?? DBNull.Value);
        command.Parameters.AddWithValue("@PaymentMethod", (object?)ride.PaymentMethod ?? DBNull.Value);
        command.Parameters.AddWithValue("@EstimatedCost", (object?)ride.EstimatedCost ?? DBNull.Value);
        await connection.OpenAsync();
        return (int)(await command.ExecuteScalarAsync())!;
    }

    public async Task<ScheduledRide?> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand($"SELECT {Columns} FROM ScheduledRides WHERE ScheduledRideID = @Id", connection);
        command.Parameters.AddWithValue("@Id", id);
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? Map(reader) : null;
    }

    public async Task<IEnumerable<ScheduledRide>> GetUpcomingByUserAsync(int userId)
    {
        const string where = @"UserID = @UserId AND Status <> 'Cancelled'
            AND ScheduledTime > DATEADD(hour, -1, SYSUTCDATETIME())";
        var list = new List<ScheduledRide>();
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(
            $"SELECT {Columns} FROM ScheduledRides WHERE {where} ORDER BY ScheduledTime", connection);
        command.Parameters.AddWithValue("@UserId", userId);
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) list.Add(Map(reader));
        return list;
    }

    public async Task<bool> CancelAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(
            "UPDATE ScheduledRides SET Status = 'Cancelled' WHERE ScheduledRideID = @Id AND Status <> 'Cancelled'", connection);
        command.Parameters.AddWithValue("@Id", id);
        await connection.OpenAsync();
        return await command.ExecuteNonQueryAsync() > 0;
    }

    public async Task<IEnumerable<ScheduledRide>> GetDueForReminderAsync(TimeSpan within)
    {
        var list = new List<ScheduledRide>();
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(
            $@"SELECT {Columns} FROM ScheduledRides
               WHERE Status = 'Scheduled' AND ScheduledTime <= @Until
                 AND ScheduledTime > DATEADD(hour, -1, SYSUTCDATETIME())", connection);
        command.Parameters.AddWithValue("@Until", DateTime.UtcNow.Add(within));
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) list.Add(Map(reader));
        return list;
    }

    public async Task MarkRemindedAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(
            "UPDATE ScheduledRides SET Status = 'Reminded' WHERE ScheduledRideID = @Id AND Status = 'Scheduled'", connection);
        command.Parameters.AddWithValue("@Id", id);
        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    private static ScheduledRide Map(SqlDataReader r) => new()
    {
        ScheduledRideID = r.GetInt32("ScheduledRideID"),
        UserID = r.GetInt32("UserID"),
        PickupAddress = r.GetString("PickupAddress"),
        PickupLatitude = r.GetDouble("PickupLatitude"),
        PickupLongitude = r.GetDouble("PickupLongitude"),
        DropoffAddress = r.GetString("DropoffAddress"),
        DropoffLatitude = r.GetDouble("DropoffLatitude"),
        DropoffLongitude = r.GetDouble("DropoffLongitude"),
        ScheduledTime = DateTime.SpecifyKind(r.GetDateTime("ScheduledTime"), DateTimeKind.Utc),
        VehicleType = r.IsDBNull("VehicleType") ? null : r.GetString("VehicleType"),
        PaymentMethod = r.IsDBNull("PaymentMethod") ? null : r.GetString("PaymentMethod"),
        EstimatedCost = r.IsDBNull("EstimatedCost") ? null : r.GetDecimal("EstimatedCost"),
        Status = r.GetString("Status"),
        CreatedDate = DateTime.SpecifyKind(r.GetDateTime("CreatedDate"), DateTimeKind.Utc),
    };
}
