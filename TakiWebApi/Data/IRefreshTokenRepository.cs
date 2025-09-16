using TakiWebApi.Models;

namespace TakiWebApi.Data;

public interface IRefreshTokenRepository
{
    Task SaveRefreshTokenAsync(RefreshToken token);
    Task<RefreshToken?> GetRefreshTokenAsync(string token);
    Task DeleteRefreshTokenAsync(string token);
}
