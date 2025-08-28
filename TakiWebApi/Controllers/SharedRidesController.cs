using Microsoft.AspNetCore.Mvc;
using TakiWebApi.Data;
using TakiWebApi.Models;

namespace TakiWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SharedRidesController : ControllerBase
{
    private readonly ISharedRideRepository _repo;
    public SharedRidesController(ISharedRideRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SharedRide>>> GetAll() => Ok(await _repo.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<ActionResult<SharedRide>> GetById(int id)
    {
        var ride = await _repo.GetByIdAsync(id);
        return ride == null ? NotFound() : Ok(ride);
    }

    [HttpGet("trip/{tripId}")]
    public async Task<ActionResult<IEnumerable<SharedRide>>> GetByTripId(int tripId) => Ok(await _repo.GetByTripIdAsync(tripId));

    [HttpPost]
    public async Task<ActionResult<int>> Create([FromBody] SharedRide ride)
    {
        var id = await _repo.CreateAsync(ride);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] SharedRide ride)
    {
        if (id != ride.SharedRideID) return BadRequest();
        var success = await _repo.UpdateAsync(ride);
        return success ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _repo.DeleteAsync(id);
        return success ? NoContent() : NotFound();
    }
}
