using System;

namespace TakiWebApi.Models;

public class DriverDocument
{
    public int DocumentID { get; set; }
    public int DriverID { get; set; }
    public string DocumentType { get; set; } = "";
    public string DocumentUrl { get; set; } = "";
    public DateTime UploadedDate { get; set; }

    // Pending | Approved | Rejected (Migrations/003)
    public string Status { get; set; } = "Pending";
    public string? ReviewNote { get; set; }
}
