using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TakiWebApi.Models;

[Table("UserAddresses")]
public class UserAddress
{
    [Key]
    public int UserAddressID { get; set; }

    public int? UserID { get; set; }

    [StringLength(100)]
    public string? AddressName { get; set; }

    [StringLength(500)]
    public string? FullAddress { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public bool IsDefault { get; set; } = false;

    [StringLength(50)]
    public string? AddressType { get; set; } // Home, Work, Other

    public int? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? DeletedBy { get; set; }

    public DateTime? DeletedDate { get; set; }

    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    [ForeignKey("UserID")]
    public virtual User? User { get; set; }
}
