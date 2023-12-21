using AvaMSN.MSNP.Messages;

namespace AvaMSN.MSNP;

public class DisconnectedEventArgs : EventArgs
{
    public bool Requested { get; set; }
}

public class ContactEventArgs : EventArgs
{
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}

public class PresenceEventArgs : ContactEventArgs
{
    public string Presence { get; set; } = string.Empty;
    public bool HasDisplayPicture { get; set; } = true;
}

public class PersonalMessageEventArgs : ContactEventArgs
{
    public string PersonalMessage { get; set; } = string.Empty;
}

public class MessageEventArgs : ContactEventArgs
{
    public bool InContactList { get; set; }
    public bool TypingUser { get; set; }
    public bool IsNudge { get; set; }
    public TextPlain? Message { get; set; }
}

public class SwitchboardEventArgs
{
    public Switchboard? Switchboard { get; set; }
}

public class DisplayPictureEventArgs : ContactEventArgs
{
    public byte[]? DisplayPicture { get; set; }
}