using TakiWebApi.Models;

namespace TakiWebApi.Data;

public interface IUserRatingRepository
{
    Task<IEnumerable<UserRating>> GetAllAsync();
    Task<UserRating?> GetByIdAsync(int id);
    Task<IEnumerable<UserRating>> GetByUserIdAsync(int userId);
    Task<IEnumerable<UserRating>> GetByDriverIdAsync(int driverId);
    Task<IEnumerable<UserRating>> GetByTripIdAsync(int tripId);
    Task<decimal> GetAverageByUserIdAsync(int userId);
    Task AddAsync(UserRating rating);
}
