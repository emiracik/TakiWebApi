namespace TakiWebApi.Models;

public class RefreshToken
{
    public string Token { get; set; } = string.Empty;
    public int UserId { get; set; }
    public DateTime ExpiryDate { get; set; }
}
