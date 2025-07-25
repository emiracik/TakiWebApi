using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TakiWebApi.Models;

[Table("DriverReviews")]
public class DriverReview
{
    [Key]
    public int ReviewID { get; set; }

    public int? TripID { get; set; }

    public int? DriverID { get; set; }

    public int? UserID { get; set; }

    [StringLength(1000)]
    public string? ReviewText { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? DeletedBy { get; set; }

    public DateTime? DeletedDate { get; set; }

    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    [ForeignKey("TripID")]
    public virtual Trip? Trip { get; set; }

    [ForeignKey("DriverID")]
    public virtual Driver? Driver { get; set; }

    [ForeignKey("UserID")]
    public virtual User? User { get; set; }
}
