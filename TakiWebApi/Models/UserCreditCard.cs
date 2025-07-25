using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TakiWebApi.Models;

[Table("UserCreditCards")]
public class UserCreditCard
{
    [Key]
    public int CardID { get; set; }

    public int? UserID { get; set; }

    [StringLength(200)]
    public string? CardHolderName { get; set; }

    [StringLength(20)]
    public string? CardNumberMasked { get; set; }

    [Range(1, 12)]
    public int? ExpiryMonth { get; set; }

    public int? ExpiryYear { get; set; }

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
