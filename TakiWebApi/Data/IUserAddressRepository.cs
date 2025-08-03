using TakiWebApi.Models;

namespace TakiWebApi.Data;

public interface IUserAddressRepository
{
    Task<IEnumerable<UserAddress>> GetAllUserAddressesAsync();
    Task<UserAddress?> GetUserAddressByIdAsync(int addressId);
    Task<IEnumerable<UserAddress>> GetUserAddressesByUserIdAsync(int userId);
    Task<IEnumerable<UserAddress>> GetActiveUserAddressesAsync();
    Task<IEnumerable<UserAddress>> GetUserAddressesPaginatedAsync(int pageNumber, int pageSize);
    Task<int> GetTotalUserAddressesCountAsync();
    Task<int> GetActiveUserAddressesCountAsync();
    Task<IEnumerable<UserAddress>> SearchUserAddressesByTitleAsync(string searchTerm);
    Task<IEnumerable<UserAddress>> GetUserAddressesByCreatedDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<int> CreateUserAddressAsync(UserAddress userAddress);
    Task<bool> UpdateUserAddressAsync(UserAddress userAddress);
    Task<bool> DeleteUserAddressAsync(int addressId);
}
