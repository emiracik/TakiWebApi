using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TakiWebApi.Data;
using TakiWebApi.Models;
using TakiWebApi.Security;

namespace TakiWebApi.Controllers;

/// <summary>
/// Yolcu tarafı promosyon kodları: geçerli kampanyaları listeler ve girilen
/// kodu doğrular. İndirim, talep oluşturulurken (find-best-match) sunucuda
/// uygulanır; buradaki "check" yalnızca önizlemedir.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PromotionsController : ControllerBase
{
    private readonly IPromotionRepository _repo;

    public PromotionsController(IPromotionRepository repo)
    {
        _repo = repo;
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<PromotionCheckResponse>>> GetActive()
    {
        var promos = await _repo.GetActiveAsync();
        return Ok(promos.Select(p => ToResponse(p, null)));
    }

    [HttpPost("check")]
    public async Task<ActionResult<PromotionCheckResponse>> Check([FromBody] PromotionCheckRequest request)
    {
        if (User.FindFirstValue("user_type") != "user" || User.CallerId() is not int userId)
        {
            return Forbid();
        }
        if (string.IsNullOrWhiteSpace(request.Code))
        {
            return BadRequest("Kod girin.");
        }

        var promo = await _repo.GetValidByCodeAsync(request.Code);
        if (promo == null)
        {
            return NotFound("Kod geçersiz veya süresi dolmuş.");
        }
        if (await _repo.HasUserUsedAsync(promo.PromotionID, userId))
        {
            return Conflict("Bu kodu daha önce kullandın.");
        }
        if (request.Fare is decimal fare && promo.MinimumAmount is decimal min && fare < min)
        {
            return BadRequest($"Bu kod en az ₺{min:0.##} tutarındaki yolculuklarda geçerli.");
        }

        return Ok(ToResponse(promo, request.Fare));
    }

    private static PromotionCheckResponse ToResponse(Promotion p, decimal? fare) => new()
    {
        Code = p.Code ?? string.Empty,
        Title = p.Title,
        Description = p.Description,
        DiscountType = p.DiscountType,
        DiscountValue = p.DiscountValue,
        MinimumAmount = p.MinimumAmount,
        MaximumDiscount = p.MaximumDiscount,
        EndDate = p.EndDate,
        Discount = fare is decimal f ? p.DiscountFor(f) : null,
    };
}
