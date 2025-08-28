using System;

namespace TakiWebApi.Models;

public class BankInfo
{
    public int BankInfoID { get; set; }
    public int DriverID { get; set; }
    public string BankName { get; set; } = "";
    public string IBAN { get; set; } = "";
    public string AccountNumber { get; set; } = "";
    public DateTime CreatedDate { get; set; }
}
