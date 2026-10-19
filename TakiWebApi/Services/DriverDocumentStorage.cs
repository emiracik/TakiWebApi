using Microsoft.Data.SqlClient;

namespace TakiWebApi.Services;

/// <summary>
/// Sürücü belgelerinin diskteki dosyaları. KVKK gereği hesap silinince tüm
/// belgeler, başvuru reddedilince adli sicil kaydı dosyasıyla birlikte imha edilir.
/// </summary>
public class DriverDocumentStorage
{
    public const string FilePrefix = "file:";

    private readonly string _root;
    private readonly string _connectionString;
    private readonly ILogger<DriverDocumentStorage> _logger;

    public DriverDocumentStorage(IConfiguration config, IWebHostEnvironment env, ILogger<DriverDocumentStorage> logger)
    {
        _root = Path.GetFullPath(config["Storage:DriverDocumentsPath"]
            ?? Path.Combine(env.ContentRootPath, "App_Data", "driver-documents"));
        _connectionString = config.GetConnectionString("SqlServerConnection") ?? "";
        _logger = logger;
    }

    public string Root => _root;

    public void DeleteFile(string documentUrl)
    {
        if (!documentUrl.StartsWith(FilePrefix)) return;
        var full = Path.GetFullPath(Path.Combine(_root, documentUrl[FilePrefix.Length..]));
        if (!full.StartsWith(_root, StringComparison.OrdinalIgnoreCase)) return;
        try { File.Delete(full); }
        catch (IOException ex) { _logger.LogWarning(ex, "Belge dosyası silinemedi: {Path}", full); }
    }

    /// <summary>Sürücünün belgelerini (isteğe bağlı yalnızca bir türü) kayıt ve dosyasıyla siler.</summary>
    public async Task<int> PurgeAsync(int driverId, string? documentType = null)
    {
        var urls = new List<string>();
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        using (var select = new SqlCommand(
            "SELECT DocumentUrl FROM DriverDocuments WHERE DriverID = @D AND (@T IS NULL OR DocumentType = @T)", connection))
        {
            select.Parameters.AddWithValue("@D", driverId);
            select.Parameters.AddWithValue("@T", (object?)documentType ?? DBNull.Value);
            using var r = await select.ExecuteReaderAsync();
            while (await r.ReadAsync()) urls.Add(r.GetString(0));
        }
        using (var delete = new SqlCommand(
            "DELETE FROM DriverDocuments WHERE DriverID = @D AND (@T IS NULL OR DocumentType = @T)", connection))
        {
            delete.Parameters.AddWithValue("@D", driverId);
            delete.Parameters.AddWithValue("@T", (object?)documentType ?? DBNull.Value);
            await delete.ExecuteNonQueryAsync();
        }
        foreach (var url in urls) DeleteFile(url);
        if (documentType == null)
        {
            // Sürücünün klasörü boş kaldıysa onu da kaldır.
            var dir = Path.Combine(_root, driverId.ToString());
            try { if (Directory.Exists(dir) && !Directory.EnumerateFileSystemEntries(dir).Any()) Directory.Delete(dir); }
            catch (IOException) { }
        }
        return urls.Count;
    }
}
