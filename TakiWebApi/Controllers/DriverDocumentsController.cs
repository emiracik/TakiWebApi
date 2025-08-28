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
    public async Task<ActionResult<IEnumerable<DriverDocument>>> GetAll() => Ok(await _repo.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<ActionResult<DriverDocument>> GetById(int id)
    {
        var doc = await _repo.GetByIdAsync(id);
        return doc == null ? NotFound() : Ok(doc);
    }

    [HttpGet("driver/{driverId}")]
    public async Task<ActionResult<IEnumerable<DriverDocument>>> GetByDriverId(int driverId) => Ok(await _repo.GetByDriverIdAsync(driverId));

    [HttpPost]
    public async Task<ActionResult<int>> Create([FromBody] DriverDocument doc)
    {
        var id = await _repo.CreateAsync(doc);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _repo.DeleteAsync(id);
        return success ? NoContent() : NotFound();
    }
}
