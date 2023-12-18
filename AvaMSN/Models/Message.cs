using SQLite;
using System;

namespace AvaMSN.Models;

/// <summary>
/// Represents sent or received messages.
/// </summary>
public class Message
{
    [PrimaryKey, AutoIncrement]
    public int ID { get; set; }

    public string Sender { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;

    public string SenderDisplayName { get; set; } = string.Empty;
    public string RecipientDisplayName { get; set; } = string.Empty;

    public bool Bold { get; set; }
    public bool Italic { get; set; }
    public string Decorations { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;

    public DateTime DateTime { get; set; }

    public bool IsHistory { get; set; }
    public bool IsNudge { get; set; }
}
