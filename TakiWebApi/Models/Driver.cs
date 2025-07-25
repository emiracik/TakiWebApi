using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TakiWebApi.Models;

[Table("Drivers")]
public class Driver
{
    [Key]
    public int DriverID { get; set; }

    [Required]
    [StringLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Email { get; set; }

    [StringLength(20)]
    public string? VehiclePlate { get; set; }

    [StringLength(100)]
    public string? VehicleModel { get; set; }

    [StringLength(50)]
    public string? VehicleColor { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? DeletedBy { get; set; }

    public DateTime? DeletedDate { get; set; }

    public bool IsDeleted { get; set; } = false;

    // Navigation properties - removed virtual keyword since we're using ADO.NET
    public ICollection<Trip> Trips { get; set; } = new List<Trip>();
    public ICollection<DriverRating> DriverRatings { get; set; } = new List<DriverRating>();
    public ICollection<DriverReview> DriverReviews { get; set; } = new List<DriverReview>();
    public ICollection<DriverEarning> DriverEarnings { get; set; } = new List<DriverEarning>();
}
