using TakiWebApi.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TakiWebApi.Data;
using TakiWebApi.Models;

namespace TakiWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserRatingsController : ControllerBase
{
    private readonly IUserRatingRepository _userRatingRepository;
    private readonly ITripRepository _tripRepository;

    public UserRatingsController(IUserRatingRepository userRatingRepository, ITripRepository tripRepository)
    {
        _userRatingRepository = userRatingRepository;
        _tripRepository = tripRepository;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<UserRating>>> GetAll()
    {
        var ratings = await _userRatingRepository.GetAllAsync();
        return Ok(ratings);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserRating>> GetById(int id)
    {
        var rating = await _userRatingRepository.GetByIdAsync(id);
        if (rating == null)
            return NotFound();
        if (!User.IsPartyOf(rating.RatedUserID, rating.RatedByDriverID))
            return Forbid();
        return Ok(rating);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<UserRating>>> GetByUserId(int userId)
    {
        var ratings = await _userRatingRepository.GetByUserIdAsync(userId);
        return Ok(ratings);
    }

    [HttpGet("driver/{driverId}")]
    public async Task<ActionResult<IEnumerable<UserRating>>> GetByDriverId(int driverId)
    {
        var ratings = await _userRatingRepository.GetByDriverIdAsync(driverId);
        return Ok(ratings);
    }

    [HttpGet("trip/{tripId}")]
    public async Task<ActionResult<IEnumerable<UserRating>>> GetByTripId(int tripId)
    {
        var trip = await _tripRepository.GetTripByIdAsync(tripId);
        if (trip == null) return NotFound();
        if (!User.IsPartyOf(trip.PassengerID, trip.DriverID)) return Forbid();
        var ratings = await _userRatingRepository.GetByTripIdAsync(tripId);
        return Ok(ratings);
    }

    [HttpGet("user/{userId}/average")]
    [AllowAnyCaller]
    public async Task<ActionResult<decimal>> GetAverageByUserId(int userId)
    {
        var avg = await _userRatingRepository.GetAverageByUserIdAsync(userId);
        return Ok(avg);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] UserRating rating)
    {
        if (!User.IsDriver(rating.RatedByDriverID)) return Forbid();
        if (rating.Rating < 1 || rating.Rating > 5) return BadRequest("Puan 1 ile 5 arasında olmalı.");

        // Sürücü yalnızca kendi tamamlanmış yolculuğundaki yolcuyu, bir kez puanlar.
        var trip = await _tripRepository.GetTripByIdAsync(rating.TripID);
        if (trip == null || trip.DriverID != rating.RatedByDriverID || trip.PassengerID != rating.RatedUserID)
            return Forbid();
        if (trip.Status != RideStatus.completed && trip.Status != RideStatus.awaitingConfirmation)
            return BadRequest("Yalnızca biten yolculuk puanlanabilir.");
        if ((await _userRatingRepository.GetByTripIdAsync(rating.TripID)).Any())
            return Conflict("Bu yolculuk zaten puanlandı.");

        rating.CreatedDate = DateTime.UtcNow;
        await _userRatingRepository.AddAsync(rating);
        return Ok();
    }
}
