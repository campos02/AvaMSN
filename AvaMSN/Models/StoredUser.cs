using ReactiveUI;
using SQLite;

namespace AvaMSN.Models;

/// <summary>
/// Represents a user account for storage purposes.
/// </summary>
public class StoredUser : ReactiveObject
{
    private string email = string.Empty;

    [PrimaryKey, AutoIncrement]
    public int ID { get; set; }

    public string UserEmail
    {
        get => email;
        set => this.RaiseAndSetIfChanged(ref email, value);
    }

    public string PersonalMessage { get; set; } = string.Empty;
    public byte[] Password { get; set; } = [];
    public byte[] BinarySecret { get; set; } = [];
    public byte[] Ticket { get; set; } = [];
    public byte[] TicketToken { get; set; } = [];
}
