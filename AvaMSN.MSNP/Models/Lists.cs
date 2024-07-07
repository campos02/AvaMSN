namespace AvaMSN.MSNP.Models;

/// <summary>
/// Contains what lists a contact is in, as well as list functions.
/// </summary>
public class Lists
{
    // "Belongs to list" properties
    public bool Forward { get; set; }
    public bool Allow { get; set; }
    public bool Block { get; set; }
    public bool Reverse { get; set; }
    public bool Pending { get; set; }

    /// <summary>
    /// Sets whether a contact belongs to the provided list to true. Uses member roles returned from the contact service.
    /// </summary>
    /// <param name="memberRole">Member role returned from contact service.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">Thrown if the argument is not a member role.</exception>
    public bool SetMembershipList(string memberRole) => memberRole switch
    {
        "Allow" => Allow = true,
        "Block" => Block = true,
        "Reverse" => Reverse = true,
        "Pending" => Pending = true,
        _ => throw new ArgumentException("Argument is not any member role")
    };

    /// <summary>
    /// Returns a combined list number from forward, allow and block lists.
    /// </summary>
    /// <returns>Combined lists number.</returns>
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
