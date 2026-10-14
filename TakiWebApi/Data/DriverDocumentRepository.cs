using Microsoft.Data.SqlClient;
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
            list.Add(Map(reader));
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
            return Map(reader);
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
            list.Add(Map(reader));
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

    public async Task<bool> UpdateStatusAsync(int documentId, string status, string? reviewNote)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(
            "UPDATE DriverDocuments SET Status = @Status, ReviewNote = @Note WHERE DocumentID = @DocumentID", connection);
        command.Parameters.AddWithValue("@Status", status);
        command.Parameters.AddWithValue("@Note", (object?)reviewNote ?? DBNull.Value);
        command.Parameters.AddWithValue("@DocumentID", documentId);
        await connection.OpenAsync();
        return await command.ExecuteNonQueryAsync() > 0;
    }

    // Status/ReviewNote Migrations/003 ile geldi; sütun yoksa "Pending" say.
    private static DriverDocument Map(SqlDataReader reader)
    {
        bool Has(string name)
        {
            for (var i = 0; i < reader.FieldCount; i++)
                if (reader.GetName(i).Equals(name, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }
        return new DriverDocument
        {
            DocumentID = reader.GetInt32(reader.GetOrdinal("DocumentID")),
            DriverID = reader.GetInt32(reader.GetOrdinal("DriverID")),
            DocumentType = reader.GetString(reader.GetOrdinal("DocumentType")),
            DocumentUrl = reader.GetString(reader.GetOrdinal("DocumentUrl")),
            UploadedDate = reader.GetDateTime(reader.GetOrdinal("UploadedDate")),
            Status = Has("Status") && !reader.IsDBNull(reader.GetOrdinal("Status"))
                ? reader.GetString(reader.GetOrdinal("Status")) : "Pending",
            ReviewNote = Has("ReviewNote") && !reader.IsDBNull(reader.GetOrdinal("ReviewNote"))
                ? reader.GetString(reader.GetOrdinal("ReviewNote")) : null,
        };
    }
}
