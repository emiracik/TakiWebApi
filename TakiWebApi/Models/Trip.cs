using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TakiWebApi.Models;

public enum RideStatus
{
    completed,
    cancelled,
    inProgress
}

[Table("Trips")]
public class Trip
{
    public RideStatus? Status { get; set; }
    [Key]
    public int TripID { get; set; }

    public int? PassengerID { get; set; }

    public int? DriverID { get; set; }

    [StringLength(500)]
    public string? StartAddress { get; set; }

    [StringLength(500)]
    public string? EndAddress { get; set; }

    public double? StartLatitude { get; set; }

    public double? StartLongitude { get; set; }

    public double? EndLatitude { get; set; }

    public double? EndLongitude { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? Cost { get; set; }

    [StringLength(50)]
    public string? PaymentMethod { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? DeletedBy { get; set; }

    public DateTime? DeletedDate { get; set; }

    public bool IsDeleted { get; set; } = false;

    public double? Distance { get; set; }

    // Navigation properties - removed virtual keyword since we're using ADO.NET
    [ForeignKey("PassengerID")]
    public User? Passenger { get; set; }

    [ForeignKey("DriverID")]
    public Driver? Driver { get; set; }

    public ICollection<DriverRating> DriverRatings { get; set; } = new List<DriverRating>();
    public ICollection<DriverReview> DriverReviews { get; set; } = new List<DriverReview>();
    public ICollection<DriverEarning> DriverEarnings { get; set; } = new List<DriverEarning>();
}
