using System;

namespace TakiWebApi.Models;

public class CreditCard
{
    public int CardID { get; set; }
    public int UserID { get; set; }
    public string CardNumber { get; set; } = "";
    public string CardHolder { get; set; } = "";
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public string CVC { get; set; } = "";
    public bool IsDefault { get; set; }
    public DateTime CreatedDate { get; set; }
}
