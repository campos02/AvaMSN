namespace AvaMSN.Models;

/// <summary>
/// Represents a presence status.
/// </summary>
public class Presence
{
    public string Status { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;
}
