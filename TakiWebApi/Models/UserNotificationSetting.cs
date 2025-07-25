using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TakiWebApi.Models;

[Table("UserNotificationSettings")]
public class UserNotificationSetting
{
    [Key]
    public int SettingID { get; set; }

    public int? UserID { get; set; }

    public bool AllowPromotions { get; set; } = true;

    public bool AllowTripUpdates { get; set; } = true;

    public bool AllowNews { get; set; } = true;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("UserID")]
    public virtual User? User { get; set; }
}
