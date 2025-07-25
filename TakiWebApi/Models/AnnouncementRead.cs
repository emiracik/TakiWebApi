using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TakiWebApi.Models;

[Table("AnnouncementRead")]
public class AnnouncementRead
{
    [Key]
    public int id { get; set; }

    public int userId { get; set; }

    public int announcementId { get; set; }

    public DateTime createdDate { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("userId")]
    public virtual User User { get; set; } = null!;

    [ForeignKey("announcementId")]
    public virtual Announcement Announcement { get; set; } = null!;
}
