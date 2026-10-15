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

    // Yolcunun seçtiği ödeme yöntemini ("wallet" | "card" | "cash") yolculuğa yazar;
    // yolculuk tamamlandığında ücret buna göre tahsil edilir.
    Task<bool> SetPaymentMethodAsync(int tripId, string paymentMethod);

    // Yolcu, sürücü kabul ettikten sonra iptal etti (yalnızca devam eden yolculuk).
    Task<bool> CancelInProgressTripAsync(int tripId);

    // Tamamlanmış yolculuğa bir kez bahşiş yazar (Migrations/003: Trips.TipAmount).
    Task<bool> SetTipAsync(int tripId, decimal amount);

    // Yolculuk aşamaları (Migrations/005). Sütunlar yoksa null döner.
    Task<(DateTime? ArrivedAt, DateTime? PickedUpAt)> GetStagesAsync(int tripId);
    Task<bool> MarkArrivedAsync(int tripId);
    Task<bool> MarkPickedUpAsync(int tripId);

    // Yolcu gelmedi: yolculuk iptal, ücret yerine bekleme ücreti yazılır.
    Task<bool> MarkNoShowAsync(int tripId, decimal fee);
}
