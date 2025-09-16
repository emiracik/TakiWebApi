using System.Security.Cryptography;
using System.Text;

namespace TakiWebApi.Services;

public interface IPasswordService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}

public class PasswordService : IPasswordService
{
    public string HashPassword(string password)
    {
        // Simple Base64 encoding for demo purposes
        // In production, use BCrypt or Argon2
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        return Convert.ToBase64String(passwordBytes);
    }

    public bool VerifyPassword(string password, string hash)
    {
        var passwordHash = HashPassword(password);
        return passwordHash == hash;
    }
}
