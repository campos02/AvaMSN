namespace AvaMSN.Models;

/// <summary>
/// Represents client settings.
/// </summary>
public class Settings
{
    public string Server { get; set; } = string.Empty;
    public bool SaveMessagingHistory { get; set; }
}
