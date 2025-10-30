using Microsoft.Data.SqlClient;
using TakiWebApi.Models;
using System.Data;

namespace TakiWebApi.Data;

public class MatchingRepository : IMatchingRepository
{
    private readonly string _connectionString;
    private readonly IDriverRatingRepository _driverRatingRepository;

    public MatchingRepository(IConfiguration configuration, IDriverRatingRepository driverRatingRepository)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        _driverRatingRepository = driverRatingRepository;
    }

    #region Matching Request Operations

    public async Task<MatchingRequest> CreateMatchingRequestAsync(MatchingRequest request)
    {
        const string sql = @"
            INSERT INTO MatchingRequests 
            (PassengerID, PickupAddress, DropoffAddress, PickupLatitude, PickupLongitude, 
             DropoffLatitude, DropoffLongitude, EstimatedCost, EstimatedDistance, EstimatedDuration,
             Status, RequestTime, ExpiryTime, Notes, MaxWaitTime, MinRating, MaxDistance, CreatedBy, CreatedDate)
            VALUES 
            (@PassengerID, @PickupAddress, @DropoffAddress, @PickupLatitude, @PickupLongitude,
             @DropoffLatitude, @DropoffLongitude, @EstimatedCost, @EstimatedDistance, @EstimatedDuration,
             @Status, @RequestTime, @ExpiryTime, @Notes, @MaxWaitTime, @MinRating, @MaxDistance, @CreatedBy, @CreatedDate);
            SELECT SCOPE_IDENTITY();";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        command.Parameters.AddWithValue("@PassengerID", request.PassengerID);
        command.Parameters.AddWithValue("@PickupAddress", request.PickupAddress);
        command.Parameters.AddWithValue("@DropoffAddress", request.DropoffAddress);
        command.Parameters.AddWithValue("@PickupLatitude", request.PickupLatitude);
        command.Parameters.AddWithValue("@PickupLongitude", request.PickupLongitude);
        command.Parameters.AddWithValue("@DropoffLatitude", request.DropoffLatitude);
        command.Parameters.AddWithValue("@DropoffLongitude", request.DropoffLongitude);
        command.Parameters.AddWithValue("@EstimatedCost", (object?)request.EstimatedCost ?? DBNull.Value);
        command.Parameters.AddWithValue("@EstimatedDistance", (object?)request.EstimatedDistance ?? DBNull.Value);
        command.Parameters.AddWithValue("@EstimatedDuration", (object?)request.EstimatedDuration ?? DBNull.Value);
        command.Parameters.AddWithValue("@Status", (int)request.Status);
        command.Parameters.AddWithValue("@RequestTime", request.RequestTime);
        command.Parameters.AddWithValue("@ExpiryTime", (object?)request.ExpiryTime ?? DBNull.Value);
        command.Parameters.AddWithValue("@Notes", (object?)request.Notes ?? DBNull.Value);
        command.Parameters.AddWithValue("@MaxWaitTime", (object?)request.MaxWaitTime ?? DBNull.Value);
        command.Parameters.AddWithValue("@MinRating", (object?)request.MinRating ?? DBNull.Value);
        command.Parameters.AddWithValue("@MaxDistance", (object?)request.MaxDistance ?? DBNull.Value);
        command.Parameters.AddWithValue("@CreatedBy", (object?)request.CreatedBy ?? DBNull.Value);
        command.Parameters.AddWithValue("@CreatedDate", request.CreatedDate);

        await connection.OpenAsync();
        var id = Convert.ToInt32(await command.ExecuteScalarAsync());
        request.MatchingRequestID = id;
        
        return request;
    }

    public async Task<MatchingRequest?> GetMatchingRequestByIdAsync(int requestId)
    {
        const string sql = @"
            SELECT MatchingRequestID, PassengerID, DriverID, PickupAddress, DropoffAddress,
                   PickupLatitude, PickupLongitude, DropoffLatitude, DropoffLongitude,
                   EstimatedCost, EstimatedDistance, EstimatedDuration, Status, RequestTime,
                   AcceptedTime, ExpiryTime, Notes, MaxWaitTime, MinRating, MaxDistance,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM MatchingRequests 
            WHERE MatchingRequestID = @RequestId AND IsDeleted = 0";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@RequestId", requestId);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapMatchingRequestFromReader(reader);
        }

        return null;
    }

    public async Task<IEnumerable<MatchingRequest>> GetPendingMatchingRequestsAsync()
    {
        var requests = new List<MatchingRequest>();
        const string sql = @"
            SELECT MatchingRequestID, PassengerID, DriverID, PickupAddress, DropoffAddress,
                   PickupLatitude, PickupLongitude, DropoffLatitude, DropoffLongitude,
                   EstimatedCost, EstimatedDistance, EstimatedDuration, Status, RequestTime,
                   AcceptedTime, ExpiryTime, Notes, MaxWaitTime, MinRating, MaxDistance,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM MatchingRequests 
            WHERE Status = @Status AND IsDeleted = 0 
            ORDER BY RequestTime ASC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Status", (int)MatchingStatus.Pending);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            requests.Add(MapMatchingRequestFromReader(reader));
        }

        return requests;
    }

    #endregion

    #region Driver Location Operations

    public async Task UpdateDriverLocationAsync(int driverId, UpdateDriverLocationRequest locationRequest)
    {
        const string sql = @"
            MERGE DriverLocations AS target
            USING (SELECT @DriverID as DriverID) AS source
            ON target.DriverID = source.DriverID
            WHEN MATCHED THEN
                UPDATE SET 
                    Latitude = @Latitude,
                    Longitude = @Longitude,
                    IsAvailable = @IsAvailable,
                    CurrentAddress = @CurrentAddress,
                    LocationTime = @LocationTime,
                    Speed = @Speed,
                    Heading = @Heading,
                    IsOnline = 1,
                    UpdatedDate = @UpdatedDate
            WHEN NOT MATCHED THEN
                INSERT (DriverID, Latitude, Longitude, IsAvailable, CurrentAddress, 
                        LocationTime, Speed, Heading, IsOnline, CreatedDate)
                VALUES (@DriverID, @Latitude, @Longitude, @IsAvailable, @CurrentAddress,
                        @LocationTime, @Speed, @Heading, 1, @CreatedDate);";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        command.Parameters.AddWithValue("@DriverID", driverId);
        command.Parameters.AddWithValue("@Latitude", locationRequest.Latitude);
        command.Parameters.AddWithValue("@Longitude", locationRequest.Longitude);
        command.Parameters.AddWithValue("@IsAvailable", locationRequest.IsAvailable);
        command.Parameters.AddWithValue("@CurrentAddress", (object?)locationRequest.CurrentAddress ?? DBNull.Value);
        command.Parameters.AddWithValue("@LocationTime", DateTime.UtcNow);
        command.Parameters.AddWithValue("@Speed", (object?)locationRequest.Speed ?? DBNull.Value);
        command.Parameters.AddWithValue("@Heading", (object?)locationRequest.Heading ?? DBNull.Value);
        command.Parameters.AddWithValue("@CreatedDate", DateTime.UtcNow);
        command.Parameters.AddWithValue("@UpdatedDate", DateTime.UtcNow);

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    #endregion

    #region Matching Algorithm Operations

    public async Task<IEnumerable<NearbyDriverResponse>> FindNearbyDriversAsync(
        double latitude, double longitude, double radiusKm, int maxResults, double? minRating = null)
    {
        var nearbyDrivers = new List<NearbyDriverResponse>();
        
        const string sql = @"
            SELECT TOP (@MaxResults)
                d.DriverID, d.FullName, d.PhoneNumber, d.VehiclePlate, d.VehicleModel, d.VehicleColor,
                dl.Latitude, dl.Longitude, dl.IsAvailable,
                COALESCE(AVG(CAST(dr.Rating as FLOAT)), 0) as AverageRating,
                COUNT(dr.RatingID) as TotalRatings
            FROM Drivers d
            INNER JOIN DriverLocations dl ON d.DriverID = dl.DriverID
            LEFT JOIN DriverRatings dr ON d.DriverID = dr.DriverID AND dr.IsDeleted = 0
            WHERE d.IsDeleted = 0 
                AND dl.IsAvailable = 1 
                AND dl.IsOnline = 1
                AND dl.LocationTime > DATEADD(MINUTE, -30, GETUTCDATE())
            GROUP BY d.DriverID, d.FullName, d.PhoneNumber, d.VehiclePlate, d.VehicleModel, 
                     d.VehicleColor, dl.Latitude, dl.Longitude, dl.IsAvailable
            HAVING (@MinRating IS NULL OR COALESCE(AVG(CAST(dr.Rating as FLOAT)), 0) >= @MinRating)
            ORDER BY AverageRating DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@MaxResults", maxResults);
        command.Parameters.AddWithValue("@MinRating", (object?)minRating ?? DBNull.Value);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var driverLat = reader.GetDouble("Latitude");
            var driverLng = reader.GetDouble("Longitude");
            var distance = await CalculateDistanceAsync(latitude, longitude, driverLat, driverLng);

            if (distance <= radiusKm)
            {
                var estimatedArrival = await EstimateArrivalTimeAsync(distance);
                
                nearbyDrivers.Add(new NearbyDriverResponse
                {
                    DriverID = reader.GetInt32("DriverID"),
                    FullName = reader.GetString("FullName"),
                    PhoneNumber = reader.GetString("PhoneNumber"),
                    VehiclePlate = reader.IsDBNull("VehiclePlate") ? null : reader.GetString("VehiclePlate"),
                    VehicleModel = reader.IsDBNull("VehicleModel") ? null : reader.GetString("VehicleModel"),
                    VehicleColor = reader.IsDBNull("VehicleColor") ? null : reader.GetString("VehicleColor"),
                    Distance = Math.Round(distance, 2),
                    AverageRating = Math.Round(reader.GetDouble("AverageRating"), 2),
                    TotalRatings = reader.GetInt32("TotalRatings"),
                    IsAvailable = reader.GetBoolean("IsAvailable"),
                    EstimatedArrival = $"{estimatedArrival} dakika",
                    Latitude = driverLat,
                    Longitude = driverLng
                });
            }
        }

        return nearbyDrivers.OrderBy(d => d.Distance).Take(maxResults);
    }

    public async Task<MatchingResponse?> FindBestMatchAsync(CreateMatchingRequest request)
    {
        // 1. Calculate trip distance and cost
        var tripDistance = await CalculateDistanceAsync(
            request.PickupLatitude, request.PickupLongitude,
            request.DropoffLatitude, request.DropoffLongitude);

        var estimatedDuration = await EstimateArrivalTimeAsync(tripDistance);
        var estimatedCost = await CalculateEstimatedCostAsync(tripDistance, estimatedDuration);

        // 2. Find nearby available drivers
        var nearbyDrivers = await FindNearbyDriversAsync(
            request.PickupLatitude, request.PickupLongitude,
            request.MaxDistance ?? 10.0, 5, request.MinRating);

        if (!nearbyDrivers.Any())
        {
            return new MatchingResponse
            {
                Status = MatchingStatus.Rejected,
                Message = "Yakında müsait sürücü bulunamadı."
            };
        }

        // 3. Select best driver (closest with good rating)
        var bestDriver = nearbyDrivers.First();

        // 4. Create matching request
        var matchingRequest = new MatchingRequest
        {
            PassengerID = request.PassengerID,
            PickupAddress = request.PickupAddress,
            DropoffAddress = request.DropoffAddress,
            PickupLatitude = request.PickupLatitude,
            PickupLongitude = request.PickupLongitude,
            DropoffLatitude = request.DropoffLatitude,
            DropoffLongitude = request.DropoffLongitude,
            EstimatedCost = estimatedCost,
            EstimatedDistance = tripDistance,
            EstimatedDuration = estimatedDuration,
            MaxWaitTime = request.MaxWaitTime,
            MinRating = request.MinRating,
            MaxDistance = request.MaxDistance,
            Notes = request.Notes,
            Status = MatchingStatus.Pending,
            ExpiryTime = DateTime.UtcNow.AddMinutes(request.MaxWaitTime ?? 15)
        };

        var createdRequest = await CreateMatchingRequestAsync(matchingRequest);

        return new MatchingResponse
        {
            MatchingRequestID = createdRequest.MatchingRequestID,
            Status = MatchingStatus.Pending,
            DriverID = bestDriver.DriverID,
            DriverName = bestDriver.FullName,
            VehiclePlate = bestDriver.VehiclePlate,
            DriverPhone = bestDriver.PhoneNumber,
            Distance = bestDriver.Distance,
            EstimatedCost = estimatedCost,
            EstimatedArrival = bestDriver.EstimatedArrival,
            RequestTime = createdRequest.RequestTime,
            Message = "En uygun sürücü bulundu ve talep oluşturuldu."
        };
    }

    public async Task<MatchingResponse> AcceptMatchingRequestAsync(int driverId, int requestId)
    {
        const string sql = @"
            UPDATE MatchingRequests 
            SET DriverID = @DriverId, Status = @Status, AcceptedTime = @AcceptedTime, UpdatedDate = @UpdatedDate
            WHERE MatchingRequestID = @RequestId AND Status = @PendingStatus";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        command.Parameters.AddWithValue("@DriverId", driverId);
        command.Parameters.AddWithValue("@Status", (int)MatchingStatus.Accepted);
        command.Parameters.AddWithValue("@AcceptedTime", DateTime.UtcNow);
        command.Parameters.AddWithValue("@UpdatedDate", DateTime.UtcNow);
        command.Parameters.AddWithValue("@RequestId", requestId);
        command.Parameters.AddWithValue("@PendingStatus", (int)MatchingStatus.Pending);

        await connection.OpenAsync();
        var rowsAffected = await command.ExecuteNonQueryAsync();

        if (rowsAffected > 0)
        {
            var request = await GetMatchingRequestByIdAsync(requestId);
            return new MatchingResponse
            {
                MatchingRequestID = requestId,
                Status = MatchingStatus.Accepted,
                DriverID = driverId,
                RequestTime = request?.RequestTime ?? DateTime.UtcNow,
                AcceptedTime = DateTime.UtcNow,
                Message = "Talep başarıyla kabul edildi."
            };
        }

        return new MatchingResponse
        {
            MatchingRequestID = requestId,
            Status = MatchingStatus.Rejected,
            Message = "Talep kabul edilemedi. Talep bulunamadı veya zaten işlem görmüş."
        };
    }

    #endregion

    #region Utility Operations

    public async Task<decimal> CalculateEstimatedCostAsync(double distanceKm, double durationMinutes)
    {
        // Basit fiyat hesaplama: Base ücret + km başına ücret + dakika başına ücret
        var baseFare = 15.0m; // TL
        var perKmRate = 3.5m; // TL/km
        var perMinuteRate = 0.5m; // TL/dakika

        var totalCost = baseFare + ((decimal)distanceKm * perKmRate) + ((decimal)durationMinutes * perMinuteRate);
        
        return await Task.FromResult(Math.Round(totalCost, 2));
    }

    public async Task<double> CalculateDistanceAsync(double lat1, double lng1, double lat2, double lng2)
    {
        // Haversine formula for distance calculation
        const double earthRadius = 6371; // km

        var dLat = ToRadians(lat2 - lat1);
        var dLng = ToRadians(lng2 - lng1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLng / 2) * Math.Sin(dLng / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        var distance = earthRadius * c;

        return await Task.FromResult(distance);
    }

    public async Task<int> EstimateArrivalTimeAsync(double distanceKm, double avgSpeed = 30.0)
    {
        var timeInHours = distanceKm / avgSpeed;
        var timeInMinutes = timeInHours * 60;
        
        return await Task.FromResult((int)Math.Ceiling(timeInMinutes));
    }

    public async Task<bool> IsDriverAvailableAsync(int driverId)
    {
        const string sql = @"
            SELECT COUNT(1) 
            FROM DriverLocations dl
            WHERE dl.DriverID = @DriverId 
                AND dl.IsAvailable = 1 
                AND dl.IsOnline = 1
                AND dl.LocationTime > DATEADD(MINUTE, -30, GETUTCDATE())";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@DriverId", driverId);

        await connection.OpenAsync();
        var count = (int)await command.ExecuteScalarAsync();
        
        return count > 0;
    }

    public async Task<double> GetDriverAverageRatingAsync(int driverId)
    {
        return await _driverRatingRepository.GetAverageRatingByDriverIdAsync(driverId);
    }

    #endregion

    #region Additional Operations

    public async Task<IEnumerable<MatchingRequest>> GetMatchingRequestsByPassengerAsync(int passengerId)
    {
        var requests = new List<MatchingRequest>();
        const string sql = @"
            SELECT MatchingRequestID, PassengerID, DriverID, PickupAddress, DropoffAddress,
                   PickupLatitude, PickupLongitude, DropoffLatitude, DropoffLongitude,
                   EstimatedCost, EstimatedDistance, EstimatedDuration, Status, RequestTime,
                   AcceptedTime, ExpiryTime, Notes, MaxWaitTime, MinRating, MaxDistance,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM MatchingRequests 
            WHERE PassengerID = @PassengerId AND IsDeleted = 0 
            ORDER BY RequestTime DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@PassengerId", passengerId);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            requests.Add(MapMatchingRequestFromReader(reader));
        }

        return requests;
    }

    public async Task<IEnumerable<MatchingRequest>> GetMatchingRequestsByDriverAsync(int driverId)
    {
        var requests = new List<MatchingRequest>();
        const string sql = @"
            SELECT MatchingRequestID, PassengerID, DriverID, PickupAddress, DropoffAddress,
                   PickupLatitude, PickupLongitude, DropoffLatitude, DropoffLongitude,
                   EstimatedCost, EstimatedDistance, EstimatedDuration, Status, RequestTime,
                   AcceptedTime, ExpiryTime, Notes, MaxWaitTime, MinRating, MaxDistance,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM MatchingRequests 
            WHERE DriverID = @DriverId AND IsDeleted = 0 
            ORDER BY RequestTime DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@DriverId", driverId);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            requests.Add(MapMatchingRequestFromReader(reader));
        }

        return requests;
    }

    public async Task<MatchingRequest> UpdateMatchingRequestAsync(MatchingRequest request)
    {
        const string sql = @"
            UPDATE MatchingRequests 
            SET DriverID = @DriverID, Status = @Status, AcceptedTime = @AcceptedTime,
                UpdatedBy = @UpdatedBy, UpdatedDate = @UpdatedDate
            WHERE MatchingRequestID = @MatchingRequestID";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        command.Parameters.AddWithValue("@DriverID", (object?)request.DriverID ?? DBNull.Value);
        command.Parameters.AddWithValue("@Status", (int)request.Status);
        command.Parameters.AddWithValue("@AcceptedTime", (object?)request.AcceptedTime ?? DBNull.Value);
        command.Parameters.AddWithValue("@UpdatedBy", (object?)request.UpdatedBy ?? DBNull.Value);
        command.Parameters.AddWithValue("@UpdatedDate", DateTime.UtcNow);
        command.Parameters.AddWithValue("@MatchingRequestID", request.MatchingRequestID);

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();

        return request;
    }

    public async Task<bool> DeleteMatchingRequestAsync(int requestId)
    {
        const string sql = @"
            UPDATE MatchingRequests 
            SET IsDeleted = 1, DeletedDate = @DeletedDate
            WHERE MatchingRequestID = @RequestId";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@RequestId", requestId);
        command.Parameters.AddWithValue("@DeletedDate", DateTime.UtcNow);

        await connection.OpenAsync();
        var rowsAffected = await command.ExecuteNonQueryAsync();
        
        return rowsAffected > 0;
    }

    public async Task<DriverLocation?> GetDriverLocationAsync(int driverId)
    {
        const string sql = @"
            SELECT LocationID, DriverID, Latitude, Longitude, IsAvailable, CurrentAddress,
                   LocationTime, Speed, Heading, IsOnline, CreatedDate, UpdatedDate
            FROM DriverLocations 
            WHERE DriverID = @DriverId";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@DriverId", driverId);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapDriverLocationFromReader(reader);
        }

        return null;
    }

    public async Task<IEnumerable<DriverLocation>> GetAllDriverLocationsAsync()
    {
        var locations = new List<DriverLocation>();
        const string sql = @"
            SELECT LocationID, DriverID, Latitude, Longitude, IsAvailable, CurrentAddress,
                   LocationTime, Speed, Heading, IsOnline, CreatedDate, UpdatedDate
            FROM DriverLocations 
            WHERE IsOnline = 1 AND LocationTime > DATEADD(MINUTE, -30, GETUTCDATE())
            ORDER BY LocationTime DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            locations.Add(MapDriverLocationFromReader(reader));
        }

        return locations;
    }

    public async Task SetDriverAvailabilityAsync(int driverId, bool isAvailable)
    {
        const string sql = @"
            UPDATE DriverLocations 
            SET IsAvailable = @IsAvailable, UpdatedDate = @UpdatedDate
            WHERE DriverID = @DriverId";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@IsAvailable", isAvailable);
        command.Parameters.AddWithValue("@UpdatedDate", DateTime.UtcNow);
        command.Parameters.AddWithValue("@DriverId", driverId);

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    public async Task<MatchingResponse> RejectMatchingRequestAsync(int driverId, int requestId)
    {
        const string sql = @"
            UPDATE MatchingRequests 
            SET Status = @Status, UpdatedDate = @UpdatedDate
            WHERE MatchingRequestID = @RequestId AND Status = @PendingStatus";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        command.Parameters.AddWithValue("@Status", (int)MatchingStatus.Rejected);
        command.Parameters.AddWithValue("@UpdatedDate", DateTime.UtcNow);
        command.Parameters.AddWithValue("@RequestId", requestId);
        command.Parameters.AddWithValue("@PendingStatus", (int)MatchingStatus.Pending);

        await connection.OpenAsync();
        var rowsAffected = await command.ExecuteNonQueryAsync();

        return new MatchingResponse
        {
            MatchingRequestID = requestId,
            Status = rowsAffected > 0 ? MatchingStatus.Rejected : MatchingStatus.Pending,
            Message = rowsAffected > 0 ? "Talep reddedildi." : "Talep reddedilemedi."
        };
    }

    public async Task<MatchingResponse> CancelMatchingRequestAsync(int requestId)
    {
        const string sql = @"
            UPDATE MatchingRequests 
            SET Status = @Status, UpdatedDate = @UpdatedDate
            WHERE MatchingRequestID = @RequestId AND Status IN (@PendingStatus, @AcceptedStatus)";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        command.Parameters.AddWithValue("@Status", (int)MatchingStatus.Cancelled);
        command.Parameters.AddWithValue("@UpdatedDate", DateTime.UtcNow);
        command.Parameters.AddWithValue("@RequestId", requestId);
        command.Parameters.AddWithValue("@PendingStatus", (int)MatchingStatus.Pending);
        command.Parameters.AddWithValue("@AcceptedStatus", (int)MatchingStatus.Accepted);

        await connection.OpenAsync();
        var rowsAffected = await command.ExecuteNonQueryAsync();

        return new MatchingResponse
        {
            MatchingRequestID = requestId,
            Status = rowsAffected > 0 ? MatchingStatus.Cancelled : MatchingStatus.Pending,
            Message = rowsAffected > 0 ? "Talep iptal edildi." : "Talep iptal edilemedi."
        };
    }

    public async Task<int> GetActiveMatchingRequestsCountAsync()
    {
        const string sql = @"
            SELECT COUNT(1) 
            FROM MatchingRequests 
            WHERE Status IN (@PendingStatus, @AcceptedStatus) AND IsDeleted = 0";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@PendingStatus", (int)MatchingStatus.Pending);
        command.Parameters.AddWithValue("@AcceptedStatus", (int)MatchingStatus.Accepted);

        await connection.OpenAsync();
        return (int)await command.ExecuteScalarAsync();
    }

    public async Task<int> GetAvailableDriversCountAsync()
    {
        const string sql = @"
            SELECT COUNT(DISTINCT dl.DriverID)
            FROM DriverLocations dl
            INNER JOIN Drivers d ON dl.DriverID = d.DriverID
            WHERE dl.IsAvailable = 1 
                AND dl.IsOnline = 1 
                AND d.IsDeleted = 0
                AND dl.LocationTime > DATEADD(MINUTE, -30, GETUTCDATE())";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        await connection.OpenAsync();
        return (int)await command.ExecuteScalarAsync();
    }

    public async Task<IEnumerable<MatchingRequest>> GetMatchingRequestsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var requests = new List<MatchingRequest>();
        const string sql = @"
            SELECT MatchingRequestID, PassengerID, DriverID, PickupAddress, DropoffAddress,
                   PickupLatitude, PickupLongitude, DropoffLatitude, DropoffLongitude,
                   EstimatedCost, EstimatedDistance, EstimatedDuration, Status, RequestTime,
                   AcceptedTime, ExpiryTime, Notes, MaxWaitTime, MinRating, MaxDistance,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM MatchingRequests 
            WHERE RequestTime >= @StartDate AND RequestTime <= @EndDate AND IsDeleted = 0 
            ORDER BY RequestTime DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@StartDate", startDate);
        command.Parameters.AddWithValue("@EndDate", endDate);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            requests.Add(MapMatchingRequestFromReader(reader));
        }

        return requests;
    }

    #endregion

    #region Helper Methods

    private static double ToRadians(double degrees)
    {
        return degrees * (Math.PI / 180);
    }

    private static MatchingRequest MapMatchingRequestFromReader(SqlDataReader reader)
    {
        return new MatchingRequest
        {
            MatchingRequestID = reader.GetInt32("MatchingRequestID"),
            PassengerID = reader.GetInt32("PassengerID"),
            DriverID = reader.IsDBNull("DriverID") ? null : reader.GetInt32("DriverID"),
            PickupAddress = reader.GetString("PickupAddress"),
            DropoffAddress = reader.GetString("DropoffAddress"),
            PickupLatitude = reader.GetDouble("PickupLatitude"),
            PickupLongitude = reader.GetDouble("PickupLongitude"),
            DropoffLatitude = reader.GetDouble("DropoffLatitude"),
            DropoffLongitude = reader.GetDouble("DropoffLongitude"),
            EstimatedCost = reader.IsDBNull("EstimatedCost") ? null : reader.GetDecimal("EstimatedCost"),
            EstimatedDistance = reader.IsDBNull("EstimatedDistance") ? null : reader.GetDouble("EstimatedDistance"),
            EstimatedDuration = reader.IsDBNull("EstimatedDuration") ? null : reader.GetInt32("EstimatedDuration"),
            Status = (MatchingStatus)reader.GetInt32("Status"),
            RequestTime = reader.GetDateTime("RequestTime"),
            AcceptedTime = reader.IsDBNull("AcceptedTime") ? null : reader.GetDateTime("AcceptedTime"),
            ExpiryTime = reader.IsDBNull("ExpiryTime") ? null : reader.GetDateTime("ExpiryTime"),
            Notes = reader.IsDBNull("Notes") ? null : reader.GetString("Notes"),
            MaxWaitTime = reader.IsDBNull("MaxWaitTime") ? null : reader.GetDouble("MaxWaitTime"),
            MinRating = reader.IsDBNull("MinRating") ? null : reader.GetDouble("MinRating"),
            MaxDistance = reader.IsDBNull("MaxDistance") ? null : reader.GetDouble("MaxDistance"),
            CreatedBy = reader.IsDBNull("CreatedBy") ? null : reader.GetInt32("CreatedBy"),
            CreatedDate = reader.GetDateTime("CreatedDate"),
            UpdatedBy = reader.IsDBNull("UpdatedBy") ? null : reader.GetInt32("UpdatedBy"),
            UpdatedDate = reader.IsDBNull("UpdatedDate") ? null : reader.GetDateTime("UpdatedDate"),
            DeletedBy = reader.IsDBNull("DeletedBy") ? null : reader.GetInt32("DeletedBy"),
            DeletedDate = reader.IsDBNull("DeletedDate") ? null : reader.GetDateTime("DeletedDate"),
            IsDeleted = reader.GetBoolean("IsDeleted")
        };
    }

    private static DriverLocation MapDriverLocationFromReader(SqlDataReader reader)
    {
        return new DriverLocation
        {
            LocationID = reader.GetInt32("LocationID"),
            DriverID = reader.GetInt32("DriverID"),
            Latitude = reader.GetDouble("Latitude"),
            Longitude = reader.GetDouble("Longitude"),
            IsAvailable = reader.GetBoolean("IsAvailable"),
            CurrentAddress = reader.IsDBNull("CurrentAddress") ? null : reader.GetString("CurrentAddress"),
            LocationTime = reader.GetDateTime("LocationTime"),
            Speed = reader.IsDBNull("Speed") ? null : reader.GetDouble("Speed"),
            Heading = reader.IsDBNull("Heading") ? null : reader.GetDouble("Heading"),
            IsOnline = reader.GetBoolean("IsOnline"),
            CreatedDate = reader.GetDateTime("CreatedDate"),
            UpdatedDate = reader.IsDBNull("UpdatedDate") ? null : reader.GetDateTime("UpdatedDate")
        };
    }

    #endregion
}