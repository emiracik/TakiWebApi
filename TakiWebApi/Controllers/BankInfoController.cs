using TakiWebApi.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TakiWebApi.Data;
using TakiWebApi.Models;

namespace TakiWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BankInfoController : ControllerBase
{
    private readonly IBankInfoRepository _repo;
    public BankInfoController(IBankInfoRepository repo) => _repo = repo;

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<BankInfo>>> GetAll() => Ok(await _repo.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<ActionResult<BankInfo>> GetById(int id)
    {
        var info = await _repo.GetByIdAsync(id);
        if (info == null) return NotFound();
        if (!(User.IsDriver(info.DriverID))) return Forbid();
        return Ok(info);
    }

    [HttpGet("driver/{driverId}")]
    public async Task<ActionResult<IEnumerable<BankInfo>>> GetByDriverId(int driverId) => Ok(await _repo.GetByDriverIdAsync(driverId));

    [HttpPost]
    public async Task<ActionResult<int>> Create([FromBody] BankInfo info)
    {
        if (!User.IsDriver(info.DriverID)) return Forbid();
        var id = await _repo.CreateAsync(info);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] BankInfo info)
    {
        if (id != info.BankInfoID) return BadRequest();
        var existing = await _repo.GetByIdAsync(id);
        if (existing == null) return NotFound();
        if (!User.IsDriver(existing.DriverID)) return Forbid();
        info.DriverID = existing.DriverID;
        var success = await _repo.UpdateAsync(info);
        return success ? NoContent() : NotFound();
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
