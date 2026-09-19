using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TakiWebApi.Data;

namespace TakiWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DriverEarningsController : ControllerBase
{
    private readonly IDriverEarningRepository _repo;

    public DriverEarningsController(IDriverEarningRepository repo)
    {
        _repo = repo;
    }

    /// <summary>
    /// Sürücünün tüm hakediş kayıtları (her tamamlanan yolculuk için tek
    /// satır — komisyon modeli aktifse Trip.Cost'tan düşük olabilir).
    /// </summary>
    [HttpGet("driver/{driverId}")]
    public async Task<IActionResult> GetByDriver(int driverId)
    {
        var earnings = await _repo.GetByDriverIdAsync(driverId);
        return Ok(earnings);
    }

    /// <summary>
    /// Sürücünün toplam (tüm zamanlar) hakedişi.
    /// </summary>
    [HttpGet("driver/{driverId}/total")]
    public async Task<IActionResult> GetTotal(int driverId)
    {
        var total = await _repo.GetTotalByDriverIdAsync(driverId);
        return Ok(total);
    }
}
