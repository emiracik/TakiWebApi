using TakiWebApi.Models;

namespace TakiWebApi.Data;

public interface ITripRatingRepository
{
    Task<IEnumerable<TripRating>> GetAllAsync();
    Task<TripRating?> GetByIdAsync(int id);
    Task<IEnumerable<TripRating>> GetByTripIdAsync(int tripId);
    Task<decimal> GetAverageByTripIdAsync(int tripId);
    Task AddAsync(TripRating rating);
}
