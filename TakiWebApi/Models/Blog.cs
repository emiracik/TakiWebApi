using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


[Table("Blogs")]
public class Blog
{
    [Key]
    public int BlogID { get; set; }

    [Required]
    [StringLength(300)]
    public string Title { get; set; } = string.Empty;

    public string? Content { get; set; }

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    public DateTime? PublishedAt { get; set; }

    public bool IsPublished { get; set; } = false;

    public int? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? DeletedBy { get; set; }

    public DateTime? DeletedDate { get; set; }

    public bool IsDeleted { get; set; } = false;
}
