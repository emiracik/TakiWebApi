using System.Globalization;
using System.Text.Json;

namespace TakiWebApi.Services;

public record RoutePoint(double Latitude, double Longitude);

public record RouteEstimate(double DistanceKm, int DurationMinutes, bool IsRoadDistance);

public interface IRouteService
{
    /// <summary>
    /// Noktalar arasından (alış → duraklar → varış) geçen gerçek yol mesafesi
    /// ve süresi. Rota servisine ulaşılamazsa kuş uçuşu mesafe × yol katsayısı
    /// ile tahmin döner (IsRoadDistance = false).
    /// </summary>
    Task<RouteEstimate> EstimateAsync(IReadOnlyList<RoutePoint> points);
}

/// OSRM (Open Source Routing Machine) ile rota. Ayar:
///   "Routing": { "OsrmBaseUrl": "https://router.project-osrm.org", "TimeoutSeconds": 4 }
/// Not: router.project-osrm.org herkese açık demo sunucudur, yoğun ticari
/// kullanım için kendi OSRM sunucunuzu kurup adresi buraya yazın.
public class RouteService : IRouteService
{
    // Şehir içinde gerçek yol, kuş uçuşunun ortalama ~1,3 katı.
    private const double RoadFactor = 1.3;
    private const double FallbackSpeedKmh = 25;

    private readonly IHttpClientFactory _http;
    private readonly ILogger<RouteService> _logger;
    private readonly string _baseUrl;
    private readonly TimeSpan _timeout;

    public RouteService(IHttpClientFactory http, IConfiguration config, ILogger<RouteService> logger)
    {
        _http = http;
        _logger = logger;
        _baseUrl = (config["Routing:OsrmBaseUrl"] ?? "https://router.project-osrm.org").TrimEnd('/');
        _timeout = TimeSpan.FromSeconds(config.GetValue("Routing:TimeoutSeconds", 4));
    }

    public async Task<RouteEstimate> EstimateAsync(IReadOnlyList<RoutePoint> points)
    {
        if (points.Count < 2) return new RouteEstimate(0, 0, false);
        try
        {
            var coords = string.Join(";", points.Select(p =>
                $"{p.Longitude.ToString("0.000000", CultureInfo.InvariantCulture)},{p.Latitude.ToString("0.000000", CultureInfo.InvariantCulture)}"));
            var client = _http.CreateClient();
            client.Timeout = _timeout;
            client.DefaultRequestHeaders.UserAgent.ParseAdd("VeloxTaxiApi/1.0");
            using var response = await client.GetAsync($"{_baseUrl}/route/v1/driving/{coords}?overview=false");
            if (response.IsSuccessStatusCode)
            {
                using var doc = JsonDocument.Parse(await response.Content.ReadAsStreamAsync());
                var root = doc.RootElement;
                if (root.GetProperty("code").GetString() == "Ok")
                {
                    var route = root.GetProperty("routes")[0];
                    var km = route.GetProperty("distance").GetDouble() / 1000.0;
                    var min = (int)Math.Ceiling(route.GetProperty("duration").GetDouble() / 60.0);
                    return new RouteEstimate(Math.Round(km, 2), Math.Max(min, 1), true);
                }
            }
            _logger.LogWarning("Rota servisi yanıt vermedi ({Status}); kuş uçuşu tahmin kullanılıyor.", response.StatusCode);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException or KeyNotFoundException or InvalidOperationException)
        {
            _logger.LogWarning(ex, "Rota servisine ulaşılamadı; kuş uçuşu tahmin kullanılıyor.");
        }

        var straight = 0.0;
        for (var i = 1; i < points.Count; i++) straight += Haversine(points[i - 1], points[i]);
        var estKm = Math.Round(straight * RoadFactor, 2);
        return new RouteEstimate(estKm, Math.Max(1, (int)Math.Ceiling(estKm / FallbackSpeedKmh * 60)), false);
    }

    private static double Haversine(RoutePoint a, RoutePoint b)
    {
        const double r = 6371;
        double Rad(double d) => d * Math.PI / 180;
        var dLat = Rad(b.Latitude - a.Latitude);
        var dLng = Rad(b.Longitude - a.Longitude);
        var h = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(Rad(a.Latitude)) * Math.Cos(Rad(b.Latitude)) * Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
        return r * 2 * Math.Atan2(Math.Sqrt(h), Math.Sqrt(1 - h));
    }
}
