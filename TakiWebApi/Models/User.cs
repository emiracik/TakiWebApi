using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TakiWebApi.Models;

[Table("Users")]
public class User
{
    [Key]
    public int UserID { get; set; }

    [Required]
    [StringLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Email { get; set; }

    public bool IsActive { get; set; } = true;

    public int? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? DeletedBy { get; set; }

    public DateTime? DeletedDate { get; set; }

    public bool IsDeleted { get; set; } = false;

    // Navigation properties - removed virtual keyword since we're using ADO.NET
    public ICollection<DriverDetail> DriverDetails { get; set; } = new List<DriverDetail>();
    public ICollection<Trip> Trips { get; set; } = new List<Trip>();
    public ICollection<DriverRating> DriverRatings { get; set; } = new List<DriverRating>();
    public ICollection<DriverReview> DriverReviews { get; set; } = new List<DriverReview>();
    public ICollection<UserAddress> UserAddresses { get; set; } = new List<UserAddress>();
    public ICollection<UserCreditCard> UserCreditCards { get; set; } = new List<UserCreditCard>();
    public ICollection<UserNotificationSetting> UserNotificationSettings { get; set; } = new List<UserNotificationSetting>();
    public ICollection<OtpRequest> OtpRequests { get; set; } = new List<OtpRequest>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<SupportTicket> SupportTickets { get; set; } = new List<SupportTicket>();
    public ICollection<AnnouncementRead> AnnouncementReads { get; set; } = new List<AnnouncementRead>();
}
