using TakiWebApi.Models;

namespace TakiWebApi.Data;

public interface IBankInfoRepository
{
    Task<IEnumerable<BankInfo>> GetAllAsync();
    Task<BankInfo?> GetByIdAsync(int bankInfoId);
    Task<IEnumerable<BankInfo>> GetByDriverIdAsync(int driverId);
    Task<int> CreateAsync(BankInfo bankInfo);
    Task<bool> UpdateAsync(BankInfo bankInfo);
    Task<bool> DeleteAsync(int bankInfoId);
}
