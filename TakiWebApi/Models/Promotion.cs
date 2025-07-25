using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TakiWebApi.Models;

[Table("Promotions")]
public class Promotion
{
    [Key]
    public int PromoID { get; set; }

    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? DiscountAmount { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public int? MaxUsageCount { get; set; }

    public int UsedCount { get; set; } = 0;

    public bool IsActive { get; set; } = true;
}
