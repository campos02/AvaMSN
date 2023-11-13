namespace AvaMSN.MSNP;

public class Contact
{
    // Presence info
    public string Presence { get; set; } = string.Empty;
    public string PersonalMessage { get; set; } = string.Empty;
    public byte[] DisplayPicture { get; set; } = Array.Empty<byte>();
    public string DisplayPictureObject { get; set; } = string.Empty;
    public bool HasJoinedSession { get; set; }

    // List info
    public Lists InLists { get; set; } = new();

    // Membership and AB info
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public string ContactID { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;

    public bool IsEmailHidden { get; set; }

    public DateTime MembershipLastChanged { get; set; }

    public DateTime JoinedDate { get; set; }
    public DateTime ExpirationDate { get; set; }

    public bool IsFavorite { get; set; }
    public bool HasSpace { get; set; }
    public bool IsPrivate { get; set; }

    public string Gender { get; set; } = string.Empty;
    public string TimeZone { get; set; } = string.Empty;

    public DateTime ABLastChanged { get; set; }
    public DateTime Birthdate { get; set; }
}
