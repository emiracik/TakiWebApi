using TakiWebApi.Models;

namespace TakiWebApi.Data;

public interface IFeedbackRepository
{
    Task<IEnumerable<Feedback>> GetAllAsync();
    Task<Feedback?> GetByIdAsync(int feedbackId);
    Task<IEnumerable<Feedback>> GetByUserIdAsync(int userId);
    Task<IEnumerable<Feedback>> GetByDriverIdAsync(int driverId);
    Task<IEnumerable<Feedback>> GetByTripIdAsync(int tripId);
    Task<int> CreateAsync(Feedback feedback);
    Task<bool> DeleteAsync(int feedbackId);
}
