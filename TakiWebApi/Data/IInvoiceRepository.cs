using TakiWebApi.Models;

namespace TakiWebApi.Data;

public interface IInvoiceRepository
{
    Task<IEnumerable<Invoice>> GetAllAsync();
    Task<Invoice?> GetByIdAsync(int invoiceId);
    Task<IEnumerable<Invoice>> GetByUserIdAsync(int userId);
    Task<IEnumerable<Invoice>> GetByTripIdAsync(int tripId);
    Task<int> CreateAsync(Invoice invoice);
    Task<bool> UpdateAsync(Invoice invoice);
    Task<bool> DeleteAsync(int invoiceId);
}
