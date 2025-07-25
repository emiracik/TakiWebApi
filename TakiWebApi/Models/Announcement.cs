using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TakiWebApi.Models;

[Table("Announcements")]
public class Announcement
{
    [Key]
    public int AnnouncementID { get; set; }

    [Required]
    [StringLength(300)]
    public string Title { get; set; } = string.Empty;

    public string? Content { get; set; }

    public DateTime PublishedAt { get; set; } = DateTime.UtcNow;

    public int? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? DeletedBy { get; set; }

    public DateTime? DeletedDate { get; set; }

    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    public virtual ICollection<AnnouncementRead> AnnouncementReads { get; set; } = new List<AnnouncementRead>();
}
