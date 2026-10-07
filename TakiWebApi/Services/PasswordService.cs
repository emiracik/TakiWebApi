using System.Security.Cryptography;
using System.Text;

namespace TakiWebApi.Services;

public interface IPasswordService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
    // Hash eski (Base64) formatta mı — doğru şifreyle girişte BCrypt'e çevrilmeli.
    bool NeedsRehash(string hash);
}

public class PasswordService : IPasswordService
{
    private const int WorkFactor = 11;

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    public bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash))
            return false;

        if (IsBcrypt(hash))
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch (BCrypt.Net.SaltParseException)
            {
                return false;
            }
        }

        // Eski kayıtlar şifreyi sadece Base64'e çevirerek tutuyordu. Bu
        // kullanıcılar bir kez giriş yapıp hash'leri BCrypt'e geçene kadar
        // eski formatı da kabul ediyoruz (sabit zamanlı karşılaştırma).
        var legacy = Convert.ToBase64String(Encoding.UTF8.GetBytes(password));
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(legacy),
            Encoding.UTF8.GetBytes(hash));
    }

    public bool NeedsRehash(string hash) => !IsBcrypt(hash);

    private static bool IsBcrypt(string hash) => hash.StartsWith("$2");
}
