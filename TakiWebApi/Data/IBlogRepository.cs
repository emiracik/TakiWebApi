using TakiWebApi.Models;

namespace TakiWebApi.Data;

public interface IBlogRepository
{
    Task<IEnumerable<Blog>> GetAllBlogsAsync();
    Task<Blog?> GetBlogByIdAsync(int blogId);
    Task<IEnumerable<Blog>> GetPublishedBlogsAsync();
    Task<IEnumerable<Blog>> GetActiveBlogsAsync();
    Task<IEnumerable<Blog>> GetBlogsPaginatedAsync(int pageNumber, int pageSize);
    Task<int> GetTotalBlogsCountAsync();
    Task<int> GetPublishedBlogsCountAsync();
    Task<IEnumerable<Blog>> SearchBlogsByTitleAsync(string searchTerm);
    Task<IEnumerable<Blog>> GetBlogsByDateRangeAsync(DateTime startDate, DateTime endDate);
}
