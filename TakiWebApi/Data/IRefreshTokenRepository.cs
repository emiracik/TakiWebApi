using TakiWebApi.Models;

namespace TakiWebApi.Data;

public interface IRefreshTokenRepository
{
    // Yeni refresh token üretir, hash'ini kaydeder ve ham token'ı döner.
    Task<string> IssueAsync(int subjectId, string subjectType);

    // Token'ı kullanır ve yerine yenisini verir. Geçersiz/süresi dolmuşsa
    // null döner. Daha önce kullanılmış (iptal edilmiş) bir token tekrar
    // gelirse çalınmış kabul edilir ve o hesabın tüm token'ları iptal edilir.
    Task<(RefreshToken Record, string NewToken)?> RotateAsync(string rawToken);

    Task RevokeAsync(string rawToken);
    Task RevokeAllAsync(int subjectId, string subjectType);
}
