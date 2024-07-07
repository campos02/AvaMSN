using System;
using System.Threading.Tasks;
using AvaMSN.MSNP.NotificationServer.Contacts;
using AvaMSN.MSNP.NotificationServer.UserProfile;

namespace AvaMSN.Models;

public class LoggedInEventArgs : EventArgs
{
    public ContactActions? ContactActions { get; set; }
    public UserProfile? UserProfile { get; set; }
}

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