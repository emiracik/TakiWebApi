using System.Data.SqlClient;
using TakiWebApi.Models;

namespace TakiWebApi.Data;

public class DriverDocumentRepository : IDriverDocumentRepository
{
    private readonly string _connectionString;
    public DriverDocumentRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection");
    }

    public async Task<IEnumerable<DriverDocument>> GetAllAsync()
    {
        var list = new List<DriverDocument>();
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("SELECT * FROM DriverDocuments", connection);
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new DriverDocument
            {
                DocumentID = reader.GetInt32(reader.GetOrdinal("DocumentID")),
                DriverID = reader.GetInt32(reader.GetOrdinal("DriverID")),
                DocumentType = reader.GetString(reader.GetOrdinal("DocumentType")),
                DocumentUrl = reader.GetString(reader.GetOrdinal("DocumentUrl")),
                UploadedDate = reader.GetDateTime(reader.GetOrdinal("UploadedDate"))
            });
        }
        return list;
    }

    public async Task<DriverDocument?> GetByIdAsync(int documentId)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("SELECT * FROM DriverDocuments WHERE DocumentID = @DocumentID", connection);
        command.Parameters.AddWithValue("@DocumentID", documentId);
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new DriverDocument
            {
                DocumentID = reader.GetInt32(reader.GetOrdinal("DocumentID")),
                DriverID = reader.GetInt32(reader.GetOrdinal("DriverID")),
                DocumentType = reader.GetString(reader.GetOrdinal("DocumentType")),
                DocumentUrl = reader.GetString(reader.GetOrdinal("DocumentUrl")),
                UploadedDate = reader.GetDateTime(reader.GetOrdinal("UploadedDate"))
            };
        }
        return null;
    }

    public async Task<IEnumerable<DriverDocument>> GetByDriverIdAsync(int driverId)
    {
        var list = new List<DriverDocument>();
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("SELECT * FROM DriverDocuments WHERE DriverID = @DriverID", connection);
        command.Parameters.AddWithValue("@DriverID", driverId);
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new DriverDocument
            {
                DocumentID = reader.GetInt32(reader.GetOrdinal("DocumentID")),
                DriverID = reader.GetInt32(reader.GetOrdinal("DriverID")),
                DocumentType = reader.GetString(reader.GetOrdinal("DocumentType")),
                DocumentUrl = reader.GetString(reader.GetOrdinal("DocumentUrl")),
                UploadedDate = reader.GetDateTime(reader.GetOrdinal("UploadedDate"))
            });
        }
        return list;
    }

    public async Task<int> CreateAsync(DriverDocument document)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(
            @"INSERT INTO DriverDocuments (DriverID, DocumentType, DocumentUrl, UploadedDate)
              VALUES (@DriverID, @DocumentType, @DocumentUrl, @UploadedDate);
              SELECT SCOPE_IDENTITY();", connection);
        command.Parameters.AddWithValue("@DriverID", document.DriverID);
        command.Parameters.AddWithValue("@DocumentType", document.DocumentType);
        command.Parameters.AddWithValue("@DocumentUrl", document.DocumentUrl);
        command.Parameters.AddWithValue("@UploadedDate", document.UploadedDate);
        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<bool> DeleteAsync(int documentId)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("DELETE FROM DriverDocuments WHERE DocumentID = @DocumentID", connection);
        command.Parameters.AddWithValue("@DocumentID", documentId);
        await connection.OpenAsync();
        var affected = await command.ExecuteNonQueryAsync();
        return affected > 0;
    }
}
