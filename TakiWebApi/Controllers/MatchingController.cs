using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TakiWebApi.Data;
using TakiWebApi.Models;
using System.ComponentModel.DataAnnotations;

namespace TakiWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MatchingController : ControllerBase
{
    private readonly IMatchingRepository _matchingRepository;
    private readonly ILogger<MatchingController> _logger;

    public MatchingController(IMatchingRepository matchingRepository, ILogger<MatchingController> logger)
    {
        _matchingRepository = matchingRepository;
        _logger = logger;
    }

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

            _logger.LogInformation($"Finding best match for passenger {request.PassengerID}");

            var matchingResult = await _matchingRepository.FindBestMatchAsync(request);
            
            if (matchingResult == null)
            {
                return NotFound("Uygun sürücü bulunamadı.");
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

            // Calculate trip details
            var tripDistance = await _matchingRepository.CalculateDistanceAsync(
                request.PickupLatitude, request.PickupLongitude,
                request.DropoffLatitude, request.DropoffLongitude);

            var estimatedDuration = await _matchingRepository.EstimateArrivalTimeAsync(tripDistance);
            var estimatedCost = await _matchingRepository.CalculateEstimatedCostAsync(tripDistance, estimatedDuration);

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
    public async Task<ActionResult<MatchingResponse>> CancelMatchingRequest(int requestId)
    {
        try
        {
            var response = await _matchingRepository.CancelMatchingRequestAsync(requestId);
            
            if (response.Status == MatchingStatus.Cancelled)
            {
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
            // Check if driver is available
            var isAvailable = await _matchingRepository.IsDriverAvailableAsync(driverId);
            if (!isAvailable)
            {
                return BadRequest("Sürücü şu anda müsait değil.");
            }

            var response = await _matchingRepository.AcceptMatchingRequestAsync(driverId, requestId);
            
            if (response.Status == MatchingStatus.Accepted)
            {
                // Set driver as unavailable
                await _matchingRepository.SetDriverAvailabilityAsync(driverId, false);
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
        [FromQuery, Required] double durationMinutes)
    {
        try
        {
            var cost = await _matchingRepository.CalculateEstimatedCostAsync(distanceKm, durationMinutes);
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
            _ => "Bilinmeyen durum"
        };
    }

    #endregion
}