using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TakiWebApi.Models;

public enum MatchingStatus
{
    Pending,
    Accepted,
    Rejected,
    Cancelled,
    Expired
}

[Table("MatchingRequests")]
public class MatchingRequest
{
    [Key]
    public int MatchingRequestID { get; set; }

    [Required]
    public int PassengerID { get; set; }

    public int? DriverID { get; set; }

    [Required]
    [StringLength(500)]
    public string PickupAddress { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string DropoffAddress { get; set; } = string.Empty;

    [Required]
    public double PickupLatitude { get; set; }

    [Required]
    public double PickupLongitude { get; set; }

    [Required]
    public double DropoffLatitude { get; set; }

    [Required]
    public double DropoffLongitude { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? EstimatedCost { get; set; }

    public double? EstimatedDistance { get; set; }

    public int? EstimatedDuration { get; set; } // minutes

    public MatchingStatus Status { get; set; } = MatchingStatus.Pending;

    public DateTime RequestTime { get; set; } = DateTime.UtcNow;

    public DateTime? AcceptedTime { get; set; }

    public DateTime? ExpiryTime { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    public double? MaxWaitTime { get; set; } // minutes

    public double? MinRating { get; set; }

    public double? MaxDistance { get; set; } // km from pickup

    // Audit fields
    public int? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public int? UpdatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public int? DeletedBy { get; set; }
    public DateTime? DeletedDate { get; set; }
    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    public virtual User? Passenger { get; set; }
    public virtual Driver? Driver { get; set; }
}