using TakiWebApi.Models;

namespace TakiWebApi.Data;

public interface IDriverRepository
{
    Task<IEnumerable<Driver>> GetAllDriversAsync();
    Task<Driver?> GetDriverByIdAsync(int driverId);
    Task<Driver?> GetDriverByPhoneNumberAsync(string phoneNumber);
    Task<Driver?> GetDriverByEmailAsync(string email);
    Task<IEnumerable<Driver>> GetActiveDriversAsync();
    Task<IEnumerable<Driver>> GetDriversPaginatedAsync(int pageNumber, int pageSize);
    Task<int> GetTotalDriversCountAsync();
    Task<int> GetActiveDriversCountAsync();
    Task<IEnumerable<Driver>> SearchDriversByNameAsync(string searchTerm);
    Task<IEnumerable<Driver>> GetDriversByCreatedDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<Driver>> GetDriversByVehiclePlateAsync(string vehiclePlate);
    Task<Driver> AddDriverAsync(Driver driver);
    Task<Driver> UpdateDriverAsync(Driver driver);
    Task<bool> DeleteDriverAsync(int driverId);
}
