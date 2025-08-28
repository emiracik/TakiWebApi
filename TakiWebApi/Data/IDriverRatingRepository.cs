using TakiWebApi.Models;

namespace TakiWebApi.Data;

public interface IDriverRatingRepository
{
    Task<IEnumerable<DriverRating>> GetAllDriverRatingsAsync();
    Task<DriverRating?> GetDriverRatingByIdAsync(int ratingId);
    Task<IEnumerable<DriverRating>> GetDriverRatingsByDriverIdAsync(int driverId);
    Task<IEnumerable<DriverRating>> GetDriverRatingsByUserIdAsync(int userId);
    Task<IEnumerable<DriverRating>> GetDriverRatingsByTripIdAsync(int tripId);
    Task<IEnumerable<DriverRating>> GetActiveDriverRatingsAsync();
    Task<IEnumerable<DriverRating>> GetDriverRatingsPaginatedAsync(int pageNumber, int pageSize);
    Task<int> GetTotalDriverRatingsCountAsync();
    Task<int> GetActiveDriverRatingsCountAsync();
    Task<decimal> GetAverageRatingByDriverIdAsync(int driverId);
    Task<IEnumerable<DriverRating>> GetDriverRatingsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<DriverRating>> GetDriverRatingsByRatingValueAsync(decimal rating);
    Task<int> CreateDriverRatingAsync(DriverRating driverRating);
    Task<bool> UpdateDriverRatingAsync(DriverRating driverRating);
    Task<bool> DeleteDriverRatingAsync(int ratingId);
}
