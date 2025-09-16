using TakiWebApi.Models;

namespace TakiWebApi.Data;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private static readonly List<RefreshToken> _tokens = new();

    public Task SaveRefreshTokenAsync(RefreshToken token)
    {
        _tokens.Add(token);
        return Task.CompletedTask;
    }

    public Task<RefreshToken?> GetRefreshTokenAsync(string token)
    {
        var found = _tokens.FirstOrDefault(t => t.Token == token && t.ExpiryDate > DateTime.UtcNow);
        return Task.FromResult(found);
    }

    public Task DeleteRefreshTokenAsync(string token)
    {
        _tokens.RemoveAll(t => t.Token == token);
        return Task.CompletedTask;
    }
}
