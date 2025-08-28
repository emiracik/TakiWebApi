using System;

namespace TakiWebApi.Models;

public class Message
{
    public int MessageID { get; set; }
    public int SenderID { get; set; }
    public int ReceiverID { get; set; }
    public string Content { get; set; } = "";
    public DateTime SentAt { get; set; }
    public bool IsRead { get; set; }
}
