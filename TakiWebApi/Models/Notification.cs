using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TakiWebApi.Models;

[Table("Notifications")]
public class Notification
{
    [Key]
    public int NotificationID { get; set; }

    [StringLength(200)]
    public string? Title { get; set; }

    public string? Message { get; set; }

    public int? UserID { get; set; }

    public int? DriverID { get; set; }

    [StringLength(50)]
    public string? NotificationType { get; set; }

    public bool IsRead { get; set; } = false;

    public DateTime? ReadDate { get; set; }

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

    [ForeignKey("DriverID")]
    public virtual Driver? Driver { get; set; }
}
