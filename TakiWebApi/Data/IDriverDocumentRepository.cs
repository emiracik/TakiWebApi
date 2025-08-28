using TakiWebApi.Models;

namespace TakiWebApi.Data;

public interface IDriverDocumentRepository
{
    Task<IEnumerable<DriverDocument>> GetAllAsync();
    Task<DriverDocument?> GetByIdAsync(int documentId);
    Task<IEnumerable<DriverDocument>> GetByDriverIdAsync(int driverId);
    Task<int> CreateAsync(DriverDocument document);
    Task<bool> DeleteAsync(int documentId);
}
