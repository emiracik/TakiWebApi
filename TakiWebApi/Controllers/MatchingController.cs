using TakiWebApi.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TakiWebApi.Data;
using TakiWebApi.Services;
using TakiWebApi.Models;
using System.ComponentModel.DataAnnotations;

namespace TakiWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MatchingController : ControllerBase
{
    private readonly IMatchingRepository _matchingRepository;
    private readonly ITripRepository _tripRepository;
    private readonly ILogger<MatchingController> _logger;
    private readonly IPushNotificationService _push;
    private readonly IPromotionRepository _promotions;
    private readonly IConfiguration _config;
    private readonly IDriverRepository _drivers;

    public MatchingController(IMatchingRepository matchingRepository, ITripRepository tripRepository, ILogger<MatchingController> logger,
        IPushNotificationService push, IPromotionRepository promotions, IConfiguration config, IDriverRepository drivers)
    {
        _drivers = drivers;
        _matchingRepository = matchingRepository;
        _tripRepository = tripRepository;
        _logger = logger;
        _push = push;
        _promotions = promotions;
        _config = config;
    }

    // Onaylanmamış (belgeleri incelenmemiş) sürücü talep alamaz (Migrations/004).
    private async Task<ActionResult?> RequireApprovedDriverAsync(int driverId)
    {
        var (status, note) = await _drivers.GetApprovalAsync(driverId);
        if (status == "Approved") return null;
        return StatusCode(403, new
        {
            code = "driver_not_approved",
            approvalStatus = status,
            message = status == "Rejected"
                ? $"Başvurun onaylanmadı{(string.IsNullOrWhiteSpace(note) ? "." : ": " + note)}"
                : "Hesabın inceleniyor. Belgelerin onaylanınca yolculuk almaya başlayabilirsin."
        });
    }

    // Sürücü kabul ettikten bu kadar dakika sonra yolcu iptal ederse iptal ücreti uygulanır.
    private int CancellationGraceMinutes => _config.GetValue("Fares:CancellationGraceMinutes", 3);
    private decimal CancellationFee => _config.GetValue("Fares:CancellationFee", 50m);

    #region Nearby Drivers and Driver Discovery

    /// <summary>
    /// Yakındaki müsait sürücüleri bulur
    /// </summary>
    [HttpGet("nearby-drivers")]
    public async Task<ActionResult<IEnumerable<NearbyDriverResponse>>> GetNearbyDrivers(
        [FromQuery, Required] double latitude,
        [FromQuery, Required] double longitude,
        [FromQuery, Range(0.1, 50.0)] double radius = 5.0,
        [FromQuery, Range(1, 100)] int maxResults = 10,
        [FromQuery, Range(0.0, 5.0)] double? minRating = null)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation($"Searching for nearby drivers at {latitude}, {longitude} within {radius}km");

            var nearbyDrivers = await _matchingRepository.FindNearbyDriversAsync(
                latitude, longitude, radius, maxResults, minRating);

            return Ok(nearbyDrivers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding nearby drivers");
            return StatusCode(500, "Yakındaki sürücüler aranırken bir hata oluştu.");
        }
    }

    /// <summary>
    /// Yakındaki müsait sürücüleri POST ile bulur (daha detaylı filtreler için)
    /// </summary>
    [HttpPost("find-nearby-drivers")]
    public async Task<ActionResult<IEnumerable<NearbyDriverResponse>>> FindNearbyDrivers([FromBody] FindNearbyDriversRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var nearbyDrivers = await _matchingRepository.FindNearbyDriversAsync(
                request.Latitude, request.Longitude, request.Radius, request.MaxResults, request.MinRating);

            return Ok(nearbyDrivers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding nearby drivers");
            return StatusCode(500, "Yakındaki sürücüler aranırken bir hata oluştu.");
        }
    }

    #endregion

    #region Smart Matching and Auto-Matching

    /// <summary>
    /// Otomatik en iyi eşleştirmeyi yapar
    /// </summary>
    [HttpPost("find-best-match")]
    public async Task<ActionResult<MatchingResponse>> FindBestMatch([FromBody] CreateMatchingRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (!User.IsPassenger(request.PassengerID))
            {
                return Forbid();
            }

            _logger.LogInformation($"Finding best match for passenger {request.PassengerID}");

            if (!string.IsNullOrWhiteSpace(request.PromoCode))
            {
                var promo = await _promotions.GetValidByCodeAsync(request.PromoCode);
                if (promo == null)
                {
                    return BadRequest("Promosyon kodu geçersiz veya süresi dolmuş.");
                }
                if (await _promotions.HasUserUsedAsync(promo.PromotionID, request.PassengerID))
                {
                    return BadRequest("Bu promosyon kodunu daha önce kullandın.");
                }
                request.AppliedPromotion = promo;
            }

            var matchingResult = await _matchingRepository.FindBestMatchAsync(request);
            
            if (matchingResult == null)
            {
                return NotFound("Uygun sürücü bulunamadı.");
            }

            if (request.AppliedPromotion is Promotion applied && matchingResult.Discount is decimal discount)
            {
                await _promotions.RecordUsageAsync(applied.PromotionID, request.PassengerID,
                    matchingResult.MatchingRequestID, discount);
            }

            if (matchingResult.DriverID is int bestDriverId)
            {
                _push.Notify(bestDriverId, "driver", "Yeni yolculuk talebi",
                    $"{request.PickupAddress} → {request.DropoffAddress}",
                    new Dictionary<string, string>
                    {
                        ["type"] = "new_request",
                        ["requestId"] = matchingResult.MatchingRequestID.ToString()
                    });
            }

            return Ok(matchingResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in auto matching");
            return StatusCode(500, "Otomatik eşleştirme sırasında bir hata oluştu.");
        }
    }

    /// <summary>
    /// Manuel eşleştirme talebi oluşturur
    /// </summary>
    [HttpPost("create-request")]
    public async Task<ActionResult<MatchingResponse>> CreateMatchingRequest([FromBody] CreateMatchingRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!User.IsPassenger(request.PassengerID))
            {
                return Forbid();
            }

            // Calculate trip details
            var tripDistance = await _matchingRepository.CalculateDistanceAsync(
                request.PickupLatitude, request.PickupLongitude,
                request.DropoffLatitude, request.DropoffLongitude);

            var estimatedDuration = await _matchingRepository.EstimateArrivalTimeAsync(tripDistance);
            var calculatedCost = await _matchingRepository.CalculateEstimatedCostAsync(tripDistance, estimatedDuration, request.VehicleType);
            var isCustomOffer = request.OfferedFare.HasValue && request.OfferedFare.Value > 0;
            var estimatedCost = isCustomOffer ? request.OfferedFare!.Value : calculatedCost;

            var matchingRequest = new MatchingRequest
            {
                PassengerID = request.PassengerID,
                PickupAddress = request.PickupAddress,
                DropoffAddress = request.DropoffAddress,
                PickupLatitude = request.PickupLatitude,
                PickupLongitude = request.PickupLongitude,
                DropoffLatitude = request.DropoffLatitude,
                DropoffLongitude = request.DropoffLongitude,
                EstimatedCost = estimatedCost,
                EstimatedDistance = tripDistance,
                EstimatedDuration = estimatedDuration,
                MaxWaitTime = request.MaxWaitTime,
                MinRating = request.MinRating,
                MaxDistance = request.MaxDistance,
                Notes = request.Notes,
                Status = MatchingStatus.Pending,
                ExpiryTime = DateTime.UtcNow.AddMinutes(request.MaxWaitTime ?? 15)
            };

            var createdRequest = await _matchingRepository.CreateMatchingRequestAsync(matchingRequest);

            var response = new MatchingResponse
            {
                MatchingRequestID = createdRequest.MatchingRequestID,
                Status = MatchingStatus.Pending,
                EstimatedCost = estimatedCost,
                Distance = tripDistance,
                RequestTime = createdRequest.RequestTime,
                Message = "Eşleştirme talebi başarıyla oluşturuldu."
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating matching request");
            return StatusCode(500, "Eşleştirme talebi oluşturulurken bir hata oluştu.");
        }
    }

    #endregion

    #region Matching Request Management

    /// <summary>
    /// Eşleştirme talebi durumunu getirir
    /// </summary>
    [HttpGet("request/{requestId}/status")]
    public async Task<ActionResult<MatchingResponse>> GetMatchingRequestStatus(int requestId)
    {
        try
        {
            var request = await _matchingRepository.GetMatchingRequestByIdAsync(requestId);
            
            if (request == null)
            {
                return NotFound("Eşleştirme talebi bulunamadı.");
            }
            // Talebin yolcusu, atanan sürücü veya talebi kabul etmeden önce
            // durumunu kontrol eden herhangi bir sürücü görebilir.
            if (!User.IsPartyOf(request.PassengerID, request.DriverID) && !User.IsAnyDriver())
            {
                return Forbid();
            }

            var response = new MatchingResponse
            {
                MatchingRequestID = request.MatchingRequestID,
                Status = request.Status,
                DriverID = request.DriverID,
                EstimatedCost = request.EstimatedCost,
                Distance = request.EstimatedDistance,
                RequestTime = request.RequestTime,
                AcceptedTime = request.AcceptedTime,
                Message = GetStatusMessage(request.Status)
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting matching request status for ID {requestId}");
            return StatusCode(500, "Talep durumu alınırken bir hata oluştu.");
        }
    }

    /// <summary>
    /// Yolcunun tüm eşleştirme taleplerini getirir
    /// </summary>
    [HttpGet("passenger/{passengerId}/requests")]
    public async Task<ActionResult<IEnumerable<MatchingRequest>>> GetPassengerMatchingRequests(int passengerId)
    {
        try
        {
            var requests = await _matchingRepository.GetMatchingRequestsByPassengerAsync(passengerId);
            return Ok(requests);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting matching requests for passenger {passengerId}");
            return StatusCode(500, "Yolcu talepleri alınırken bir hata oluştu.");
        }
    }

    /// <summary>
    /// Eşleştirme talebini iptal eder
    /// </summary>
    [HttpDelete("request/{requestId}")]
    public async Task<ActionResult<MatchingResponse>> CancelMatchingRequest(int requestId, [FromQuery] string? reason = null)
    {
        try
        {
            if (await RequestAccessAsync(requestId, r => User.IsPartyOf(r.PassengerID, r.DriverID)) is ActionResult denied)
            {
                return denied;
            }
            var before = await _matchingRepository.GetMatchingRequestByIdAsync(requestId);
            var response = await _matchingRepository.CancelMatchingRequestAsync(requestId);
            
            if (response.Status == MatchingStatus.Cancelled)
            {
                decimal? fee = null;
                if (before is { Status: MatchingStatus.Accepted, DriverID: int driverId })
                {
                    // Sürücü yola çıkmıştı: yolculuğu kapat, sürücüyü serbest bırak, haber ver.
                    var trip = await _tripRepository.GetActiveTripByDriverIdAsync(driverId);
                    if (trip?.TripID is int tripId && trip.PassengerID == before.PassengerID)
                    {
                        await _tripRepository.CancelInProgressTripAsync(tripId);
                    }
                    await _matchingRepository.SetDriverAvailabilityAsync(driverId, true);

                    var byPassenger = User.IsPassenger(before.PassengerID);
                    if (byPassenger && before.AcceptedTime is DateTime accepted &&
                        DateTime.UtcNow - accepted > TimeSpan.FromMinutes(CancellationGraceMinutes))
                    {
                        fee = CancellationFee;
                    }
                    if (byPassenger)
                    {
                        _push.Notify(driverId, "driver", "Yolculuk iptal edildi",
                            "Yolcu yolculuğu iptal etti.",
                            new Dictionary<string, string> { ["type"] = "trip_cancelled", ["requestId"] = requestId.ToString() });
                    }
                }

                var cleanReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim()[..Math.Min(reason.Trim().Length, 200)];
                await _matchingRepository.SetCancellationInfoAsync(requestId, cleanReason, fee);
                response.CancellationFee = fee;
                if (fee is decimal f)
                {
                    response.Message = $"Talep iptal edildi. İptal ücreti: ₺{f:0.##}";
                }
                return Ok(response);
            }
            
            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error cancelling matching request {requestId}");
            return StatusCode(500, "Talep iptal edilirken bir hata oluştu.");
        }
    }

    #endregion

    #region Driver Operations

    /// <summary>
    /// Sürücünün konum bilgisini günceller
    /// </summary>
    [HttpPut("driver/{driverId}/location")]
    public async Task<IActionResult> UpdateDriverLocation(int driverId, [FromBody] UpdateDriverLocationRequest locationRequest)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (locationRequest.IsAvailable && await RequireApprovedDriverAsync(driverId) is not null)
            {
                locationRequest.IsAvailable = false; // konum paylaşabilir ama müsait görünmez
            }
            await _matchingRepository.UpdateDriverLocationAsync(driverId, locationRequest);
            
            return Ok(new { message = "Sürücü konumu başarıyla güncellendi." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating driver location for driver {driverId}");
            return StatusCode(500, "Sürücü konumu güncellenirken bir hata oluştu.");
        }
    }

    /// <summary>
    /// Sürücünün mevcut konum bilgisini getirir
    /// </summary>
    [HttpGet("driver/{driverId}/location")]
    [AllowAnyCaller]
    public async Task<ActionResult<DriverLocation>> GetDriverLocation(int driverId)
    {
        try
        {
            var location = await _matchingRepository.GetDriverLocationAsync(driverId);
            
            if (location == null)
            {
                return NotFound("Sürücü konum bilgisi bulunamadı.");
            }

            return Ok(location);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting driver location for driver {driverId}");
            return StatusCode(500, "Sürücü konumu alınırken bir hata oluştu.");
        }
    }

    /// <summary>
    /// Sürücünün müsaitlik durumunu ayarlar
    /// </summary>
    [HttpPut("driver/{driverId}/availability")]
    public async Task<IActionResult> SetDriverAvailability(int driverId, [FromBody] bool isAvailable)
    {
        try
        {
            if (isAvailable && await RequireApprovedDriverAsync(driverId) is ActionResult notApproved)
            {
                return notApproved;
            }
            await _matchingRepository.SetDriverAvailabilityAsync(driverId, isAvailable);
            
            var status = isAvailable ? "müsait" : "meşgul";
            return Ok(new { message = $"Sürücü durumu '{status}' olarak güncellendi." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error setting driver availability for driver {driverId}");
            return StatusCode(500, "Sürücü müsaitlik durumu güncellenirken bir hata oluştu.");
        }
    }

    /// <summary>
    /// Sürücünün bekleyen taleplerini getirir
    /// </summary>
    [HttpGet("driver/{driverId}/requests")]
    public async Task<ActionResult<IEnumerable<MatchingRequest>>> GetDriverMatchingRequests(int driverId)
    {
        try
        {
            var requests = await _matchingRepository.GetMatchingRequestsByDriverAsync(driverId);
            return Ok(requests);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting matching requests for driver {driverId}");
            return StatusCode(500, "Sürücü talepleri alınırken bir hata oluştu.");
        }
    }

    /// <summary>
    /// Sürücü eşleştirme talebini kabul eder
    /// </summary>
    [HttpPost("driver/{driverId}/accept/{requestId}")]
    public async Task<ActionResult<MatchingResponse>> AcceptMatchingRequest(int driverId, int requestId)
    {
        try
        {
            if (await RequireApprovedDriverAsync(driverId) is ActionResult notApproved)
            {
                return notApproved;
            }
            // Check if driver is available
            var isAvailable = await _matchingRepository.IsDriverAvailableAsync(driverId);
            if (!isAvailable)
            {
                return BadRequest("Sürücü şu anda müsait değil.");
            }

            var response = await _matchingRepository.AcceptMatchingRequestAsync(driverId, requestId);

            if (response.Status == MatchingStatus.Accepted)
            {
                await CreateTripForAcceptedRequestAsync(response, requestId, driverId);
                var accepted = await _matchingRepository.GetMatchingRequestByIdAsync(requestId);
                if (accepted != null)
                {
                    _push.Notify(accepted.PassengerID, "user", "Sürücünüz yola çıktı",
                        string.Join(" · ", new[] { response.DriverName, response.VehiclePlate }
                            .Where(s => !string.IsNullOrWhiteSpace(s))),
                        new Dictionary<string, string> { ["type"] = "request_accepted", ["requestId"] = requestId.ToString() });
                }
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error accepting matching request {requestId} by driver {driverId}");
            return StatusCode(500, "Talep kabul edilirken bir hata oluştu.");
        }
    }

    /// <summary>
    /// Sürücü, bekleyen bir talebe kabul yerine kendi fiyatını önerir
    /// (karşı teklif). Yolcu bu teklifi kabul/red edene kadar talep bu
    /// sürücüye "ayrılmış" olur.
    /// </summary>
    [HttpPost("driver/{driverId}/counter/{requestId}")]
    public async Task<ActionResult<MatchingResponse>> DriverCounterOffer(int driverId, int requestId, [FromBody] decimal amount)
    {
        try
        {
            if (amount <= 0)
            {
                return BadRequest("Geçerli bir tutar giriniz.");
            }
            if (await RequireApprovedDriverAsync(driverId) is ActionResult notApproved)
            {
                return notApproved;
            }
            var isAvailable = await _matchingRepository.IsDriverAvailableAsync(driverId);
            if (!isAvailable)
            {
                return BadRequest("Sürücü şu anda müsait değil.");
            }

            // Sürücünün absürt (çok düşük/çok yüksek) teklif vermesini engelle —
            // yolcunun istediği/hesaplanan ücretin %100 - %150 aralığıyla sınırla.
            var existingRequest = await _matchingRepository.GetMatchingRequestByIdAsync(requestId);
            if (existingRequest?.EstimatedCost is decimal baseFare && baseFare > 0)
            {
                var minAllowed = Math.Round(baseFare * 1.0m, 2);
                var maxAllowed = Math.Round(baseFare * 1.5m, 2);
                if (amount < minAllowed || amount > maxAllowed)
                {
                    return BadRequest($"Teklif ₺{minAllowed:0} ile ₺{maxAllowed:0} arasında olmalı.");
                }
            }

            var response = await _matchingRepository.DriverCounterOfferAsync(driverId, requestId, amount);
            if (existingRequest != null)
            {
                _push.Notify(existingRequest.PassengerID, "user", "Sürücüden karşı teklif",
                    $"Sürücü yolculuk için ₺{amount:0} teklif etti.",
                    new Dictionary<string, string> { ["type"] = "counter_offer", ["requestId"] = requestId.ToString() });
            }
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error making counter offer for request {requestId} by driver {driverId}");
            return StatusCode(500, "Teklif gönderilirken bir hata oluştu.");
        }
    }

    /// <summary>
    /// Yolcu, sürücünün karşı teklifini kabul eder — yolculuk başlar.
    /// </summary>
    [HttpPost("request/{requestId}/counter/accept")]
    public async Task<ActionResult<MatchingResponse>> AcceptCounterOffer(int requestId)
    {
        try
        {
            // Karşı teklife sadece talebin yolcusu yanıt verebilir.
            if (await RequestAccessAsync(requestId, r => User.IsPassenger(r.PassengerID)) is ActionResult denied)
            {
                return denied;
            }
            var countered = await _matchingRepository.GetMatchingRequestByIdAsync(requestId);
            var response = await _matchingRepository.RespondToCounterOfferAsync(requestId, true);
            if (countered?.DriverID is int offerDriverId && response.Status == MatchingStatus.Accepted)
            {
                _push.Notify(offerDriverId, "driver", "Teklifiniz kabul edildi", "Yolcuyu almak için yola çıkabilirsiniz.",
                    new Dictionary<string, string> { ["type"] = "counter_accepted", ["requestId"] = requestId.ToString() });
            }
            if (response.Status == MatchingStatus.Accepted && response.DriverID is int driverId)
            {
                await CreateTripForAcceptedRequestAsync(response, requestId, driverId);
            }
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error accepting counter offer for request {requestId}");
            return StatusCode(500, "Teklif kabul edilirken bir hata oluştu.");
        }
    }

    /// <summary>
    /// Yolcu, sürücünün karşı teklifini reddeder — görüşme/talep biter.
    /// </summary>
    [HttpPost("request/{requestId}/counter/reject")]
    public async Task<ActionResult<MatchingResponse>> RejectCounterOffer(int requestId)
    {
        try
        {
            // Karşı teklife sadece talebin yolcusu yanıt verebilir.
            if (await RequestAccessAsync(requestId, r => User.IsPassenger(r.PassengerID)) is ActionResult denied)
            {
                return denied;
            }
            var countered = await _matchingRepository.GetMatchingRequestByIdAsync(requestId);
            var response = await _matchingRepository.RespondToCounterOfferAsync(requestId, false);
            if (countered?.DriverID is int offerDriverId)
            {
                _push.Notify(offerDriverId, "driver", "Teklifiniz reddedildi", "Yolcu karşı teklifinizi kabul etmedi.",
                    new Dictionary<string, string> { ["type"] = "counter_rejected", ["requestId"] = requestId.ToString() });
            }
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error rejecting counter offer for request {requestId}");
            return StatusCode(500, "Teklif reddedilirken bir hata oluştu.");
        }
    }

    // Talep yoksa 404, çağıran yetkili değilse 403 döner; izin varsa null.
    private async Task<ActionResult?> RequestAccessAsync(int requestId, Func<MatchingRequest, bool> allowed)
    {
        var request = await _matchingRepository.GetMatchingRequestByIdAsync(requestId);
        if (request == null)
        {
            return NotFound("Eşleştirme talebi bulunamadı.");
        }
        return User.IsAdmin() || allowed(request) ? null : Forbid();
    }

    // Kabul edilen bir talep için gerçek Trip kaydını oluşturur ve sürücüyü
    // meşgul yapar — hem normal kabul hem karşı-teklif-kabul akışında
    // kullanılıyor.
    private async Task CreateTripForAcceptedRequestAsync(MatchingResponse response, int requestId, int driverId)
    {
        await _matchingRepository.SetDriverAvailabilityAsync(driverId, false);
        try
        {
            var matchingRequest = await _matchingRepository.GetMatchingRequestByIdAsync(requestId);
            if (matchingRequest != null)
            {
                var trip = await _tripRepository.CreateTripAsync(new Trip
                {
                    PassengerID = matchingRequest.PassengerID,
                    DriverID = driverId,
                    StartAddress = matchingRequest.PickupAddress,
                    EndAddress = matchingRequest.DropoffAddress,
                    StartLatitude = matchingRequest.PickupLatitude,
                    StartLongitude = matchingRequest.PickupLongitude,
                    EndLatitude = matchingRequest.DropoffLatitude,
                    EndLongitude = matchingRequest.DropoffLongitude,
                    StartTime = DateTime.UtcNow,
                    Cost = matchingRequest.EstimatedCost,
                    Distance = matchingRequest.EstimatedDistance,
                    Status = RideStatus.inProgress
                });
                response.TripID = trip.TripID;
            }
        }
        catch (Exception tripEx)
        {
            // Trip kaydı oluşturulamasa bile eşleştirme kabulü geçerli
            // kalsın — sadece logla.
            _logger.LogError(tripEx, $"Trip kaydı oluşturulamadı (requestId={requestId})");
        }
    }

    /// <summary>
    /// Sürücü eşleştirme talebini reddeder
    /// </summary>
    [HttpPost("driver/{driverId}/reject/{requestId}")]
    public async Task<ActionResult<MatchingResponse>> RejectMatchingRequest(int driverId, int requestId)
    {
        try
        {
            var response = await _matchingRepository.RejectMatchingRequestAsync(driverId, requestId);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error rejecting matching request {requestId} by driver {driverId}");
            return StatusCode(500, "Talep reddedilirken bir hata oluştu.");
        }
    }

    #endregion

    #region Analytics and Statistics

    /// <summary>
    /// Tüm bekleyen eşleştirme taleplerini getirir
    /// </summary>
    [HttpGet("pending-requests")]
    public async Task<ActionResult<IEnumerable<MatchingRequest>>> GetPendingMatchingRequests()
    {
        try
        {
            // Bekleyen talepler (yolcu adresleri dahil) sadece sürücülere açık.
            if (!User.IsAnyDriver() && !User.IsAdmin())
            {
                return Forbid();
            }
            var requests = await _matchingRepository.GetPendingMatchingRequestsAsync();
            return Ok(requests);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending matching requests");
            return StatusCode(500, "Bekleyen talepler alınırken bir hata oluştu.");
        }
    }

    /// <summary>
    /// Aktif eşleştirme talepleri sayısını getirir
    /// </summary>
    [HttpGet("statistics/active-requests")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<int>> GetActiveMatchingRequestsCount()
    {
        try
        {
            var count = await _matchingRepository.GetActiveMatchingRequestsCountAsync();
            return Ok(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active requests count");
            return StatusCode(500, "Aktif talep sayısı alınırken bir hata oluştu.");
        }
    }

    /// <summary>
    /// Müsait sürücü sayısını getirir
    /// </summary>
    [HttpGet("statistics/available-drivers")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<int>> GetAvailableDriversCount()
    {
        try
        {
            var count = await _matchingRepository.GetAvailableDriversCountAsync();
            return Ok(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available drivers count");
            return StatusCode(500, "Müsait sürücü sayısı alınırken bir hata oluştu.");
        }
    }

    /// <summary>
    /// Tüm sürücü konumlarını getirir (admin için)
    /// </summary>
    [HttpGet("all-driver-locations")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<IEnumerable<DriverLocation>>> GetAllDriverLocations()
    {
        try
        {
            var locations = await _matchingRepository.GetAllDriverLocationsAsync();
            return Ok(locations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all driver locations");
            return StatusCode(500, "Sürücü konumları alınırken bir hata oluştu.");
        }
    }

    /// <summary>
    /// Tarih aralığında eşleştirme taleplerini getirir
    /// </summary>
    [HttpGet("requests/date-range")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<MatchingRequest>>> GetMatchingRequestsByDateRange(
        [FromQuery] DateTime startDate, 
        [FromQuery] DateTime endDate)
    {
        try
        {
            if (startDate > endDate)
            {
                return BadRequest("Başlangıç tarihi bitiş tarihinden büyük olamaz.");
            }

            var requests = await _matchingRepository.GetMatchingRequestsByDateRangeAsync(startDate, endDate);
            return Ok(requests);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting matching requests by date range");
            return StatusCode(500, "Tarih aralığında talepler alınırken bir hata oluştu.");
        }
    }

    #endregion

    #region Utility Operations

    /// <summary>
    /// İki nokta arasındaki mesafeyi hesaplar
    /// </summary>
    [HttpGet("calculate-distance")]
    public async Task<ActionResult<double>> CalculateDistance(
        [FromQuery, Required] double lat1,
        [FromQuery, Required] double lng1,
        [FromQuery, Required] double lat2,
        [FromQuery, Required] double lng2)
    {
        try
        {
            var distance = await _matchingRepository.CalculateDistanceAsync(lat1, lng1, lat2, lng2);
            return Ok(Math.Round(distance, 2));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating distance");
            return StatusCode(500, "Mesafe hesaplanırken bir hata oluştu.");
        }
    }

    /// <summary>
    /// Tahmini varış süresini hesaplar
    /// </summary>
    [HttpGet("estimate-arrival")]
    public async Task<ActionResult<int>> EstimateArrivalTime(
        [FromQuery, Required] double distanceKm,
        [FromQuery] double avgSpeed = 30.0)
    {
        try
        {
            var estimatedTime = await _matchingRepository.EstimateArrivalTimeAsync(distanceKm, avgSpeed);
            return Ok(estimatedTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error estimating arrival time");
            return StatusCode(500, "Varış süresi hesaplanırken bir hata oluştu.");
        }
    }

    /// <summary>
    /// Yolculuk maliyetini hesaplar
    /// </summary>
    [HttpGet("calculate-cost")]
    public async Task<ActionResult<decimal>> CalculateCost(
        [FromQuery, Required] double distanceKm,
        [FromQuery, Required] double durationMinutes,
        [FromQuery] string? vehicleType = null)
    {
        try
        {
            var cost = await _matchingRepository.CalculateEstimatedCostAsync(distanceKm, durationMinutes, vehicleType);
            return Ok(cost);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating cost");
            return StatusCode(500, "Maliyet hesaplanırken bir hata oluştu.");
        }
    }

    #endregion

    #region Private Helper Methods

    private static string GetStatusMessage(MatchingStatus status)
    {
        return status switch
        {
            MatchingStatus.Pending => "Talep beklemede",
            MatchingStatus.Accepted => "Talep kabul edildi",
            MatchingStatus.Rejected => "Talep reddedildi",
            MatchingStatus.Cancelled => "Talep iptal edildi",
            MatchingStatus.Expired => "Talep süresi doldu",
            MatchingStatus.CounterOffered => "Sürücü karşı teklif verdi",
            _ => "Bilinmeyen durum"
        };
    }

    #endregion
}