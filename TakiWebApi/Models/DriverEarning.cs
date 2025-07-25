using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TakiWebApi.Models;

[Table("DriverEarnings")]
public class DriverEarning
{
    [Key]
    public int EarningID { get; set; }

    public int? DriverID { get; set; }

    public int? TripID { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? Amount { get; set; }

    public DateTime EarnedAt { get; set; } = DateTime.UtcNow;

    public int? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? DeletedBy { get; set; }

    public DateTime? DeletedDate { get; set; }

    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    [ForeignKey("DriverID")]
    public virtual Driver? Driver { get; set; }

    [ForeignKey("TripID")]
    public virtual Trip? Trip { get; set; }
}
