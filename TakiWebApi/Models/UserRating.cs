using System;

namespace TakiWebApi.Models;

public class UserRating
{
    public int UserRatingID { get; set; }
    public int TripID { get; set; }
    public int RatedUserID { get; set; }
    public int RatedByDriverID { get; set; }
    public decimal Rating { get; set; }
    public string? Comment { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public int? UpdatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public int? DeletedBy { get; set; }
    public DateTime? DeletedDate { get; set; }
    public bool IsDeleted { get; set; }
}
