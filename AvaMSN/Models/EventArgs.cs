using System;

namespace AvaMSN.Models;

public class NewMessageEventArgs : EventArgs
{
    public Contact? Sender { get; set; }
    public Message? Message { get; set; }
}
