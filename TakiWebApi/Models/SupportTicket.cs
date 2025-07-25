using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TakiWebApi.Models;

[Table("SupportTickets")]
public class SupportTicket
{
    [Key]
    public int TicketID { get; set; }

    public int? UserID { get; set; }

    [StringLength(200)]
    public string? Subject { get; set; }

    [StringLength(2000)]
    public string? Message { get; set; }

    [StringLength(50)]
    public string Status { get; set; } = "Open";

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public int? ResolvedBy { get; set; }

    public DateTime? ResolvedDate { get; set; }

    // Navigation properties
    [ForeignKey("UserID")]
    public virtual User? User { get; set; }

    [ForeignKey("ResolvedBy")]
    public virtual Admin? ResolvedByAdmin { get; set; }
}
