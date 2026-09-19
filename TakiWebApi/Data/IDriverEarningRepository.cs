using TakiWebApi.Models;

namespace TakiWebApi.Data;

public interface IDriverEarningRepository
{
    Task<DriverEarning> CreateAsync(DriverEarning earning);
    Task<IEnumerable<DriverEarning>> GetByDriverIdAsync(int driverId);
    Task<decimal> GetTotalByDriverIdAsync(int driverId);
    // Aynı yolculuk için ikinci kez hakediş yazılmasını önlemek amacıyla
    // (ör. hem confirm hem complete tetiklenirse).
    Task<bool> ExistsForTripAsync(int tripId);
}
