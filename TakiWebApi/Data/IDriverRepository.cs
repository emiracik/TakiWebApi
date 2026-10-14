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
    Task<bool> UpdateDriverPasswordAsync(int driverId, string newPasswordHash);
    // Hesap silme: kişisel veriyi anonimleştirir ve hesabı siler.
    Task<bool> AnonymizeAndDeleteDriverAsync(int driverId);

    // Onay süreci (Migrations/004). Sütun yoksa herkes "Approved" sayılır.
    Task<(string Status, string? Note)> GetApprovalAsync(int driverId);
    Task<bool> SetApprovalAsync(int driverId, string status, string? note);
    Task<IEnumerable<Driver>> GetDriversByApprovalAsync(string status);
}
