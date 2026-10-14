using TakiWebApi.Data;

namespace TakiWebApi.Services;

/// <summary>
/// Dakikada bir, saatine 15 dakika kalan planlı yolculuklar için yolcuya
/// "taksi çağır" bildirimi gönderir. Tablo yoksa (Migrations/003
/// çalıştırılmadıysa) sessizce bekler.
/// </summary>
public class ScheduledRideReminderService : BackgroundService
{
    private static readonly TimeSpan Lead = TimeSpan.FromMinutes(15);

    private readonly IServiceScopeFactory _scopes;
    private readonly IPushNotificationService _push;
    private readonly ILogger<ScheduledRideReminderService> _logger;

    public ScheduledRideReminderService(IServiceScopeFactory scopes, IPushNotificationService push,
        ILogger<ScheduledRideReminderService> logger)
    {
        _scopes = scopes;
        _push = push;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        do
        {
            try
            {
                using var scope = _scopes.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<IScheduledRideRepository>();
                foreach (var ride in await repo.GetDueForReminderAsync(Lead))
                {
                    var local = TimeZoneInfo.ConvertTimeFromUtc(ride.ScheduledTime, TurkeyTime);
                    _push.Notify(ride.UserID, "user", "Planladığın yolculuk yaklaşıyor",
                        $"{local:HH:mm} · {ride.PickupAddress} → {ride.DropoffAddress}. Taksi çağırmak için dokun.",
                        new Dictionary<string, string>
                        {
                            ["type"] = "scheduled_ride",
                            ["scheduledRideId"] = ride.ScheduledRideID.ToString()
                        });
                    await repo.MarkRemindedAsync(ride.ScheduledRideID);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Planlı yolculuk hatırlatmaları gönderilemedi");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private static TimeZoneInfo TurkeyTime
    {
        get
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time"); }
            catch { return TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul"); }
        }
    }
}
