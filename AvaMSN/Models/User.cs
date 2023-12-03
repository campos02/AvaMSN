using ReactiveUI;
using SQLite;

namespace AvaMSN.Models;

/// <summary>
/// Represents a user account for storage purposes.
/// </summary>
public class User : ReactiveObject
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
    public string BinarySecret { get; set; } = string.Empty;
    public string Ticket { get; set; } = string.Empty;
    public string TicketToken { get; set; } = string.Empty;
}
