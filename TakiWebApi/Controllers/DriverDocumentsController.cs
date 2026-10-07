using TakiWebApi.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TakiWebApi.Data;
using TakiWebApi.Models;

namespace TakiWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DriverDocumentsController : ControllerBase
{
    private readonly IDriverDocumentRepository _repo;
    public DriverDocumentsController(IDriverDocumentRepository repo) => _repo = repo;

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<DriverDocument>>> GetAll() => Ok(await _repo.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<ActionResult<DriverDocument>> GetById(int id)
    {
        var doc = await _repo.GetByIdAsync(id);
        if (doc == null) return NotFound();
        if (!(User.IsDriver(doc.DriverID))) return Forbid();
        return Ok(doc);
    }

    [HttpGet("driver/{driverId}")]
    public async Task<ActionResult<IEnumerable<DriverDocument>>> GetByDriverId(int driverId) => Ok(await _repo.GetByDriverIdAsync(driverId));

    [HttpPost]
    public async Task<ActionResult<int>> Create([FromBody] DriverDocument doc)
    {
        if (!User.IsDriver(doc.DriverID)) return Forbid();
        var id = await _repo.CreateAsync(doc);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing == null) return NotFound();
        if (!User.IsDriver(existing.DriverID)) return Forbid();
        var success = await _repo.DeleteAsync(id);
        return success ? NoContent() : NotFound();
    }
}
