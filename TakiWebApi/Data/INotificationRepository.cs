using TakiWebApi.Models;

namespace TakiWebApi.Data;

public interface INotificationRepository
{
    Task<IEnumerable<Notification>> GetAllNotificationsAsync();
    Task<Notification?> GetNotificationByIdAsync(int notificationId);
    Task<IEnumerable<Notification>> GetNotificationsByUserIdAsync(int userId);
    Task<IEnumerable<Notification>> GetNotificationsByDriverIdAsync(int driverId);
    Task<IEnumerable<Notification>> GetUnreadNotificationsAsync();
    Task<IEnumerable<Notification>> GetNotificationsPaginatedAsync(int pageNumber, int pageSize);
    Task<int> GetTotalNotificationsCountAsync();
    Task<int> GetUnreadNotificationsCountAsync();
    Task<IEnumerable<Notification>> SearchNotificationsByTitleAsync(string searchTerm);
    Task<IEnumerable<Notification>> GetNotificationsByCreatedDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<int> CreateNotificationAsync(Notification notification);
    Task<bool> UpdateNotificationAsync(Notification notification);
    Task<bool> MarkAsReadAsync(int notificationId);
    Task<bool> DeleteNotificationAsync(int notificationId);
}
