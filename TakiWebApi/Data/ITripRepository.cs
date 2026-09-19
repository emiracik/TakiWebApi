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
    Task<decimal> GetTotalTripCostByPassengerIdAsync(int passengerId);

    // Eşleştirme kabul edildiğinde gerçek bir yolculuk kaydı oluşturmak,
    // bitirildiğinde tamamlanmış olarak işaretlemek ve uygulama yeniden
    // açıldığında devam eden yolculuğu bulabilmek için.
    Task<Trip> CreateTripAsync(Trip trip);
    Task<bool> CompleteTripAsync(int tripId, decimal? finalCost);
    // Sürücü "bitir" dediğinde: yolculuk 'awaitingConfirmation' durumuna
    // geçer (EndTime + nihai ücret yazılır) ama ücret tahsil edilmez —
    // yolcu onayını bekler.
    Task<bool> DriverFinishTripAsync(int tripId, decimal? finalCost);
    Task<Trip?> GetActiveTripByDriverIdAsync(int driverId);

    // Yolcunun seçtiği ödeme yöntemini ("wallet" | "card") yolculuğa yazar;
    // yolculuk tamamlandığında ücret buna göre tahsil edilir.
    Task<bool> SetPaymentMethodAsync(int tripId, string paymentMethod);
}
