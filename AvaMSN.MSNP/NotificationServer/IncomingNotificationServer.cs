using AvaMSN.MSNP.Models;
using AvaMSN.MSNP.NotificationServer.Contacts;

namespace AvaMSN.MSNP.NotificationServer;

/// <summary>
/// Handles other Notification Server incoming responses, in this case new Switchboard requests.
/// </summary>
public class IncomingNotificationServer
{
    public IncomingContacts? IncomingContacts { get; set; }
    public User? User { get; internal init; }
    public NotificationServer? Server { get; set; }
    
    public event EventHandler<SwitchboardEventArgs>? SwitchboardChanged;

    /// <summary>
    /// Disconnects in case a request to change servers is received through the XFR command.
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    internal async Task HandleXFR(string response)
    {
        if (response.Split(" ")[2] == "NS")
        {
            string address = response.Split(" ")[3];
            string server = address.Split(":")[0];
            int port = Convert.ToInt32(address.Split(":")[1]);

            await Server!.DisconnectAsync(new DisconnectedEventArgs
            {
                Requested = true,
                RedirectedByTheServer = true,
                NewServerHost = server,
                NewServerPort = port
            });
        }
    }

    /// <summary>
    /// Handles an invitation to a switchboard session by connecting to it.
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    internal async Task<Switchboard.Switchboard> HandleRNG(string response)
    {
        string[] parameters = response.Split(" ");
        string host = parameters[2].Split(":")[0];
        string port = parameters[2].Split(":")[1];
        string sessionID = parameters[1];
        string authString = parameters[4];
        string email = parameters[5];
        string displayName = Uri.UnescapeDataString(parameters[6]);

        Contact? contact = IncomingContacts?.ContactList.FirstOrDefault(c => c.Email == email);
        if (contact == null)
        {
            contact = new Contact
            {
                Email = email
            };

            if (string.IsNullOrEmpty(contact.DisplayName))
                contact.DisplayName = displayName;

            IncomingContacts?.ContactList.Add(contact);
        }

        Switchboard.Switchboard switchboard = new Switchboard.Switchboard
        {
            Host = host,
            Port = Convert.ToInt32(port),
            User = User,
            Contact = contact
        };

        Switchboard.Authentication authentication = new Switchboard.Authentication
        {
            Server = switchboard
        };

        await authentication.SendANS(sessionID, authString);
        switchboard.ContactInSession = true;
        
        SwitchboardChanged?.Invoke(this, new SwitchboardEventArgs
        {
            Switchboard = switchboard
        });

        Server?.Switchboards.Add(switchboard);
        return switchboard;
    }
}
