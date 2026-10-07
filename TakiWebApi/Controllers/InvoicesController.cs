using TakiWebApi.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TakiWebApi.Data;
using TakiWebApi.Models;

namespace TakiWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceRepository _repo;
    public InvoicesController(IInvoiceRepository repo) => _repo = repo;

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<Invoice>>> GetAll() => Ok(await _repo.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<ActionResult<Invoice>> GetById(int id)
    {
        var invoice = await _repo.GetByIdAsync(id);
        if (invoice == null) return NotFound();
        if (!(User.IsPassenger(invoice.UserID))) return Forbid();
        return Ok(invoice);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<Invoice>>> GetByUserId(int userId) => Ok(await _repo.GetByUserIdAsync(userId));

    [HttpGet("trip/{tripId}")]
    public async Task<ActionResult<IEnumerable<Invoice>>> GetByTripId(int tripId) =>
        Ok((await _repo.GetByTripIdAsync(tripId)).Where(i => User.IsAdmin() || User.IsPassenger(i.UserID)));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<int>> Create([FromBody] Invoice invoice)
    {
        var id = await _repo.CreateAsync(invoice);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] Invoice invoice)
    {
        if (id != invoice.InvoiceID) return BadRequest();
        var success = await _repo.UpdateAsync(invoice);
        return success ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _repo.DeleteAsync(id);
        return success ? NoContent() : NotFound();
    }
}
