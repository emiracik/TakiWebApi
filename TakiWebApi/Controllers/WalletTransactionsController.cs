using TakiWebApi.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TakiWebApi.Data;
using TakiWebApi.Models;

namespace TakiWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WalletTransactionsController : ControllerBase
{
    private readonly IWalletTransactionRepository _repo;
    public WalletTransactionsController(IWalletTransactionRepository repo) => _repo = repo;

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<WalletTransaction>>> GetAll() => Ok(await _repo.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<ActionResult<WalletTransaction>> GetById(int id)
    {
        var tx = await _repo.GetByIdAsync(id);
        if (tx == null) return NotFound();
        if (!(User.IsPassenger(tx.UserID))) return Forbid();
        return Ok(tx);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<WalletTransaction>>> GetByUserId(int userId) => Ok(await _repo.GetByUserIdAsync(userId));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<int>> Create([FromBody] WalletTransaction tx)
    {
        var id = await _repo.CreateAsync(tx);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }
}
