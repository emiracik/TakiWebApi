using TakiWebApi.Models;

namespace TakiWebApi.Data;

public interface ISharedRideRepository
{
    Task<IEnumerable<SharedRide>> GetAllAsync();
    Task<SharedRide?> GetByIdAsync(int sharedRideId);
    Task<IEnumerable<SharedRide>> GetByTripIdAsync(int tripId);
    Task<int> CreateAsync(SharedRide sharedRide);
    Task<bool> UpdateAsync(SharedRide sharedRide);
    Task<bool> DeleteAsync(int sharedRideId);
}
