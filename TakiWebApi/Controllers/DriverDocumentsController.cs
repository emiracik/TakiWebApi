using TakiWebApi.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TakiWebApi.Data;
using TakiWebApi.Models;

namespace TakiWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DriverDocumentsController : ControllerBase
{
    // Sürücünün yükleyebileceği belge türleri.
    private static readonly HashSet<string> DocumentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "license", "registration", "insurance", "criminal_record", "profile_photo"
    };
    private static readonly Dictionary<string, string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = "image/jpeg", [".jpeg"] = "image/jpeg", [".png"] = "image/png", [".pdf"] = "application/pdf"
    };
    private const long MaxFileBytes = 8 * 1024 * 1024;
    private const string FilePrefix = "file:";

    private readonly IDriverDocumentRepository _repo;
    private readonly string _storageRoot;

    public DriverDocumentsController(IDriverDocumentRepository repo, IConfiguration config, IWebHostEnvironment env)
    {
        _repo = repo;
        // Belgeler kişisel veridir: wwwroot dışında saklanır, yalnızca yetkili uçtan okunur.
        _storageRoot = config["Storage:DriverDocumentsPath"]
            ?? Path.Combine(env.ContentRootPath, "App_Data", "driver-documents");
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<DriverDocument>>> GetAll() => Ok(await _repo.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<ActionResult<DriverDocument>> GetById(int id)
    {
        var doc = await _repo.GetByIdAsync(id);
        if (doc == null) return NotFound();
        if (!(User.IsDriver(doc.DriverID) || User.IsAdmin())) return Forbid();
        return Ok(doc);
    }

    [HttpGet("driver/{driverId}")]
    public async Task<ActionResult<IEnumerable<DriverDocument>>> GetByDriverId(int driverId) => Ok(await _repo.GetByDriverIdAsync(driverId));

    [HttpPost]
    public async Task<ActionResult<int>> Create([FromBody] DriverDocument doc)
    {
        if (!User.IsDriver(doc.DriverID)) return Forbid();
        doc.Status = "Pending";
        var id = await _repo.CreateAsync(doc);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    /// <summary>
    /// Sürücü belge fotoğrafı/PDF'i yükler (multipart: documentType, file).
    /// Belge "Pending" durumunda başlar; yönetici onaylar.
    /// </summary>
    [HttpPost("upload")]
    [RequestSizeLimit(MaxFileBytes + 64 * 1024)]
    public async Task<ActionResult<DriverDocument>> Upload([FromForm] string documentType, IFormFile file)
    {
        if (!User.IsAnyDriver() || User.CallerId() is not int driverId) return Forbid();
        if (!DocumentTypes.Contains(documentType)) return BadRequest("Geçersiz belge türü.");
        if (file == null || file.Length == 0) return BadRequest("Dosya seçin.");
        if (file.Length > MaxFileBytes) return BadRequest("Dosya en fazla 8 MB olabilir.");

        var ext = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.ContainsKey(ext)) return BadRequest("Yalnızca JPG, PNG veya PDF yüklenebilir.");

        var relative = Path.Combine(driverId.ToString(), $"{documentType.ToLowerInvariant()}-{Guid.NewGuid():N}{ext.ToLowerInvariant()}");
        var fullPath = Path.Combine(_storageRoot, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        await using (var stream = System.IO.File.Create(fullPath))
        {
            await file.CopyToAsync(stream);
        }

        var doc = new DriverDocument
        {
            DriverID = driverId,
            DocumentType = documentType.ToLowerInvariant(),
            DocumentUrl = FilePrefix + relative.Replace('\\', '/'),
            UploadedDate = DateTime.UtcNow,
        };
        doc.DocumentID = await _repo.CreateAsync(doc);
        return Ok(await _repo.GetByIdAsync(doc.DocumentID) ?? doc);
    }

    /// <summary>Yüklenen belgenin dosyası (yalnızca sahibi sürücü ve yönetici).</summary>
    [HttpGet("{id}/file")]
    public async Task<IActionResult> GetFile(int id)
    {
        var doc = await _repo.GetByIdAsync(id);
        if (doc == null) return NotFound();
        if (!(User.IsDriver(doc.DriverID) || User.IsAdmin())) return Forbid();
        if (!doc.DocumentUrl.StartsWith(FilePrefix)) return NotFound();

        var relative = doc.DocumentUrl[FilePrefix.Length..];
        var root = Path.GetFullPath(_storageRoot);
        var fullPath = Path.GetFullPath(Path.Combine(root, relative));
        if (!fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase) || !System.IO.File.Exists(fullPath))
        {
            return NotFound();
        }
        var type = AllowedExtensions.GetValueOrDefault(Path.GetExtension(fullPath), "application/octet-stream");
        return PhysicalFile(fullPath, type);
    }

    public record DocumentReview(string Status, string? Note);

    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SetStatus(int id, [FromBody] DocumentReview review)
    {
        if (review.Status is not ("Pending" or "Approved" or "Rejected")) return BadRequest("Geçersiz durum.");
        return await _repo.UpdateStatusAsync(id, review.Status, review.Note) ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing == null) return NotFound();
        if (!User.IsDriver(existing.DriverID)) return Forbid();
        var success = await _repo.DeleteAsync(id);
        return success ? NoContent() : NotFound();
    }
}
