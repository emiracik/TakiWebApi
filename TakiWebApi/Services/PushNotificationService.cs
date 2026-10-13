using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Google.Apis.Auth.OAuth2;
using TakiWebApi.Data;

namespace TakiWebApi.Services;

public interface IPushNotificationService
{
    // Bildirimi arka planda gönderir; isteği bekletmez, hata fırlatmaz.
    void Notify(int subjectId, string subjectType, string title, string body,
        IDictionary<string, string>? data = null);
}

// Firebase Cloud Messaging (HTTP v1) ile anlık bildirim. Ayar:
//   "Firebase": { "ProjectId": "...", "ServiceAccountFile": "C:/.../firebase-sa.json" }
// Ayar yoksa bildirim gönderilmez (uygulama normal çalışır).
public class PushNotificationService : IPushNotificationService
{
    private const string Scope = "https://www.googleapis.com/auth/firebase.messaging";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IDeviceTokenRepository _deviceTokens;
    private readonly ILogger<PushNotificationService> _logger;
    private readonly string? _projectId;
    private readonly ITokenAccess? _credential;

    public PushNotificationService(IConfiguration configuration, IHttpClientFactory httpClientFactory,
        ILogger<PushNotificationService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _deviceTokens = new DeviceTokenRepository(configuration);

        _projectId = configuration["Firebase:ProjectId"];
        var serviceAccountFile = configuration["Firebase:ServiceAccountFile"];
        if (string.IsNullOrWhiteSpace(_projectId) || string.IsNullOrWhiteSpace(serviceAccountFile)
            || !File.Exists(serviceAccountFile))
        {
            _logger.LogWarning("Firebase ayarı yok; anlık bildirimler kapalı.");
            return;
        }

        _credential = CredentialFactory.FromFile<ServiceAccountCredential>(serviceAccountFile)
            .ToGoogleCredential()
            .CreateScoped(Scope);
    }

    public void Notify(int subjectId, string subjectType, string title, string body,
        IDictionary<string, string>? data = null)
    {
        if (_credential == null)
            return;

        _ = Task.Run(async () =>
        {
            try
            {
                await SendAsync(subjectId, subjectType, title, body, data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bildirim gönderilemedi: {Type} {Id}", subjectType, subjectId);
            }
        });
    }

    private async Task SendAsync(int subjectId, string subjectType, string title, string body,
        IDictionary<string, string>? data)
    {
        var tokens = await _deviceTokens.GetTokensAsync(subjectId, subjectType);
        if (tokens.Count == 0)
            return;

        var accessToken = await _credential!.GetAccessTokenForRequestAsync();
        var client = _httpClientFactory.CreateClient();
        var url = $"https://fcm.googleapis.com/v1/projects/{_projectId}/messages:send";

        foreach (var token in tokens)
        {
            var payload = new
            {
                message = new
                {
                    token,
                    notification = new { title, body },
                    data = data ?? new Dictionary<string, string>(),
                    android = new { priority = "high", notification = new { channel_id = "velox_ride_channel" } },
                    apns = new { payload = new { aps = new { sound = "default" } } }
                }
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(payload)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
                continue;

            var error = await response.Content.ReadAsStringAsync();
            // Uygulama silinmiş / token geçersiz: bir daha deneme.
            if (response.StatusCode is HttpStatusCode.NotFound
                || error.Contains("UNREGISTERED", StringComparison.Ordinal)
                || error.Contains("INVALID_ARGUMENT", StringComparison.Ordinal))
            {
                await _deviceTokens.DeleteAsync(token);
            }
            else
            {
                _logger.LogWarning("FCM hatası {Status}: {Error}", (int)response.StatusCode, error);
            }
        }
    }
}
