using System.Data;
using Microsoft.Data.SqlClient;
using TakiWebApi.Models;

namespace TakiWebApi.Data;

public class AnnouncementRepository : IAnnouncementRepository
{
    private readonly string _connectionString;

    public AnnouncementRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("SqlServerConnection") 
            ?? throw new ArgumentNullException(nameof(configuration), "Connection string cannot be null");
    }

    public async Task<IEnumerable<Announcement>> GetAllAnnouncementsAsync()
    {
        var announcements = new List<Announcement>();
        const string sql = @"
            SELECT AnnouncementID, Title, Content, PublishedAt,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM Announcements
            ORDER BY PublishedAt DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            announcements.Add(MapAnnouncementFromReader(reader));
        }

        return announcements;
    }

    public async Task<Announcement?> GetAnnouncementByIdAsync(int announcementId)
    {
        const string sql = @"
            SELECT AnnouncementID, Title, Content, PublishedAt,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM Announcements
            WHERE AnnouncementID = @AnnouncementId";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@AnnouncementId", announcementId);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapAnnouncementFromReader(reader);
        }

        return null;
    }

    public async Task<IEnumerable<Announcement>> GetActiveAnnouncementsAsync()
    {
        var announcements = new List<Announcement>();
        const string sql = @"
            SELECT AnnouncementID, Title, Content, PublishedAt,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM Announcements
            WHERE IsDeleted = 0
            ORDER BY PublishedAt DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            announcements.Add(MapAnnouncementFromReader(reader));
        }

        return announcements;
    }

    public async Task<IEnumerable<Announcement>> GetAnnouncementsPaginatedAsync(int pageNumber, int pageSize)
    {
        var announcements = new List<Announcement>();
        const string sql = @"
            SELECT AnnouncementID, Title, Content, PublishedAt,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM Announcements
            ORDER BY PublishedAt DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Offset", (pageNumber - 1) * pageSize);
        command.Parameters.AddWithValue("@PageSize", pageSize);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            announcements.Add(MapAnnouncementFromReader(reader));
        }

        return announcements;
    }

    public async Task<int> GetTotalAnnouncementsCountAsync()
    {
        const string sql = "SELECT COUNT(*) FROM Announcements";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();

        return Convert.ToInt32(result);
    }

    public async Task<int> GetActiveAnnouncementsCountAsync()
    {
        const string sql = "SELECT COUNT(*) FROM Announcements WHERE IsDeleted = 0";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();

        return Convert.ToInt32(result);
    }

    public async Task<IEnumerable<Announcement>> SearchAnnouncementsByTitleAsync(string searchTerm)
    {
        var announcements = new List<Announcement>();
        const string sql = @"
            SELECT AnnouncementID, Title, Content, PublishedAt,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM Announcements
            WHERE Title LIKE @SearchTerm
            ORDER BY PublishedAt DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            announcements.Add(MapAnnouncementFromReader(reader));
        }

        return announcements;
    }

    public async Task<IEnumerable<Announcement>> GetAnnouncementsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var announcements = new List<Announcement>();
        const string sql = @"
            SELECT AnnouncementID, Title, Content, PublishedAt,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM Announcements
            WHERE PublishedAt >= @StartDate AND PublishedAt <= @EndDate
            ORDER BY PublishedAt DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@StartDate", startDate);
        command.Parameters.AddWithValue("@EndDate", endDate);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            announcements.Add(MapAnnouncementFromReader(reader));
        }

        return announcements;
    }

    private static Announcement MapAnnouncementFromReader(SqlDataReader reader)
    {
        return new Announcement
        {
            AnnouncementID = reader.GetInt32("AnnouncementID"),
            Title = reader.GetString("Title"),
            Content = reader.IsDBNull("Content") ? null : reader.GetString("Content"),
            PublishedAt = reader.GetDateTime("PublishedAt"),
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
