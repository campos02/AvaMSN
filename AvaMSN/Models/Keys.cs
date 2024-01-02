namespace AvaMSN.Models;

public class Keys
{
    public string UserEmail { get; set; } = string.Empty;

    // Ticket
    public byte[] Key1 { get; set; } = [];
    public byte[] IV1 { get; set; } = [];

    // Password
    public byte[] Key2 { get; set; } = [];
    public byte[] IV2 { get; set; } = [];

    // Ticket token
    public byte[] Key3 { get; set; } = [];
    public byte[] IV3 { get; set; } = [];

    // Binary secret
    public byte[] Key4 { get; set; } = [];
    public byte[] IV4 { get; set; } = [];
}
