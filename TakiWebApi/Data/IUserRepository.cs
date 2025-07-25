using TakiWebApi.Models;

namespace TakiWebApi.Data;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(int userId);
    Task<User?> GetUserByPhoneNumberAsync(string phoneNumber);
    Task<User?> GetUserByEmailAsync(string email);
    Task<IEnumerable<User>> GetActiveUsersAsync();
    Task<IEnumerable<User>> GetUsersPaginatedAsync(int pageNumber, int pageSize);
    Task<int> GetTotalUsersCountAsync();
    Task<int> GetActiveUsersCountAsync();
    Task<IEnumerable<User>> SearchUsersByNameAsync(string searchTerm);
    Task<IEnumerable<User>> GetUsersByCreatedDateRangeAsync(DateTime startDate, DateTime endDate);
}
