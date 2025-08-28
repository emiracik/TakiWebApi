using Microsoft.AspNetCore.Mvc;
using TakiWebApi.Data;
using TakiWebApi.Models;

namespace TakiWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FeedbacksController : ControllerBase
{
    private readonly IFeedbackRepository _repo;
    public FeedbacksController(IFeedbackRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Feedback>>> GetAll() => Ok(await _repo.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<ActionResult<Feedback>> GetById(int id)
    {
        var feedback = await _repo.GetByIdAsync(id);
        return feedback == null ? NotFound() : Ok(feedback);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<Feedback>>> GetByUserId(int userId) => Ok(await _repo.GetByUserIdAsync(userId));

    [HttpGet("driver/{driverId}")]
    public async Task<ActionResult<IEnumerable<Feedback>>> GetByDriverId(int driverId) => Ok(await _repo.GetByDriverIdAsync(driverId));

    [HttpGet("trip/{tripId}")]
    public async Task<ActionResult<IEnumerable<Feedback>>> GetByTripId(int tripId) => Ok(await _repo.GetByTripIdAsync(tripId));

    [HttpPost]
    public async Task<ActionResult<int>> Create([FromBody] Feedback feedback)
    {
        var id = await _repo.CreateAsync(feedback);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _repo.DeleteAsync(id);
        return success ? NoContent() : NotFound();
    }
}
