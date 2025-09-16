using System.Collections.Generic;
using TakiWebApi.Models;

namespace TakiWebApi.Data
{
    public interface IWalletRepository
    {
        Task<Wallet?> GetWalletByUserIdAsync(int userId);
        Task<List<WalletTransaction>> GetLastTransactionsAsync(int userId, int count = 10);
        Task AddTransactionAsync(WalletTransaction transaction);
        Task UpdateWalletBalanceAsync(int walletId, decimal newBalance, decimal totalIn, decimal totalOut);
        Task<Wallet> CreateWalletAsync(int userId);
    }
}
