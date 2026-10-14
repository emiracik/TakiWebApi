using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TakiWebApi.Data;
using TakiWebApi.Models;
using TakiWebApi.Security;

namespace TakiWebApi.Controllers;

/// <summary>
/// İleri tarihli yolculuk planları. Saat yaklaşınca
/// <see cref="Services.ScheduledRideReminderService"/> yolcuya bildirim
/// gönderir; yolcu bildirimden tek dokunuşla taksi çağırır.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ScheduledRidesController : ControllerBase
{
    private static readonly TimeSpan MinLead = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan MaxLead = TimeSpan.FromDays(7);

    private readonly IScheduledRideRepository _repo;

    public ScheduledRidesController(IScheduledRideRepository repo)
    {
        _repo = repo;
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<ScheduledRide>>> GetUpcoming(int userId)
    {
        if (!User.IsPassenger(userId) && !User.IsAdmin()) return Forbid();
        return Ok(await _repo.GetUpcomingByUserAsync(userId));
    }

    [HttpPost]
    public async Task<ActionResult<ScheduledRide>> Create([FromBody] ScheduledRide ride)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (!User.IsPassenger(ride.UserID)) return Forbid();

        var when = ride.ScheduledTime.Kind == DateTimeKind.Utc
            ? ride.ScheduledTime
            : DateTime.SpecifyKind(ride.ScheduledTime, DateTimeKind.Utc);
        var lead = when - DateTime.UtcNow;
        if (lead < MinLead) return BadRequest("Yolculuk en az 30 dakika sonrası için planlanabilir.");
        if (lead > MaxLead) return BadRequest("Yolculuk en fazla 7 gün sonrası için planlanabilir.");

        var pm = ride.PaymentMethod?.Trim().ToLowerInvariant();
        if (pm is not (null or "wallet" or "card" or "cash")) return BadRequest("Geçersiz ödeme yöntemi.");

        ride.ScheduledTime = when;
        ride.PaymentMethod = pm;
        ride.Status = "Scheduled";
        ride.ScheduledRideID = await _repo.CreateAsync(ride);
        return Ok(await _repo.GetByIdAsync(ride.ScheduledRideID));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Cancel(int id)
    {
        var ride = await _repo.GetByIdAsync(id);
        if (ride == null) return NotFound();
        if (!User.IsPassenger(ride.UserID) && !User.IsAdmin()) return Forbid();
        await _repo.CancelAsync(id);
        return NoContent();
    }
}
