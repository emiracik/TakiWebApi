using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TakiWebApi.Models;

[Table("Admins")]
public class Admin
{
    [Key]
    public int AdminID { get; set; }

    [Required]
    [StringLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [StringLength(20)]
    public string? Phone { get; set; }

    [Required]
    [StringLength(500)]
    public string PasswordHash { get; set; } = string.Empty;

    public int? RoleID { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? LastLoginDate { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? DeletedBy { get; set; }

    public DateTime? DeletedDate { get; set; }

    public bool IsDeleted { get; set; } = false;

    // Navigation properties - removed virtual keyword since we're using ADO.NET
    [ForeignKey("RoleID")]
    public AdminRole? AdminRole { get; set; }

    public ICollection<SupportTicket> ResolvedTickets { get; set; } = new List<SupportTicket>();
}
