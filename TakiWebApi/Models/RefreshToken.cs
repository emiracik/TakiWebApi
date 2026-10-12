namespace TakiWebApi.Models;

// Veritabanındaki bir refresh token kaydı (token'ın kendisi değil hash'i).
public class RefreshToken
{
    public int RefreshTokenID { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public int SubjectId { get; set; }
    public string SubjectType { get; set; } = string.Empty; // "user" | "driver"
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
}
