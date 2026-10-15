using Microsoft.AspNetCore.SignalR;
using TakiWebApi.Hubs;

namespace TakiWebApi.Services;

public interface IRealtimeNotifier
{
    // Hepsi arka planda gönderir; isteği bekletmez, hata fırlatmaz.
    void Send(int subjectId, string subjectType, string type, IDictionary<string, string>? data = null);
    void ToDrivers(string type);
    void DriverLocation(int driverId, double latitude, double longitude);
}

public class RealtimeNotifier : IRealtimeNotifier
{
    private readonly IHubContext<RideHub> _hub;
    private readonly ILogger<RealtimeNotifier> _logger;

    public RealtimeNotifier(IHubContext<RideHub> hub, ILogger<RealtimeNotifier> logger)
    {
        _hub = hub;
        _logger = logger;
    }

    public void Send(int subjectId, string subjectType, string type, IDictionary<string, string>? data = null)
    {
        var payload = new Dictionary<string, object?> { ["type"] = type };
        if (data != null)
        {
            foreach (var (key, value) in data)
            {
                if (key != "type") payload[key] = value;
            }
        }
        Fire(RideHub.Subject(subjectType, subjectId), payload);
    }

    public void ToDrivers(string type) =>
        Fire(RideHub.Drivers, new Dictionary<string, object?> { ["type"] = type });

    public void DriverLocation(int driverId, double latitude, double longitude) =>
        Fire(RideHub.DriverLocation(driverId), new Dictionary<string, object?>
        {
            ["type"] = "driver_location",
            ["driverId"] = driverId,
            ["latitude"] = latitude,
            ["longitude"] = longitude,
        });

    private void Fire(string group, Dictionary<string, object?> payload)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await _hub.Clients.Group(group).SendAsync("update", payload);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Anlık güncelleme gönderilemedi: {Group}", group);
            }
        });
    }
}
