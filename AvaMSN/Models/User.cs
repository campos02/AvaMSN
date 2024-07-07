namespace AvaMSN.Models;

/// <summary>
/// Represents user account data, which extends contact data.
/// </summary>
public class User : Contact
{
    public int UserID { get; set; }
    public byte[]? DisplayPictureData { get; set; }
}
