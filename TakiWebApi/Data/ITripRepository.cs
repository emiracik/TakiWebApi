using TakiWebApi.Models;

namespace TakiWebApi.Data;

public interface ITripRepository
{
    Task<IEnumerable<Trip>> GetAllTripsAsync();
    Task<Trip?> GetTripByIdAsync(int tripId);
    Task<IEnumerable<Trip>> GetTripsByPassengerIdAsync(int passengerId);
    Task<IEnumerable<Trip>> GetTripsByDriverIdAsync(int driverId);
    Task<IEnumerable<Trip>> GetActiveTripsAsync();
    Task<IEnumerable<Trip>> GetTripsPaginatedAsync(int pageNumber, int pageSize);
    Task<int> GetTotalTripsCountAsync();
    Task<int> GetActiveTripsCountAsync();
    Task<IEnumerable<Trip>> GetTripsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<Trip>> GetTripsByPaymentMethodAsync(string paymentMethod);
    Task<decimal> GetTotalTripCostByDateRangeAsync(DateTime startDate, DateTime endDate);
}
