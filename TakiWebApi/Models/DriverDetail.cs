using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TakiWebApi.Models;

[Table("DriverDetails")]
public class DriverDetail
{
    [Key]
    public int DriverDetailID { get; set; }

    public int? UserID { get; set; }

    [StringLength(20)]
    public string? VehiclePlate { get; set; }

    [StringLength(100)]
    public string? VehicleModel { get; set; }

    [StringLength(50)]
    public string? VehicleColor { get; set; }

    [StringLength(50)]
    public string? LicenseNumber { get; set; }

    public int? ExperienceYears { get; set; }

    public double? RatingAverage { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? DeletedBy { get; set; }

    public DateTime? DeletedDate { get; set; }

    public bool IsDeleted { get; set; } = false;

    // Navigation properties - removed virtual keyword since we're using ADO.NET
    [ForeignKey("UserID")]
    public User? User { get; set; }
}
