using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TakiWebApi.Models;

[Table("OtpRequests")]
public class OtpRequest
{
    [Key]
    public int OtpID { get; set; }

    public int? UserID { get; set; }

    [Required]
    [StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(10)]
    public string OTPCode { get; set; } = string.Empty;

    public DateTime ExpirationTime { get; set; }

    public bool IsUsed { get; set; } = false;

    public DateTime? UsedAt { get; set; }

    [StringLength(50)]
    public string? RequestIP { get; set; }

    [StringLength(100)]
    public string? RequestSource { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public int? CreatedBy { get; set; }

    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    [ForeignKey("UserID")]
    public virtual User? User { get; set; }

    public virtual ICollection<SmsLog> SmsLogs { get; set; } = new List<SmsLog>();
}
