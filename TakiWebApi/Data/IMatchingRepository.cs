using TakiWebApi.Models;

namespace TakiWebApi.Data;

public interface IMatchingRepository
{
    // Matching Request Operations
    Task<MatchingRequest> CreateMatchingRequestAsync(MatchingRequest request);
    Task<MatchingRequest?> GetMatchingRequestByIdAsync(int requestId);
    Task<IEnumerable<MatchingRequest>> GetMatchingRequestsByPassengerAsync(int passengerId);
    Task<IEnumerable<MatchingRequest>> GetMatchingRequestsByDriverAsync(int driverId);
    Task<IEnumerable<MatchingRequest>> GetPendingMatchingRequestsAsync();
    Task<MatchingRequest> UpdateMatchingRequestAsync(MatchingRequest request);
    Task<bool> DeleteMatchingRequestAsync(int requestId);

    // Driver Location Operations
    Task UpdateDriverLocationAsync(int driverId, UpdateDriverLocationRequest locationRequest);
    Task<DriverLocation?> GetDriverLocationAsync(int driverId);
    Task<IEnumerable<DriverLocation>> GetAllDriverLocationsAsync();
    Task SetDriverAvailabilityAsync(int driverId, bool isAvailable);

    // Matching Algorithm Operations
    Task<IEnumerable<NearbyDriverResponse>> FindNearbyDriversAsync(
        double latitude, double longitude, double radiusKm, int maxResults, double? minRating = null);
    
    Task<MatchingResponse?> FindBestMatchAsync(CreateMatchingRequest request);
    
    Task<MatchingResponse> AcceptMatchingRequestAsync(int driverId, int requestId);
    
    Task<MatchingResponse> RejectMatchingRequestAsync(int driverId, int requestId);
    
    Task<MatchingResponse> CancelMatchingRequestAsync(int requestId);

    // İptal nedeni ve ücreti (Migrations/003).
    Task SetCancellationInfoAsync(int requestId, string? reason, decimal? fee);

    // Sürücü karşı teklif sistemi: sürücü bekleyen bir talebe kabul yerine
    // kendi fiyatını önerebilir; yolcu bunu kabul/red eder.
    Task<MatchingResponse> DriverCounterOfferAsync(int driverId, int requestId, decimal amount);
    Task<MatchingResponse> RespondToCounterOfferAsync(int requestId, bool accept);

    // Utility Operations
    Task<decimal> CalculateEstimatedCostAsync(double distanceKm, double durationMinutes, string? vehicleType = null);
    Task<double> CalculateDistanceAsync(double lat1, double lng1, double lat2, double lng2);
    Task<TakiWebApi.Services.RouteEstimate> EstimateRouteAsync(CreateMatchingRequest request);
    Task<int> EstimateArrivalTimeAsync(double distanceKm, double avgSpeed = 30.0);
    Task<bool> IsDriverAvailableAsync(int driverId);
    Task<double> GetDriverAverageRatingAsync(int driverId);

    // Statistics and Analytics
    Task<int> GetActiveMatchingRequestsCountAsync();
    Task<int> GetAvailableDriversCountAsync();
    Task<IEnumerable<MatchingRequest>> GetMatchingRequestsByDateRangeAsync(DateTime startDate, DateTime endDate);
}