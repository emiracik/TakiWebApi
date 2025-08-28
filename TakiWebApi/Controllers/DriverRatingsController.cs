using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TakiWebApi.Data;
using TakiWebApi.Models;

namespace TakiWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DriverRatingsController : ControllerBase
{
    private readonly IDriverRatingRepository _driverRatingRepository;

    public DriverRatingsController(IDriverRatingRepository driverRatingRepository)
    {
        _driverRatingRepository = driverRatingRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DriverRating>>> GetAllDriverRatings()
    {
        var driverRatings = await _driverRatingRepository.GetAllDriverRatingsAsync();
        return Ok(driverRatings);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DriverRating>> GetDriverRating(int id)
    {
        var driverRating = await _driverRatingRepository.GetDriverRatingByIdAsync(id);
        
        if (driverRating == null)
        {
            return NotFound();
        }

        return Ok(driverRating);
    }

    [HttpGet("driver/{driverId}")]
    public async Task<ActionResult<IEnumerable<DriverRating>>> GetDriverRatingsByDriverId(int driverId)
    {
        var driverRatings = await _driverRatingRepository.GetDriverRatingsByDriverIdAsync(driverId);
        return Ok(driverRatings);
    }

    [HttpGet("driver/{driverId}/average")]
    public async Task<ActionResult<decimal>> GetAverageRatingByDriverId(int driverId)
    {
        var averageRating = await _driverRatingRepository.GetAverageRatingByDriverIdAsync(driverId);
        return Ok(averageRating);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<DriverRating>>> GetDriverRatingsByUserId(int userId)
    {
        var driverRatings = await _driverRatingRepository.GetDriverRatingsByUserIdAsync(userId);
        return Ok(driverRatings);
    }

    [HttpGet("trip/{tripId}")]
    public async Task<ActionResult<IEnumerable<DriverRating>>> GetDriverRatingsByTripId(int tripId)
    {
        var driverRatings = await _driverRatingRepository.GetDriverRatingsByTripIdAsync(tripId);
        return Ok(driverRatings);
    }

    [HttpGet("rating/{rating}")]
    public async Task<ActionResult<IEnumerable<DriverRating>>> GetDriverRatingsByRatingValue(decimal rating)
    {
        if (rating < 1 || rating > 5)
        {
            return BadRequest("Rating must be between 1 and 5.");
        }

        var driverRatings = await _driverRatingRepository.GetDriverRatingsByRatingValueAsync(rating);
        return Ok(driverRatings);
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<DriverRating>>> GetActiveDriverRatings()
    {
        var driverRatings = await _driverRatingRepository.GetActiveDriverRatingsAsync();
        return Ok(driverRatings);
    }

    [HttpGet("paginated")]
    public async Task<ActionResult<IEnumerable<DriverRating>>> GetDriverRatingsPaginated([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        if (pageNumber < 1 || pageSize < 1)
        {
            return BadRequest("PageNumber and PageSize must be greater than 0.");
        }

        var driverRatings = await _driverRatingRepository.GetDriverRatingsPaginatedAsync(pageNumber, pageSize);
        return Ok(driverRatings);
    }

    [HttpGet("count")]
    public async Task<ActionResult<int>> GetTotalDriverRatingsCount()
    {
        var count = await _driverRatingRepository.GetTotalDriverRatingsCountAsync();
        return Ok(count);
    }

    [HttpGet("active/count")]
    public async Task<ActionResult<int>> GetActiveDriverRatingsCount()
    {
        var count = await _driverRatingRepository.GetActiveDriverRatingsCountAsync();
        return Ok(count);
    }

    [HttpGet("date-range")]
    public async Task<ActionResult<IEnumerable<DriverRating>>> GetDriverRatingsByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        if (startDate > endDate)
        {
            return BadRequest("Start date cannot be greater than end date.");
        }

        var driverRatings = await _driverRatingRepository.GetDriverRatingsByDateRangeAsync(startDate, endDate);
        return Ok(driverRatings);
    }

    [HttpPost]
    public async Task<ActionResult<DriverRating>> CreateDriverRating([FromBody] DriverRating driverRating)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (driverRating.Rating < 1 || driverRating.Rating > 5)
        {
            return BadRequest("Rating must be between 1 and 5.");
        }

        try
        {
            var ratingId = await _driverRatingRepository.CreateDriverRatingAsync(driverRating);
            var createdDriverRating = await _driverRatingRepository.GetDriverRatingByIdAsync(ratingId);
            
            return CreatedAtAction(nameof(GetDriverRating), new { id = ratingId }, createdDriverRating);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDriverRating(int id, [FromBody] DriverRating driverRating)
    {
        if (id != driverRating.DriverRatingID)
        {
            return BadRequest("ID mismatch between route and body.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (driverRating.Rating < 1 || driverRating.Rating > 5)
        {
            return BadRequest("Rating must be between 1 and 5.");
        }

        try
        {
            var existingDriverRating = await _driverRatingRepository.GetDriverRatingByIdAsync(id);
            if (existingDriverRating == null)
            {
                return NotFound();
            }

            driverRating.UpdatedDate = DateTime.UtcNow;
            var success = await _driverRatingRepository.UpdateDriverRatingAsync(driverRating);
            
            if (!success)
            {
                return StatusCode(500, "Failed to update driver rating.");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDriverRating(int id)
    {
        try
        {
            var existingDriverRating = await _driverRatingRepository.GetDriverRatingByIdAsync(id);
            if (existingDriverRating == null)
            {
                return NotFound();
            }

            var success = await _driverRatingRepository.DeleteDriverRatingAsync(id);
            
            if (!success)
            {
                return StatusCode(500, "Failed to delete driver rating.");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
