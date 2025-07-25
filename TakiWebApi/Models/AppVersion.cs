using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TakiWebApi.Models;

[Table("AppVersions")]
public class AppVersion
{
    [Key]
    public int VersionID { get; set; }

    [Required]
    [StringLength(50)]
    public string Platform { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string VersionNumber { get; set; } = string.Empty;

    public bool IsMandatory { get; set; } = false;

    [StringLength(2000)]
    public string? ReleaseNotes { get; set; }

    public DateTime ReleaseDate { get; set; } = DateTime.UtcNow;
}
