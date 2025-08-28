using System;

namespace TakiWebApi.Models;

public class Invoice
{
    public int InvoiceID { get; set; }
    public int UserID { get; set; }
    public int TripID { get; set; }
    public decimal Amount { get; set; }
    public DateTime InvoiceDate { get; set; }
    public string Status { get; set; } = "Paid";
}
