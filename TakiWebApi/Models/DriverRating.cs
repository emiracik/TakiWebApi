using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TakiWebApi.Models;

[Table("DriverRatings")]
public class DriverRating
{
    [Key]
    public int RatingID { get; set; }

    public int? TripID { get; set; }

    public int? DriverID { get; set; }

    public int? UserID { get; set; }

    [Range(1, 5)]
    public int? Rating { get; set; }

    public DateTime RatedAt { get; set; } = DateTime.UtcNow;

    public int? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

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
