using SQLite;
using System;

namespace AvaMSN.Models;

public class Message
{
    [PrimaryKey, AutoIncrement]
    public int ID { get; set; }

    public string Sender { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;

    public string SenderDisplayName { get; set; } = string.Empty;
    public string RecipientDisplayName { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;

    public DateTime DateTime { get; set; }

    public bool IsHistory { get; set; }
    public bool IsNudge { get; set; }
}
