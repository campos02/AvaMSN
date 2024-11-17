using System.Text;
using AvaMSN.MSNP.Exceptions;
using AvaMSN.MSNP.Models;
using Serilog;

namespace AvaMSN.MSNP.NotificationServer;

/// <summary>
/// Represents a connection to the Notification Server (NS).
/// </summary>
public class NotificationServer : Connection
{
    public User User { get; init; } = new User();
    public static string Protocol => "MSNP15";
    public IncomingNotificationServer? Incoming { get; private set; }
    internal List<Switchboard.Switchboard> Switchboards { get; set; } = [];

    public override async Task Connect()
    {
        await base.Connect();
        Incoming = new IncomingNotificationServer
        {
            User = User,
            Server = this
        };
        
        ContactService.SharingServiceUrl = $"https://{Host}/abservice/SharingService.asmx";
        ContactService.ABServiceUrl = $"https://{Host}/abservice/abservice.asmx";
        Log.Information("Connected to NS {Server} on port {Port}", Host, Port);
    }

    /// <summary>
    /// Requests a new switchboard session, connects to it, then invites a contact.
    /// </summary>
    /// <param name="contact">Contact to invite for the session.</param>
    /// <returns>New switchboard session.</returns>
    public async Task<Switchboard.Switchboard> SendXFR(Contact contact)
    {
        // Send XFR
        string message = $"XFR {TransactionID} SB\r\n";
        await SendAsync(message);

        string response;
        while (true)
        {
            // Receive XFR
            response = await ReceiveStringAsync();

            // Break if response is a command reply
            if (response.StartsWith("XFR")
                && response.Split(" ")[1] == TransactionID.ToString())
            {
                break;
            }
        }

        // Start handling incoming commands again
        _ = ReceiveIncomingAsync();

        string[] parameters = response.Split(" ");
        string host = parameters[3].Split(":")[0];
        string port = parameters[3].Split(":")[1];
        string authString = parameters[5];

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
        
        // Authenticate and invite contact
        await authentication.SendUSR(authString);
        await authentication.SendCAL();

        Switchboards.Add(switchboard);
        return switchboard;
    }

    /// <summary>
    /// Searches for a contact using its email then calls the contact parameter overload.
    /// </summary>
    /// <param name="contactEmail">Contact email.</param>
    /// <param name="contacts">Contact list.</param>
    /// <returns>New switchboard session.</returns>
    public async Task<Switchboard.Switchboard> SendXFR(string contactEmail, List<Contact> contacts)
    {
        Contact? contact = contacts.LastOrDefault(c => c.Email == contactEmail);
        if (contact != null)
            return await SendXFR(contact);
        
        throw new ContactException("Contact does not exist");
    }

    /// <summary>
    /// Handles responses that aren't the result of a command.
    /// </summary>
    /// <param name="responseBytes">Incoming binary response.</param>
    /// <returns></returns>
    internal override async Task HandleIncoming(byte[] responseBytes)
    {
        string response = Encoding.UTF8.GetString(responseBytes);
        await HandleIncoming(response);
    }

    /// <summary>
    /// Handles responses that aren't the result of a command.
    /// </summary>
    /// <param name="response">Incoming response.</param>
    /// <returns></returns>
    internal async Task HandleIncoming(string response)
    {
        await Incoming!.IncomingContacts!.HandleIncoming(response);
        string command = response.Split(" ")[0];
        
        await (command switch
        {
            "RNG" => Incoming!.HandleRNG(response),
            "XFR" => Incoming!.HandleXFR(response),
            "OUT" => DisconnectAsync(new DisconnectedEventArgs { Requested = false }),
            _ => Task.CompletedTask
        });
    }

    /// <summary>
    /// Sends a disconnection command and invokes the Disconnected event for the NS and every connected SB.
    /// </summary>
    /// <param name="eventArgs">Disconnection event arguments.</param>
    /// <returns></returns>
    public override async Task DisconnectAsync(DisconnectedEventArgs? eventArgs = null)
    {
        await base.DisconnectAsync(eventArgs);
        foreach (Switchboard.Switchboard switchboard in Switchboards)
            await switchboard.DisconnectAsync();
    }
}