using System;

namespace TakiWebApi.Models;

public class WalletTransaction
{
    public int TransactionID { get; set; }
    public int WalletID { get; set; }
    public int UserID { get; set; }
    public decimal Amount { get; set; }
    public string TransactionType { get; set; } = "";
    public string? Description { get; set; }
    public DateTime TransactionDate { get; set; } // maps to TransactionDate in DB
}
