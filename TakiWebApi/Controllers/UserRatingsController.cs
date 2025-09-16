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

    public UserRatingsController(IUserRatingRepository userRatingRepository)
    {
        _userRatingRepository = userRatingRepository;
    }

    [HttpGet]
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
        var ratings = await _userRatingRepository.GetByTripIdAsync(tripId);
        return Ok(ratings);
    }

    [HttpGet("user/{userId}/average")]
    public async Task<ActionResult<decimal>> GetAverageByUserId(int userId)
    {
        var avg = await _userRatingRepository.GetAverageByUserIdAsync(userId);
        return Ok(avg);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] UserRating rating)
    {
        rating.CreatedDate = DateTime.UtcNow;
        await _userRatingRepository.AddAsync(rating);
        return Ok();
    }
}
