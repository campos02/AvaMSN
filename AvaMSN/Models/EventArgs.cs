using System;
using System.Threading.Tasks;

namespace AvaMSN.Models;

public class ContactEventArgs : EventArgs
{
    public Contact? Contact { get; set; }
}

public class NewMessageEventArgs : ContactEventArgs
{
    public Message? Message { get; set; }
}

public class NotificationEventArgs : NewMessageEventArgs
{
    /// <summary>
    /// Synchronous delay across all windows.
    /// </summary>
    public Task? DelayTask { get; set; }
}