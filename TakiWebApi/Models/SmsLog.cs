using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TakiWebApi.Models;

[Table("SmsLogs")]
public class SmsLog
{
    [Key]
    public int SmsID { get; set; }

    [Required]
    [StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? MessageBody { get; set; }

    public bool IsSuccess { get; set; } = false;

    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    [StringLength(1000)]
    public string? ProviderResponse { get; set; }

    public int? OTPID { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    [ForeignKey("OTPID")]
    public virtual OtpRequest? OtpRequest { get; set; }
}
