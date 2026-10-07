using TakiWebApi.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TakiWebApi.Data;
using TakiWebApi.Models;

namespace TakiWebApi.Controllers;

[MessagingIds]
[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly IMessageRepository _repo;
    public MessagesController(IMessageRepository repo) => _repo = repo;

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<Message>>> GetAll() => Ok(await _repo.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<ActionResult<Message>> GetById(int id)
    {
        var msg = await _repo.GetByIdAsync(id);
        if (msg == null) return NotFound();
        if (!(User.IsMessageParty(msg.SenderID) || User.IsMessageParty(msg.ReceiverID))) return Forbid();
        return Ok(msg);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<Message>>> GetByUserId(int userId) => Ok(await _repo.GetByUserIdAsync(userId));

    [HttpPost]
    public async Task<ActionResult<int>> Create([FromBody] Message msg)
    {
        // Mesaj sadece token sahibi adına gönderilebilir.
        if (!User.IsMessageParty(msg.SenderID)) return Forbid();
        var id = await _repo.CreateAsync(msg);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("read/{id}")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing == null) return NotFound();
        if (!User.IsMessageParty(existing.ReceiverID)) return Forbid();
        var success = await _repo.MarkAsReadAsync(id);
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
