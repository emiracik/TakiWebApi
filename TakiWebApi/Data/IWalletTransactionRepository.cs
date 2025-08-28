using TakiWebApi.Models;

namespace TakiWebApi.Data;

public interface IWalletTransactionRepository
{
    Task<IEnumerable<WalletTransaction>> GetAllAsync();
    Task<WalletTransaction?> GetByIdAsync(int transactionId);
    Task<IEnumerable<WalletTransaction>> GetByUserIdAsync(int userId);
    Task<int> CreateAsync(WalletTransaction transaction);
}
