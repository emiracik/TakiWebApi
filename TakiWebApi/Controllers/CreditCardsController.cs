using Microsoft.AspNetCore.Mvc;
using TakiWebApi.Data;
using TakiWebApi.Models;

namespace TakiWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CreditCardsController : ControllerBase
{
    private readonly ICreditCardRepository _repo;
    public CreditCardsController(ICreditCardRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CreditCard>>> GetAll() => Ok(await _repo.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<ActionResult<CreditCard>> GetById(int id)
    {
        var card = await _repo.GetByIdAsync(id);
        return card == null ? NotFound() : Ok(card);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<CreditCard>>> GetByUserId(int userId) => Ok(await _repo.GetByUserIdAsync(userId));

    [HttpPost]
    public async Task<ActionResult<int>> Create([FromBody] CreditCard card)
    {
        var id = await _repo.CreateAsync(card);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreditCard card)
    {
        if (id != card.CardID) return BadRequest();
        var success = await _repo.UpdateAsync(card);
        return success ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _repo.DeleteAsync(id);
        return success ? NoContent() : NotFound();
    }
}
