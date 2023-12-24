using SQLite;

namespace AvaMSN.Models;

/// <summary>
/// Represents user and contact display pictures.
/// </summary>
public class DisplayPicture
{
    [PrimaryKey, AutoIncrement]
    public int ID { get; set; }

    public string ContactEmail { get; set; } = string.Empty;
    public byte[] PictureData { get; set; } = [];
    public string PictureHash { get; set; } = string.Empty;
    public bool IsUserPicture { get; set; }
}
