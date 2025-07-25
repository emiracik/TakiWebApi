using System.Data;
using Microsoft.Data.SqlClient;
using TakiWebApi.Models;

namespace TakiWebApi.Data;

public class BlogRepository : IBlogRepository
{
    private readonly string _connectionString;

    public BlogRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("SqlServerConnection") 
            ?? throw new ArgumentNullException(nameof(configuration), "Connection string cannot be null");
    }

    public async Task<IEnumerable<Blog>> GetAllBlogsAsync()
    {
        var blogs = new List<Blog>();
        const string sql = @"
            SELECT BlogID, Title, Content, ImageUrl, PublishedAt, IsPublished,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM Blogs
            ORDER BY CreatedDate DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            blogs.Add(MapBlogFromReader(reader));
        }

        return blogs;
    }

    public async Task<Blog?> GetBlogByIdAsync(int blogId)
    {
        const string sql = @"
            SELECT BlogID, Title, Content, ImageUrl, PublishedAt, IsPublished,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM Blogs
            WHERE BlogID = @BlogId";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@BlogId", blogId);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapBlogFromReader(reader);
        }

        return null;
    }

    public async Task<IEnumerable<Blog>> GetPublishedBlogsAsync()
    {
        var blogs = new List<Blog>();
        const string sql = @"
            SELECT BlogID, Title, Content, ImageUrl, PublishedAt, IsPublished,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM Blogs
            WHERE IsPublished = 1 AND IsDeleted = 0
            ORDER BY PublishedAt DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            blogs.Add(MapBlogFromReader(reader));
        }

        return blogs;
    }

    public async Task<IEnumerable<Blog>> GetActiveBlogsAsync()
    {
        var blogs = new List<Blog>();
        const string sql = @"
            SELECT BlogID, Title, Content, ImageUrl, PublishedAt, IsPublished,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM Blogs
            WHERE IsDeleted = 0
            ORDER BY CreatedDate DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            blogs.Add(MapBlogFromReader(reader));
        }

        return blogs;
    }

    public async Task<IEnumerable<Blog>> GetBlogsPaginatedAsync(int pageNumber, int pageSize)
    {
        var blogs = new List<Blog>();
        const string sql = @"
            SELECT BlogID, Title, Content, ImageUrl, PublishedAt, IsPublished,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM Blogs
            ORDER BY CreatedDate DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Offset", (pageNumber - 1) * pageSize);
        command.Parameters.AddWithValue("@PageSize", pageSize);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            blogs.Add(MapBlogFromReader(reader));
        }

        return blogs;
    }

    public async Task<int> GetTotalBlogsCountAsync()
    {
        const string sql = "SELECT COUNT(*) FROM Blogs";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();

        return Convert.ToInt32(result);
    }

    public async Task<int> GetPublishedBlogsCountAsync()
    {
        const string sql = "SELECT COUNT(*) FROM Blogs WHERE IsPublished = 1 AND IsDeleted = 0";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();

        return Convert.ToInt32(result);
    }

    public async Task<IEnumerable<Blog>> SearchBlogsByTitleAsync(string searchTerm)
    {
        var blogs = new List<Blog>();
        const string sql = @"
            SELECT BlogID, Title, Content, ImageUrl, PublishedAt, IsPublished,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM Blogs
            WHERE Title LIKE @SearchTerm
            ORDER BY CreatedDate DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            blogs.Add(MapBlogFromReader(reader));
        }

        return blogs;
    }

    public async Task<IEnumerable<Blog>> GetBlogsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var blogs = new List<Blog>();
        const string sql = @"
            SELECT BlogID, Title, Content, ImageUrl, PublishedAt, IsPublished,
                   CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, DeletedBy, DeletedDate, IsDeleted
            FROM Blogs
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
            blogs.Add(MapBlogFromReader(reader));
        }

        return blogs;
    }

    private static Blog MapBlogFromReader(SqlDataReader reader)
    {
        return new Blog
        {
            BlogID = reader.GetInt32("BlogID"),
            Title = reader.GetString("Title"),
            Content = reader.IsDBNull("Content") ? null : reader.GetString("Content"),
            ImageUrl = reader.IsDBNull("ImageUrl") ? null : reader.GetString("ImageUrl"),
            PublishedAt = reader.IsDBNull("PublishedAt") ? null : reader.GetDateTime("PublishedAt"),
            IsPublished = reader.GetBoolean("IsPublished"),
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
