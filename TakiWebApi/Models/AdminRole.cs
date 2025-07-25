using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TakiWebApi.Models;

[Table("AdminRoles")]
public class AdminRole
{
    [Key]
    public int RoleID { get; set; }

    [Required]
    [StringLength(100)]
    public string RoleName { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    // Navigation properties - removed virtual keyword since we're using ADO.NET
    public ICollection<Admin> Admins { get; set; } = new List<Admin>();
}
