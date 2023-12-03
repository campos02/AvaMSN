namespace AvaMSN.Models;

/// <summary>
/// Represents account data, which extends contact data.
/// </summary>
public class Profile : Contact
{
    public int UserID { get; set; }
    public byte[]? DisplayPictureData { get; set; }
}
