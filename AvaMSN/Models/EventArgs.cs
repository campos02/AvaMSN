using System;

namespace AvaMSN.Models;

public class NewMessageEventArgs : EventArgs
{
    public Contact? Contact { get; set; }
    public Message? Message { get; set; }
}