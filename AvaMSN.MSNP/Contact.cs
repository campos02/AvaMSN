namespace AvaMSN.MSNP;

public class Contact
{
    // Presence info
    public string Presence { get; set; } = string.Empty;
    public string PersonalMessage { get; set; } = string.Empty;

    // List info
    public bool InForward { get; set; }
    public bool InAllow { get; set; }
    public bool InBlock { get; set; }
    public bool InReverse { get; set; }
    public bool InPending { get; set; }

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

    public bool SetMembershipList(string memberRole) => memberRole switch
    {
        "Allow" => InAllow = true,
        "Block" => InBlock = true,
        "Reverse" => InReverse = true,
        "Pending" => InPending = true,
        _ => throw new ArgumentException("Argument is not any member role")
    };

    public int ListsNumber()
    {
        int inForward = InForward ? (int)ListNumbers.Forward : 0;
        int inAllow = InAllow ? (int)ListNumbers.Allow : 0;
        int inBlock = InBlock ? (int)ListNumbers.Block : 0;
        int inReverse = InReverse ? (int)ListNumbers.Reverse : 0;
        int inPending = InPending ? (int)ListNumbers.Pending : 0;

        // Respective value of each list if contact is in it and 0 if it's not
        int listNumber = (inForward & (int)ListNumbers.Forward)
                       + (inAllow   & (int)ListNumbers.Allow)
                       + (inBlock   & (int)ListNumbers.Block)
                       + (inReverse & (int)ListNumbers.Reverse)
                       + (inPending & (int)ListNumbers.Pending);

        return listNumber;
    }
}
