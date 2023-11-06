namespace AvaMSN.MSNP;

/// <summary>
/// Contains what lists a contact is in, as well as list functions
/// </summary>
public class Lists
{
    public bool Forward { get; set; }
    public bool Allow { get; set; }
    public bool Block { get; set; }
    public bool Reverse { get; set; }
    public bool Pending { get; set; }

    public bool SetMembershipLists(string memberRole) => memberRole switch
    {
        "Allow" => Allow = true,
        "Block" => Block = true,
        "Reverse" => Reverse = true,
        "Pending" => Pending = true,
        _ => throw new ArgumentException("Argument is not any member role")
    };

    public int ListsNumber()
    {
        // Respective value of a list if in it or 0 if not
        int forward = Forward ? (int)ListNumbers.Forward : 0;
        int allow = Allow ? (int)ListNumbers.Allow : 0;
        int block = Block ? (int)ListNumbers.Block : 0;

        int listNumber = (forward & (int)ListNumbers.Forward)
                       + (allow & (int)ListNumbers.Allow)
                       + (block & (int)ListNumbers.Block);

        return listNumber;
    }
}
