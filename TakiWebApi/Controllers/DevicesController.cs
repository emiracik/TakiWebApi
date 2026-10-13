using Microsoft.AspNetCore.Mvc;
using TakiWebApi.Data;
using TakiWebApi.Security;
using System.Security.Claims;

namespace TakiWebApi.Controllers;

// Anlık bildirim için cihaz token'ı kaydı. Token, istek yapan (token sahibi)
// yolcuya veya sürücüye bağlanır.
[ApiController]
[Route("api/[controller]")]
public class DevicesController : ControllerBase
{
    private readonly IDeviceTokenRepository _deviceTokens;

    public DevicesController(IDeviceTokenRepository deviceTokens) => _deviceTokens = deviceTokens;

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterDeviceRequest request)
    {
        var callerId = User.CallerId();
        var userType = User.FindFirstValue("user_type");
        if (callerId == null || (userType != "user" && userType != "driver"))
            return Forbid();
        if (string.IsNullOrWhiteSpace(request.Token) || request.Token.Length > 512)
            return BadRequest("Geçersiz cihaz token'ı.");

        var platform = request.Platform?.ToLowerInvariant();
        if (platform != null && platform != "android" && platform != "ios")
            platform = null;

        await _deviceTokens.UpsertAsync(request.Token, callerId.Value, userType, platform);
        return Ok(new { Success = true });
    }

    // Çıkışta: bu cihaza artık bildirim gönderilmez.
    [HttpDelete]
    public async Task<IActionResult> Unregister([FromBody] RegisterDeviceRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.Token))
            await _deviceTokens.DeleteAsync(request.Token);
        return Ok(new { Success = true });
    }
}

public class RegisterDeviceRequest
{
    public string Token { get; set; } = string.Empty;
    public string? Platform { get; set; }
}
