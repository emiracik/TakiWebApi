using TakiWebApi.Models;

namespace TakiWebApi.Data;

public interface ICreditCardRepository
{
    Task<IEnumerable<CreditCard>> GetAllAsync();
    Task<CreditCard?> GetByIdAsync(int cardId);
    Task<IEnumerable<CreditCard>> GetByUserIdAsync(int userId);
    Task<int> CreateAsync(CreditCard card);
    Task<bool> UpdateAsync(CreditCard card);
    Task<bool> DeleteAsync(int cardId);
}
