using System.ComponentModel.DataAnnotations;

namespace TakiWebApi.Models;

// dbo.ScheduledRides (Migrations/003). Yolcu ileri bir saat için yolculuk
// planlar; zamanı yaklaşınca bildirim gelir ve tek dokunuşla taksi çağırır.
public class ScheduledRide
{
    public int ScheduledRideID { get; set; }
    public int UserID { get; set; }

    [Required, StringLength(500)]
    public string PickupAddress { get; set; } = string.Empty;
    public double PickupLatitude { get; set; }
    public double PickupLongitude { get; set; }

    [Required, StringLength(500)]
    public string DropoffAddress { get; set; } = string.Empty;
    public double DropoffLatitude { get; set; }
    public double DropoffLongitude { get; set; }

    /// <summary>UTC.</summary>
    public DateTime ScheduledTime { get; set; }

    [StringLength(20)]
    public string? VehicleType { get; set; }

    [StringLength(10)]
    public string? PaymentMethod { get; set; }

    public decimal? EstimatedCost { get; set; }

    public string Status { get; set; } = "Scheduled"; // Scheduled | Reminded | Cancelled
    public DateTime CreatedDate { get; set; }
}
