using TakiWebApi.Models;

namespace TakiWebApi.Data;

public interface INotificationRepository
{
    Task<IEnumerable<Notification>> GetAllNotificationsAsync();
    Task<Notification?> GetNotificationByIdAsync(int notificationId);
    Task<IEnumerable<Notification>> GetNotificationsByUserIdAsync(int userId);
    Task<IEnumerable<Notification>> GetUnreadNotificationsByUserIdAsync(int userId);
    Task<IEnumerable<Notification>> GetActiveNotificationsAsync();
    Task<IEnumerable<Notification>> GetNotificationsPaginatedAsync(int pageNumber, int pageSize);
    Task<int> GetTotalNotificationsCountAsync();
    Task<int> GetActiveNotificationsCountAsync();
    Task<int> GetUnreadNotificationsCountByUserIdAsync(int userId);
    Task<IEnumerable<Notification>> SearchNotificationsByTitleAsync(string searchTerm);
    Task<IEnumerable<Notification>> GetNotificationsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<bool> MarkNotificationAsReadAsync(int notificationId);
    Task<bool> MarkAllNotificationsAsReadByUserIdAsync(int userId);
    Task<int> CreateNotificationAsync(Notification notification);
    Task<bool> UpdateNotificationAsync(Notification notification);
    Task<bool> DeleteNotificationAsync(int notificationId);
}
