namespace AvaMSN.MSNP.Utils;

/// <summary>
/// Represents a user profile, which extends contact data.
/// </summary>
public class Profile : Contact
{
    public int MBEA { get; set; }
    public int GTC { get; set; }
    public int BLP { get; set; }
}
