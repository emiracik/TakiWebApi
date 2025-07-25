using TakiWebApi.Models;

namespace TakiWebApi.Data;

public interface IFAQRepository
{
    Task<IEnumerable<FAQ>> GetAllFAQsAsync();
    Task<FAQ?> GetFAQByIdAsync(int faqId);
    Task<IEnumerable<FAQ>> GetActiveFAQsAsync();
    Task<IEnumerable<FAQ>> GetFAQsPaginatedAsync(int pageNumber, int pageSize);
    Task<int> GetTotalFAQsCountAsync();
    Task<int> GetActiveFAQsCountAsync();
    Task<IEnumerable<FAQ>> SearchFAQsByQuestionAsync(string searchTerm);
}
