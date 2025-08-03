using TakiWebApi.Models;

namespace TakiWebApi.Data;

public interface IUserCreditCardRepository
{
    Task<IEnumerable<UserCreditCard>> GetAllUserCreditCardsAsync();
    Task<UserCreditCard?> GetUserCreditCardByIdAsync(int cardId);
    Task<IEnumerable<UserCreditCard>> GetUserCreditCardsByUserIdAsync(int userId);
    Task<IEnumerable<UserCreditCard>> GetActiveUserCreditCardsAsync();
    Task<IEnumerable<UserCreditCard>> GetUserCreditCardsPaginatedAsync(int pageNumber, int pageSize);
    Task<int> GetTotalUserCreditCardsCountAsync();
    Task<int> GetActiveUserCreditCardsCountAsync();
    Task<IEnumerable<UserCreditCard>> SearchUserCreditCardsByNameAsync(string searchTerm);
    Task<IEnumerable<UserCreditCard>> GetUserCreditCardsByCreatedDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<int> CreateUserCreditCardAsync(UserCreditCard userCreditCard);
    Task<bool> UpdateUserCreditCardAsync(UserCreditCard userCreditCard);
    Task<bool> DeleteUserCreditCardAsync(int cardId);
}
