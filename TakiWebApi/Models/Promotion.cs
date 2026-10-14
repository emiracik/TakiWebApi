namespace TakiWebApi.Models;

// dbo.Promotions (Code sütunu Migrations/003 ile eklendi).
public class Promotion
{
    public int PromotionID { get; set; }
    public string? Code { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DiscountType { get; set; } = "FixedAmount"; // Percentage | FixedAmount
    public decimal DiscountValue { get; set; }
    public decimal? MinimumAmount { get; set; }
    public decimal? MaximumDiscount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? UsageLimit { get; set; }
    public int UsedCount { get; set; }
    public bool IsActive { get; set; } = true;

    /// <summary>Verilen ücrete uygulanacak indirim (ücreti aşmaz).</summary>
    public decimal DiscountFor(decimal fare)
    {
        if (MinimumAmount is decimal min && fare < min) return 0;
        var discount = DiscountType.Equals("Percentage", StringComparison.OrdinalIgnoreCase)
            ? Math.Round(fare * DiscountValue / 100m, 2)
            : DiscountValue;
        if (MaximumDiscount is decimal max && discount > max) discount = max;
        return Math.Min(discount, fare);
    }
}

public class PromotionCheckRequest
{
    public string Code { get; set; } = string.Empty;
    public decimal? Fare { get; set; }
}

public class PromotionCheckResponse
{
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DiscountType { get; set; } = string.Empty;
    public decimal DiscountValue { get; set; }
    public decimal? MinimumAmount { get; set; }
    public decimal? MaximumDiscount { get; set; }
    public DateTime EndDate { get; set; }
    public decimal? Discount { get; set; } // Fare verildiyse hesaplanan indirim
}
