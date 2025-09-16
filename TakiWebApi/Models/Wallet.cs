using System;

namespace TakiWebApi.Models
{
    public class Wallet
    {
        public int WalletID { get; set; }
        public int UserID { get; set; }
        public decimal Balance { get; set; }
        public decimal TotalIn { get; set; }
        public decimal TotalOut { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
