using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TakiWebApi.Data;
using TakiWebApi.Models;
using TakiWebApi.Services;
using TakiWebApi.Security;

namespace TakiWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TripsController : ControllerBase
{
    private readonly ITripRepository _tripRepository;
    private readonly IJwtService _jwtService;
    private readonly IWalletRepository _walletRepository;
    private readonly IDriverEarningRepository _driverEarningRepository;
    private readonly IPushNotificationService _push;
    private readonly IRealtimeNotifier _realtime;

    // --- Gelir modeli anahtarı ---
    // true  = ücretin TAMAMI bize kalır (sürücü hakedişi 0 yazılır — sabit
    //         maaşlı / şirket filosu modeli).
    // false = biz aracılık ücretimizi (komisyon) alırız, kalanı sürücüye
    //         hakediş olarak yazarız (klasik Uber/Bolt modeli).
    // Değiştirip yeniden deploy etmek yeterli — DB şeması gerekmiyor.
    public const bool PlatformTakesFullFare = false;
    public const decimal CommissionRate = 0.20m; // %20 aracılık ücreti

    public TripsController(ITripRepository tripRepository, IJwtService jwtService,
        IWalletRepository walletRepository, IDriverEarningRepository driverEarningRepository,
        IPushNotificationService push, IRealtimeNotifier realtime)
    {
        _push = push;
        _realtime = realtime;
        _tripRepository = tripRepository;
        _jwtService = jwtService;
        _walletRepository = walletRepository;
        _driverEarningRepository = driverEarningRepository;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<Trip>>> GetAllTrips()
    {
        var trips = await _tripRepository.GetAllTripsAsync();
        return Ok(trips);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Trip>> GetTrip(int id)
    {
        var trip = await _tripRepository.GetTripByIdAsync(id);
        
        if (trip == null)
        {
            return NotFound();
        }
        if (!User.IsPartyOf(trip.PassengerID, trip.DriverID))
        {
            return Forbid();
        }

        return Ok(trip);
    }

    [HttpGet("passenger/{passengerId}")]
    public async Task<ActionResult<IEnumerable<Trip>>> GetTripsByPassenger(int passengerId)
    {
        var trips = await _tripRepository.GetTripsByPassengerIdAsync(passengerId);
        return Ok(trips);
    }

    [HttpGet("driver/{driverId}")]
    public async Task<ActionResult<IEnumerable<Trip>>> GetTripsByDriver(int driverId)
    {
        var trips = await _tripRepository.GetTripsByDriverIdAsync(driverId);
        return Ok(trips);
    }

    [HttpGet("active")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<Trip>>> GetActiveTrips()
    {
        var trips = await _tripRepository.GetActiveTripsAsync();
        return Ok(trips);
    }

    [HttpGet("paginated")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<object>> GetTripsPaginated([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        if (pageNumber < 1 || pageSize < 1)
        {
            return BadRequest("Page number and page size must be greater than 0");
        }

        var trips = await _tripRepository.GetTripsPaginatedAsync(pageNumber, pageSize);
        var totalCount = await _tripRepository.GetTotalTripsCountAsync();
        
        var response = new
        {
            Trips = trips,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };

        return Ok(response);
    }

    [HttpGet("count")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<int>> GetTotalTripsCount()
    {
        var count = await _tripRepository.GetTotalTripsCountAsync();
        return Ok(count);
    }

    [HttpGet("active/count")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<int>> GetActiveTripsCount()
    {
        var count = await _tripRepository.GetActiveTripsCountAsync();
        return Ok(count);
    }

    [HttpGet("date-range")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<Trip>>> GetTripsByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        if (startDate > endDate)
        {
            return BadRequest("Start date cannot be greater than end date");
        }

        var trips = await _tripRepository.GetTripsByDateRangeAsync(startDate, endDate);
        return Ok(trips);
    }

    [HttpGet("payment-method/{paymentMethod}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<Trip>>> GetTripsByPaymentMethod(string paymentMethod)
    {
        var trips = await _tripRepository.GetTripsByPaymentMethodAsync(paymentMethod);
        return Ok(trips);
    }

    [HttpGet("cost/date-range")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<decimal>> GetTotalCostByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        if (startDate > endDate)
        {
            return BadRequest("Start date cannot be greater than end date");
        }

        var totalCost = await _tripRepository.GetTotalTripCostByDateRangeAsync(startDate, endDate);
        return Ok(totalCost);
    }

    [HttpGet("passenger/{passengerId}/total-cost")]
    public async Task<ActionResult<decimal>> GetTotalTripCostByPassengerId(int passengerId)
    {
        var totalCost = await _tripRepository.GetTotalTripCostByPassengerIdAsync(passengerId);
        return Ok(totalCost);
    }

    /// <summary>
    /// Sürücünün şu an devam etmekte olan (inProgress) yolculuğu — uygulama
    /// kapatılıp açıldığında aktif yolculuğu geri yüklemek için kullanılır.
    /// </summary>
    [HttpGet("driver/{driverId}/active")]
    public async Task<ActionResult<Trip>> GetActiveTripForDriver(int driverId)
    {
        var trip = await _tripRepository.GetActiveTripByDriverIdAsync(driverId);
        if (trip == null)
        {
            return NotFound();
        }
        if (trip.TripID is int id)
        {
            (trip.ArrivedAt, trip.PickedUpAt) = await _tripRepository.GetStagesAsync(id);
        }
        return Ok(trip);
    }

    /// <summary>
    /// Yolcunun devam eden (ya da onayını bekleyen) yolculuğu, aşama
    /// zamanlarıyla. Yolcu uygulaması aşamaları ve bitişi buradan izler;
    /// sürücü uç noktası yolcuya kapalıdır (yetki filtresi).
    /// </summary>
    [HttpGet("passenger/{passengerId}/active")]
    public async Task<ActionResult<Trip>> GetActiveTripForPassenger(int passengerId)
    {
        var trip = await _tripRepository.GetActiveTripByPassengerIdAsync(passengerId);
        if (trip == null)
        {
            return NotFound();
        }
        if (trip.TripID is int id)
        {
            (trip.ArrivedAt, trip.PickedUpAt) = await _tripRepository.GetStagesAsync(id);
        }
        return Ok(trip);
    }

    /// <summary>Sürücü alış noktasına vardı ("Kapıdayım"); yolcuya bildirim gider.</summary>
    [HttpPost("{tripId}/arrived")]
    public async Task<IActionResult> MarkArrived(int tripId)
    {
        if (await TripAccessAsync(tripId, t => User.IsDriver(t.DriverID)) is IActionResult denied)
        {
            return denied;
        }
        if (!await _tripRepository.MarkArrivedAsync(tripId))
        {
            return Conflict("Yolculuk bu aşamada değil.");
        }
        var trip = await _tripRepository.GetTripByIdAsync(tripId);
        if (trip?.PassengerID is int passengerId)
        {
            _push.Notify(passengerId, "user", "Sürücün kapıda",
                $"{trip.StartAddress} adresinde seni bekliyor.",
                new Dictionary<string, string> { ["type"] = "driver_arrived", ["tripId"] = tripId.ToString() });
        }
        return Ok(new { arrivedAt = DateTime.UtcNow });
    }

    /// <summary>
    /// Yolcu gelmedi: sürücü kapıya vardıktan en az NoShowWaitMinutes dakika
    /// sonra yolculuğu kapatır; yolcuya bekleme ücreti yansır, sürücü serbest kalır.
    /// </summary>
    [HttpPost("{tripId}/no-show")]
    public async Task<IActionResult> MarkNoShow(int tripId, [FromServices] IConfiguration config,
        [FromServices] IMatchingRepository matching)
    {
        if (await TripAccessAsync(tripId, t => User.IsDriver(t.DriverID)) is IActionResult denied)
        {
            return denied;
        }
        var waitMinutes = config.GetValue("Fares:NoShowWaitMinutes", 5);
        var fee = config.GetValue("Fares:NoShowFee", 50m);
        var (arrivedAt, pickedUpAt) = await _tripRepository.GetStagesAsync(tripId);
        if (arrivedAt == null || pickedUpAt != null)
        {
            return Conflict("Önce \"Kapıdayım\" demelisin; yolcu bindiyse bu işlem yapılamaz.");
        }
        var waited = DateTime.UtcNow - arrivedAt.Value;
        if (waited < TimeSpan.FromMinutes(waitMinutes))
        {
            var left = (int)Math.Ceiling((TimeSpan.FromMinutes(waitMinutes) - waited).TotalMinutes);
            return Conflict($"Yolcuyu en az {waitMinutes} dakika beklemelisin ({left} dk kaldı).");
        }
        if (!await _tripRepository.MarkNoShowAsync(tripId, fee))
        {
            return Conflict("Yolculuk bu aşamada değil.");
        }
        var trip = await _tripRepository.GetTripByIdAsync(tripId);
        if (trip?.DriverID is int driverId)
        {
            await matching.SetDriverAvailabilityAsync(driverId, true);
        }
        if (trip?.PassengerID is int passengerId)
        {
            _push.Notify(passengerId, "user", "Yolculuk iptal edildi",
                $"Sürücün {waitMinutes} dakikadan uzun bekledi. ₺{fee:0.##} bekleme ücreti uygulandı.",
                new Dictionary<string, string> { ["type"] = "no_show", ["tripId"] = tripId.ToString() });
        }
        return Ok(new { noShowFee = fee });
    }

    /// <summary>Yolcu araca bindi; yolculuk süresi buradan başlar.</summary>
    [HttpPost("{tripId}/picked-up")]
    public async Task<IActionResult> MarkPickedUp(int tripId)
    {
        if (await TripAccessAsync(tripId, t => User.IsDriver(t.DriverID)) is IActionResult denied)
        {
            return denied;
        }
        if (!await _tripRepository.MarkPickedUpAsync(tripId))
        {
            return Conflict("Yolculuk bu aşamada değil.");
        }
        await NotifyOtherPartyAsync(tripId, "picked_up");
        return Ok(new { pickedUpAt = DateTime.UtcNow });
    }

    /// <summary>
    /// Yolcunun seçtiği ödeme yöntemini yolculuğa yazar ("wallet" | "card" | "cash").
    /// Yolculuk tamamlandığında ücret buna göre tahsil edilir (nakitte sürücüye elden ödenir).
    /// </summary>
    [HttpPut("{tripId}/payment-method")]
    public async Task<IActionResult> SetPaymentMethod(int tripId, [FromBody] string paymentMethod)
    {
        var method = (paymentMethod ?? "").Trim().ToLowerInvariant();
        if (method != "wallet" && method != "card" && method != "cash")
        {
            return BadRequest("paymentMethod 'wallet', 'card' veya 'cash' olmalı.");
        }
        // Ödeme yöntemini sadece yolculuğun yolcusu seçebilir.
        if (await TripAccessAsync(tripId, t => User.IsPassenger(t.PassengerID)) is IActionResult denied)
        {
            return denied;
        }
        var ok = await _tripRepository.SetPaymentMethodAsync(tripId, method);
        if (ok) await NotifyOtherPartyAsync(tripId, "payment_method");
        return ok ? Ok(new { message = "Ödeme yöntemi kaydedildi.", paymentMethod = method })
                  : NotFound("Yolculuk bulunamadı.");
    }

    /// <summary>
    /// Yolcu, tamamlanmış yolculuğa bir kez bahşiş bırakır (₺1–₺1000).
    /// Nakitte sürücüye elden verilir; kart/cüzdan tahsilatı ödeme
    /// entegrasyonuyla birlikte yapılacak, şimdilik tutar kaydedilir.
    /// </summary>
    [HttpPost("{tripId}/tip")]
    public async Task<IActionResult> AddTip(int tripId, [FromBody] decimal amount)
    {
        if (amount < 1 || amount > 1000)
        {
            return BadRequest("Bahşiş ₺1 ile ₺1000 arasında olmalı.");
        }
        if (await TripAccessAsync(tripId, t => User.IsPassenger(t.PassengerID)) is IActionResult denied)
        {
            return denied;
        }
        if (!await _tripRepository.SetTipAsync(tripId, amount))
        {
            return Conflict("Bu yolculuğa bahşiş verilemez (tamamlanmamış ya da zaten verilmiş).");
        }
        var trip = await _tripRepository.GetTripByIdAsync(tripId);
        if (trip?.DriverID is int driverId)
        {
            _push.Notify(driverId, "driver", "Bahşiş aldın",
                $"Yolcu ₺{amount:0.##} bahşiş bıraktı. Teşekkürler!",
                new Dictionary<string, string> { ["type"] = "tip", ["tripId"] = tripId.ToString() });
        }
        return Ok(new { message = "Bahşiş kaydedildi.", tipAmount = amount });
    }

    /// <summary>
    /// Sürücü "Yolculuğu Bitir" dediğinde çağrılır. Yolculuk
    /// 'awaitingConfirmation' durumuna geçer (EndTime + nihai ücret yazılır)
    /// ama ÜCRET TAHSİL EDİLMEZ — yolcunun onayı beklenir.
    /// </summary>
    [HttpPost("{tripId}/driver-finish")]
    public async Task<IActionResult> DriverFinishTrip(int tripId, [FromBody] decimal? finalCost)
    {
        if (await TripAccessAsync(tripId, t => User.IsDriver(t.DriverID)) is IActionResult denied)
        {
            return denied;
        }
        var ok = await _tripRepository.DriverFinishTripAsync(tripId, finalCost);
        if (!ok)
        {
            return NotFound("Devam eden yolculuk bulunamadı.");
        }
        var trip = await _tripRepository.GetTripByIdAsync(tripId);
        if (trip?.PassengerID is int passengerId)
        {
            _push.Notify(passengerId, "user", "Yolculuğunuz tamamlandı",
                $"Ücret: ₺{trip.Cost ?? finalCost ?? 0m:0.00}. Lütfen onaylayın.",
                new Dictionary<string, string> { ["type"] = "trip_finished", ["tripId"] = tripId.ToString() });
        }
        return Ok(new
        {
            message = "Yolcu onayı bekleniyor.",
            awaitingConfirmation = true,
            fare = trip?.Cost ?? finalCost ?? 0m,
            paymentMethod = (trip?.PaymentMethod ?? "card").ToLowerInvariant()
        });
    }

    /// <summary>
    /// Yolcu, sürücünün bitirdiği yolculuğu onaylar. Yolculuk 'completed'
    /// olur ve ödeme yöntemi "wallet" ise ücret cüzdandan tahsil edilir.
    /// </summary>
    [HttpPost("{tripId}/confirm")]
    public async Task<IActionResult> ConfirmTrip(int tripId)
    {
        if (await TripAccessAsync(tripId, t => User.IsPassenger(t.PassengerID)) is IActionResult denied)
        {
            return denied;
        }
        var ok = await _tripRepository.CompleteTripAsync(tripId, null);
        if (!ok)
        {
            return NotFound("Yolculuk bulunamadı.");
        }
        var result = await SettleTripAsync(tripId, null);
        await NotifyOtherPartyAsync(tripId, "trip_confirmed");
        return Ok(result);
    }

    /// <summary>
    /// Devam eden bir yolculuğu doğrudan tamamlandı olarak işaretler ve
    /// ücreti tahsil eder (yolcu onayı olmadan — yedek/eski akış).
    /// </summary>
    [HttpPost("{tripId}/complete")]
    public async Task<IActionResult> CompleteTrip(int tripId, [FromBody] decimal? finalCost)
    {
        if (await TripAccessAsync(tripId, t => User.IsDriver(t.DriverID)) is IActionResult denied)
        {
            return denied;
        }
        var ok = await _tripRepository.CompleteTripAsync(tripId, finalCost);
        if (!ok)
        {
            return NotFound("Yolculuk bulunamadı.");
        }
        var result = await SettleTripAsync(tripId, finalCost);
        return Ok(result);
    }

    // Yolculuğun diğer tarafına (sürücü çağırdıysa yolcuya, yolcu çağırdıysa
    // sürücüye) anlık güncelleme gönderir; bildirim göstermez.
    private async Task NotifyOtherPartyAsync(int tripId, string type)
    {
        var trip = await _tripRepository.GetTripByIdAsync(tripId);
        if (trip == null) return;
        var data = new Dictionary<string, string> { ["tripId"] = tripId.ToString() };
        if (User.IsDriver(trip.DriverID) && trip.PassengerID is int passengerId)
            _realtime.Send(passengerId, "user", type, data);
        else if (trip.DriverID is int driverId)
            _realtime.Send(driverId, "driver", type, data);
    }

    // Yolculuk yoksa 404, çağıran yetkili değilse 403 döner; izin varsa null.
    private async Task<IActionResult?> TripAccessAsync(int tripId, Func<Trip, bool> allowed)
    {
        var trip = await _tripRepository.GetTripByIdAsync(tripId);
        if (trip == null)
        {
            return NotFound("Yolculuk bulunamadı.");
        }
        return User.IsAdmin() || allowed(trip) ? null : Forbid();
    }

    // Ücret tahsilatı — hem confirm hem complete tarafından kullanılır.
    private async Task<object> SettleTripAsync(int tripId, decimal? finalCost)
    {
        var trip = await _tripRepository.GetTripByIdAsync(tripId);
        var fare = trip?.Cost ?? finalCost ?? 0m;
        var method = (trip?.PaymentMethod ?? "card").ToLowerInvariant();
        bool walletCharged = false;
        bool walletShort = false;

        if (method == "wallet" && trip?.PassengerID is int passengerId && fare > 0)
        {
            var wallet = await _walletRepository.GetWalletByUserIdAsync(passengerId)
                         ?? await _walletRepository.CreateWalletAsync(passengerId);
            if (wallet.Balance >= fare)
            {
                await _walletRepository.AddTransactionAsync(new WalletTransaction
                {
                    WalletID = wallet.WalletID,
                    UserID = passengerId,
                    TransactionType = "TripPayment",
                    Amount = -fare,
                    Description = $"Yolculuk ödemesi (#{tripId})",
                    TransactionDate = DateTime.Now
                });
                await _walletRepository.UpdateWalletBalanceAsync(
                    wallet.WalletID, wallet.Balance - fare, wallet.TotalIn, wallet.TotalOut + fare);
                walletCharged = true;
            }
            else
            {
                walletShort = true;
            }
        }

        // Sürücü hakedişi — gelir modeline göre (PlatformTakesFullFare).
        decimal driverEarning = 0m;
        decimal platformCommission = fare;
        if (trip?.DriverID is int driverId && fare > 0
            && !await _driverEarningRepository.ExistsForTripAsync(tripId))
        {
            driverEarning = PlatformTakesFullFare ? 0m : Math.Round(fare * (1 - CommissionRate), 2);
            platformCommission = fare - driverEarning;
            await _driverEarningRepository.CreateAsync(new DriverEarning
            {
                DriverID = driverId,
                TripID = tripId,
                Amount = driverEarning,
                EarnedAt = DateTime.UtcNow,
            });
        }

        return new
        {
            message = "Yolculuk tamamlandı.",
            fare,
            paymentMethod = method,
            walletCharged,
            walletShort,
            platformTakesFullFare = PlatformTakesFullFare,
            driverEarning,
            platformCommission
        };
    }
}
