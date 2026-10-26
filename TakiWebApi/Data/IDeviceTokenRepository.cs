namespace TakiWebApi.Data;

public interface IDeviceTokenRepository
{
    // Token başka bir hesaba bağlıysa (aynı cihazda hesap değişimi) yeni
    // sahibine taşınır.
    Task UpsertAsync(string token, int subjectId, string subjectType, string? platform);
    Task DeleteAsync(string token);
    // Hesap silinince o hesaba bağlı tüm cihaz token'ları kaldırılır.
    Task DeleteAllForSubjectAsync(int subjectId, string subjectType);
    Task<IReadOnlyList<string>> GetTokensAsync(int subjectId, string subjectType);
}
