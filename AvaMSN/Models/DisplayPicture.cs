using SQLite;

namespace AvaMSN.Models;

/// <summary>
/// Represents a user's and contact's display picture.
/// </summary>
public class DisplayPicture
{
    [PrimaryKey, AutoIncrement]
    public int ID { get; set; }

    public string ContactEmail { get; set; } = string.Empty;
    public byte[] PictureData { get; set; } = [];
    public bool IsUserPicture { get; set; }
}
