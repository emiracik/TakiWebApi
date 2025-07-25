using TakiWebApi.Models;

namespace TakiWebApi.Data;

public interface IAnnouncementRepository
{
    Task<IEnumerable<Announcement>> GetAllAnnouncementsAsync();
    Task<Announcement?> GetAnnouncementByIdAsync(int announcementId);
    Task<IEnumerable<Announcement>> GetActiveAnnouncementsAsync();
    Task<IEnumerable<Announcement>> GetAnnouncementsPaginatedAsync(int pageNumber, int pageSize);
    Task<int> GetTotalAnnouncementsCountAsync();
    Task<int> GetActiveAnnouncementsCountAsync();
    Task<IEnumerable<Announcement>> SearchAnnouncementsByTitleAsync(string searchTerm);
    Task<IEnumerable<Announcement>> GetAnnouncementsByDateRangeAsync(DateTime startDate, DateTime endDate);
}
