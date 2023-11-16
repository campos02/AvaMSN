using SQLite;

namespace AvaMSN.Models;

public class DisplayPicture
{
    [PrimaryKey, AutoIncrement]
    public int ID { get; set; }

    public string ContactEmail { get; set; } = string.Empty;
    public byte[] PictureData { get; set; } = [];
    public bool IsUserPicture { get; set; }
}
