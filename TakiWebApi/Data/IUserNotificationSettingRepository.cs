using TakiWebApi.Models;

namespace TakiWebApi.Data;

public interface IUserNotificationSettingRepository
{
    Task<IEnumerable<UserNotificationSetting>> GetAllUserNotificationSettingsAsync();
    Task<UserNotificationSetting?> GetUserNotificationSettingByIdAsync(int settingId);
    Task<UserNotificationSetting?> GetUserNotificationSettingByUserIdAsync(int userId);
    Task<IEnumerable<UserNotificationSetting>> GetUserNotificationSettingsPaginatedAsync(int pageNumber, int pageSize);
    Task<int> GetTotalUserNotificationSettingsCountAsync();
    Task<int> CreateUserNotificationSettingAsync(UserNotificationSetting userNotificationSetting);
    Task<bool> UpdateUserNotificationSettingAsync(UserNotificationSetting userNotificationSetting);
    Task<bool> DeleteUserNotificationSettingAsync(int settingId);
}
