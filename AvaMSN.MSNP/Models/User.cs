namespace AvaMSN.MSNP.Models;

/// <summary>
/// Represents a user profile, which extends contact data.
/// </summary>
public class User : Contact
{
    public int MBEA { get; set; }
    public int GTC { get; set; }
    public int BLP { get; set; }
}
