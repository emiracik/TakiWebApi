using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace TakiWebApi.Controllers;

/// <summary>
/// Uygulama açılışında sürüm kontrolü. AppVersions tablosuna yeni sürüm
/// eklenip IsMandatory = 1 yapılırsa, ondan eski sürümleri kullananlara
/// "Güncelleme gerekli" ekranı gösterilir.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AppVersionsController : ControllerBase
{
    private readonly string _connectionString;
    private readonly ILogger<AppVersionsController> _logger;

    public AppVersionsController(IConfiguration config, ILogger<AppVersionsController> logger)
    {
        _connectionString = config.GetConnectionString("SqlServerConnection") ?? "";
        _logger = logger;
    }

    [HttpGet("check")]
    [AllowAnonymous]
    public async Task<IActionResult> Check([FromQuery] string platform, [FromQuery] string version)
    {
        if (!Version.TryParse(Normalize(version), out var current))
        {
            return BadRequest("Geçersiz sürüm.");
        }
        var rows = new List<(Version Version, bool Mandatory, string? Notes)>();
        try
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(
                "SELECT VersionNumber, IsMandatory, ReleaseNotes FROM AppVersions WHERE Platform = @P", connection);
            command.Parameters.AddWithValue("@P", (platform ?? "").Trim().ToLowerInvariant());
            await connection.OpenAsync();
            using var r = await command.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                if (Version.TryParse(Normalize(r.GetString(0)), out var v))
                {
                    rows.Add((v, r.GetBoolean(1), r.IsDBNull(2) ? null : r.GetString(2)));
                }
            }
        }
        catch (SqlException ex)
        {
            // Sürüm kontrolü uygulamanın açılmasını asla engellememeli.
            _logger.LogWarning(ex, "Sürüm kontrolü yapılamadı");
            return Ok(new { updateRequired = false, updateAvailable = false });
        }

        var newer = rows.Where(x => x.Version > current).OrderByDescending(x => x.Version).ToList();
        var latest = newer.FirstOrDefault();
        return Ok(new
        {
            updateRequired = newer.Any(x => x.Mandatory),
            updateAvailable = newer.Count > 0,
            latestVersion = latest.Version?.ToString(),
            releaseNotes = latest.Notes,
        });
    }

    // "1.2" → "1.2.0"; "1.0.0+5" → "1.0.0"
    private static string Normalize(string? v)
    {
        var s = (v ?? "").Split('+', '-')[0].Trim();
        return s.Count(c => c == '.') == 1 ? s + ".0" : s;
    }
}
