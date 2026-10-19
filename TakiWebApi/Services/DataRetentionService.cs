using Microsoft.Data.SqlClient;

namespace TakiWebApi.Services;

/// <summary>
/// KVKK Aydınlatma Metni §12'deki saklama sürelerini uygular. Günde bir kez:
/// - Sohbet mesajları: 1 yıl sonra silinir.
/// - Puan yorumları: 1 yıl sonra yorum metni silinir (puan, ortalama için kalır).
/// - Eşleştirme talepleri (adres/konum/not): 2 yıl sonra silinir.
/// - Yolculuk konum/adres bilgisi: 2 yıl sonra temizlenir; ücret, tarih ve
///   ödeme bilgisi mali kayıt olarak (10 yıl) kalır.
/// - Destek/kayıp eşya talepleri, SMS doğrulama kayıtları: 2 yıl sonra silinir.
/// - Süresi dolmuş oturum anahtarları: 30 gün sonra silinir.
/// Ayar: "Retention": { "Enabled": true } (varsayılan açık).
/// </summary>
public class DataRetentionService : BackgroundService
{
    private static readonly (string Name, string Sql)[] Rules =
    {
        ("Sohbet mesajları (1 yıl)",
            "DELETE FROM Messages WHERE SentAt < DATEADD(year, -1, SYSUTCDATETIME())"),
        ("Sürücü puan yorumları (1 yıl)",
            "UPDATE DriverRatings SET Comment = NULL WHERE Comment IS NOT NULL AND CreatedDate < DATEADD(year, -1, GETDATE())"),
        ("Yolcu puan yorumları (1 yıl)",
            "UPDATE UserRatings SET Comment = NULL WHERE Comment IS NOT NULL AND CreatedDate < DATEADD(year, -1, GETDATE())"),
        ("Eşleştirme talepleri (2 yıl)",
            "DELETE FROM MatchingRequests WHERE RequestTime < DATEADD(year, -2, SYSUTCDATETIME())"),
        ("Yolculuk konum/adres bilgisi (2 yıl)",
            @"UPDATE Trips SET StartAddress = NULL, EndAddress = NULL, StartLatitude = NULL, StartLongitude = NULL,
                     EndLatitude = NULL, EndLongitude = NULL
              WHERE StartLatitude IS NOT NULL AND ISNULL(EndTime, CreatedDate) < DATEADD(year, -2, SYSUTCDATETIME())"),
        ("Destek ve kayıp eşya talepleri (2 yıl)",
            "DELETE FROM Feedbacks WHERE CreatedDate < DATEADD(year, -2, GETDATE())"),
        ("SMS doğrulama kayıtları (2 yıl)",
            "DELETE FROM SmsLogs WHERE SentAt < DATEADD(year, -2, GETDATE()); DELETE FROM OtpRequests WHERE CreatedDate < DATEADD(year, -2, GETDATE())"),
        ("Süresi dolmuş oturum anahtarları (30 gün)",
            "DELETE FROM RefreshTokens WHERE ExpiresAt < DATEADD(day, -30, SYSUTCDATETIME())"),
    };

    private readonly string _connectionString;
    private readonly bool _enabled;
    private readonly ILogger<DataRetentionService> _logger;

    public DataRetentionService(IConfiguration config, ILogger<DataRetentionService> logger)
    {
        _connectionString = config.GetConnectionString("SqlServerConnection") ?? "";
        _enabled = config.GetValue("Retention:Enabled", true);
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_enabled) return;
        // Uygulama açılışını yavaşlatmamak için biraz bekle, sonra günde bir çalış.
        await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        using var timer = new PeriodicTimer(TimeSpan.FromHours(24));
        do
        {
            await RunOnceAsync(stoppingToken);
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    public async Task RunOnceAsync(CancellationToken ct = default)
    {
        foreach (var (name, sql) in Rules)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                using var command = new SqlCommand(sql, connection) { CommandTimeout = 120 };
                await connection.OpenAsync(ct);
                var affected = await command.ExecuteNonQueryAsync(ct);
                if (affected > 0) _logger.LogInformation("Saklama süresi: {Rule} -> {Count} kayıt", name, affected);
            }
            catch (SqlException ex)
            {
                // Tablo/sütun yoksa ya da kısıt engellerse diğer kurallar yine çalışır.
                _logger.LogWarning(ex, "Saklama süresi kuralı çalışmadı: {Rule}", name);
            }
        }
    }
}
