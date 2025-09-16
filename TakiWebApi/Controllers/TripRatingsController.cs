using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TakiWebApi.Data;
using TakiWebApi.Models;

namespace TakiWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TripRatingsController : ControllerBase
{
    private readonly ITripRatingRepository _tripRatingRepository;

    public TripRatingsController(ITripRatingRepository tripRatingRepository)
    {
        _tripRatingRepository = tripRatingRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TripRating>>> GetAll()
    {
        var ratings = await _tripRatingRepository.GetAllAsync();
        return Ok(ratings);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TripRating>> GetById(int id)
    {
        var rating = await _tripRatingRepository.GetByIdAsync(id);
        if (rating == null)
            return NotFound();
        return Ok(rating);
    }

    [HttpGet("trip/{tripId}")]
    public async Task<ActionResult<IEnumerable<TripRating>>> GetByTripId(int tripId)
    {
        var ratings = await _tripRatingRepository.GetByTripIdAsync(tripId);
        return Ok(ratings);
    }

    [HttpGet("trip/{tripId}/average")]
    public async Task<ActionResult<decimal>> GetAverageByTripId(int tripId)
    {
        var avg = await _tripRatingRepository.GetAverageByTripIdAsync(tripId);
        return Ok(avg);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] TripRating rating)
    {
        rating.CreatedDate = DateTime.UtcNow;
        await _tripRatingRepository.AddAsync(rating);
        return Ok();
    }
}
