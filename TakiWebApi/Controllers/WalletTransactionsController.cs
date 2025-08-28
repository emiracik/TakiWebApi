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
    public async Task<ActionResult<IEnumerable<WalletTransaction>>> GetAll() => Ok(await _repo.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<ActionResult<WalletTransaction>> GetById(int id)
    {
        var tx = await _repo.GetByIdAsync(id);
        return tx == null ? NotFound() : Ok(tx);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<WalletTransaction>>> GetByUserId(int userId) => Ok(await _repo.GetByUserIdAsync(userId));

    [HttpPost]
    public async Task<ActionResult<int>> Create([FromBody] WalletTransaction tx)
    {
        var id = await _repo.CreateAsync(tx);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }
}
