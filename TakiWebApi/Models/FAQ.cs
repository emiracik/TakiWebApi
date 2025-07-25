using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TakiWebApi.Models;

[Table("FAQs")]
public class FAQ
{
    [Key]
    public int FAQID { get; set; }

    [Required]
    [StringLength(500)]
    public string Question { get; set; } = string.Empty;

    [Required]
    [StringLength(2000)]
    public string Answer { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public int? SortOrder { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? DeletedBy { get; set; }

    public DateTime? DeletedDate { get; set; }

    public bool IsDeleted { get; set; } = false;
}
