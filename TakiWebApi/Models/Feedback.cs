using System;

namespace TakiWebApi.Models;

public class Feedback
{
    public int FeedbackID { get; set; }
    public int? UserID { get; set; }
    public int? DriverID { get; set; }
    public int? TripID { get; set; }
    public decimal Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedDate { get; set; }
}
