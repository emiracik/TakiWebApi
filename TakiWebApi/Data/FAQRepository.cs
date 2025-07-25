using System.Data;
using Microsoft.Data.SqlClient;
using TakiWebApi.Models;

namespace TakiWebApi.Data;

public class FAQRepository : IFAQRepository
{
    private readonly string _connectionString;

    public FAQRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("SqlServerConnection") 
            ?? throw new ArgumentNullException(nameof(configuration), "Connection string cannot be null");
    }

    public async Task<IEnumerable<FAQ>> GetAllFAQsAsync()
    {
        var faqs = new List<FAQ>();
        const string sql = @"
            SELECT FAQID, Question, Answer, IsActive, SortOrder,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM FAQs
            ORDER BY SortOrder, CreatedDate DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            faqs.Add(MapFAQFromReader(reader));
        }

        return faqs;
    }

    public async Task<FAQ?> GetFAQByIdAsync(int faqId)
    {
        const string sql = @"
            SELECT FAQID, Question, Answer, IsActive, SortOrder,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM FAQs
            WHERE FAQID = @FAQId";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@FAQId", faqId);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapFAQFromReader(reader);
        }

        return null;
    }

    public async Task<IEnumerable<FAQ>> GetActiveFAQsAsync()
    {
        var faqs = new List<FAQ>();
        const string sql = @"
            SELECT FAQID, Question, Answer, IsActive, SortOrder,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM FAQs
            WHERE IsActive = 1 AND IsDeleted = 0
            ORDER BY SortOrder, CreatedDate DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            faqs.Add(MapFAQFromReader(reader));
        }

        return faqs;
    }

    public async Task<IEnumerable<FAQ>> GetFAQsPaginatedAsync(int pageNumber, int pageSize)
    {
        var faqs = new List<FAQ>();
        const string sql = @"
            SELECT FAQID, Question, Answer, IsActive, SortOrder,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM FAQs
            ORDER BY SortOrder, CreatedDate DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Offset", (pageNumber - 1) * pageSize);
        command.Parameters.AddWithValue("@PageSize", pageSize);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            faqs.Add(MapFAQFromReader(reader));
        }

        return faqs;
    }

    public async Task<int> GetTotalFAQsCountAsync()
    {
        const string sql = "SELECT COUNT(*) FROM FAQs";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();

        return Convert.ToInt32(result);
    }

    public async Task<int> GetActiveFAQsCountAsync()
    {
        const string sql = "SELECT COUNT(*) FROM FAQs WHERE IsActive = 1 AND IsDeleted = 0";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();

        return Convert.ToInt32(result);
    }

    public async Task<IEnumerable<FAQ>> SearchFAQsByQuestionAsync(string searchTerm)
    {
        var faqs = new List<FAQ>();
        const string sql = @"
            SELECT FAQID, Question, Answer, IsActive, SortOrder,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM FAQs
            WHERE Question LIKE @SearchTerm OR Answer LIKE @SearchTerm
            ORDER BY SortOrder, CreatedDate DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            faqs.Add(MapFAQFromReader(reader));
        }

        return faqs;
    }

    private static FAQ MapFAQFromReader(SqlDataReader reader)
    {
        return new FAQ
        {
            FAQID = reader.GetInt32("FAQID"),
            Question = reader.GetString("Question"),
            Answer = reader.GetString("Answer"),
            IsActive = reader.GetBoolean("IsActive"),
            SortOrder = reader.IsDBNull("SortOrder") ? null : reader.GetInt32("SortOrder"),
            CreatedBy = reader.IsDBNull("CreatedBy") ? null : reader.GetInt32("CreatedBy"),
            CreatedDate = reader.GetDateTime("CreatedDate"),
            UpdatedBy = reader.IsDBNull("UpdatedBy") ? null : reader.GetInt32("UpdatedBy"),
            UpdatedDate = reader.IsDBNull("UpdatedDate") ? null : reader.GetDateTime("UpdatedDate"),
            DeletedBy = reader.IsDBNull("DeletedBy") ? null : reader.GetInt32("DeletedBy"),
            DeletedDate = reader.IsDBNull("DeletedDate") ? null : reader.GetDateTime("DeletedDate"),
            IsDeleted = reader.GetBoolean("IsDeleted")
        };
    }
}
