using TakiWebApi.Models;

namespace TakiWebApi.Data;

public interface IMessageRepository
{
    Task<IEnumerable<Message>> GetAllAsync();
    Task<Message?> GetByIdAsync(int messageId);
    Task<IEnumerable<Message>> GetByUserIdAsync(int userId);
    Task<int> CreateAsync(Message message);
    Task<bool> MarkAsReadAsync(int messageId);
    Task<bool> DeleteAsync(int messageId);
}
