using System.Text;
using AvaMSN.MSNP.Exceptions;
using AvaMSN.MSNP.Models;

namespace AvaMSN.MSNP.NotificationServer.Contacts;

/// <summary>
/// Provides contact actions such as adding, removing, blocking, unblocking and receiving them from the ABCH.
/// </summary>
public class ContactActions
{
    public NotificationServer? Server { get; init; }
    public User? User => Server?.User;
    public List<Contact> ContactList { get; } = [];
    public IncomingContacts? Incoming { get; private set; }
    
    /// <summary>
    /// Gets ABCH data and sends it to the server in the required order.
    /// </summary>
    /// <returns></returns>
    public async Task SendContactList()
    {
        if (Server == null)
            throw new NullReferenceException("Server is null");
        
        Incoming = new IncomingContacts
        {
            Server = Server,
            ContactList = ContactList
        };
        
        if (Server.Incoming != null)
            Server.Incoming.IncomingContacts = Incoming;

        await ContactService.FindMembership(ContactList);
        await ContactService.ABFindAll(ContactList, Server.User);

        UserProfile.UserProfile userProfile = new UserProfile.UserProfile
        {
            Server = Server
        };

        await userProfile.SendBLP();
        await SendInitialADL(ContactPayloads.InitialListPayload(ContactList));
        await userProfile.SendPRP(userProfile.User?.DisplayName ?? string.Empty);
    }

    /// <summary>
    /// Adds all contacts retrieved from the ABCH on the NS side.
    /// </summary>
    /// <param name="payload">Initial ADL payload.</param>
    /// <returns></returns>
    /// <exception cref="PayloadException">Thrown if payload exceeds max size.</exception>
    private async Task SendInitialADL(string payload)
    {
        if (Server == null)
            throw new NullReferenceException("Server is null");
        
        int length = Encoding.UTF8.GetByteCount(payload);
        if (length > 1160)
            throw new PayloadException("Payload too big");

        Server.TransactionID++;
        string message = $"ADL {Server.TransactionID} {length}\r\n";

        // Send ADL and payload
        await Server.SendAsync(message + payload);

        while (true)
        {
            // Receive ADL
            string response = await Server.ReceiveStringAsync();

            // Break if response contains a command reply and ADL was successful
            if (response.Contains("ADL")
                && response.Contains("OK"))
            {
                // Remove other data before reply
                string adlResponse = response[response.IndexOf("ADL")..];

                if (adlResponse.Split(" ")[1] == Server.TransactionID.ToString())
                    break;
            }

            // Handle normally if response wasn't a reply to this command
            await Server.HandleIncoming(response);
        }
    }

    /// <summary>
    /// Adds a contact on the NS side.
    /// </summary>
    /// <param name="payload">ADL list payload.</param>
    /// <returns></returns>
    /// <exception cref="PayloadException">Thrown if payload exceeds max size.</exception>
    private async Task SendADL(string payload)
    {
        if (Server == null)
            throw new NullReferenceException("Server is null");
        
        int length = Encoding.UTF8.GetByteCount(payload);
        if (length > 1160)
            throw new PayloadException("Payload too big");

        Server.TransactionID++;
        string message = $"ADL {Server.TransactionID} {length}\r\n";

        // Send ADL and payload
        await Server.SendAsync(message + payload);
    }

    /// <summary>
    /// Sends an FQY command, used when adding a contact.
    /// </summary>
    /// <param name="payload">FQY contact payload.</param>
    /// <returns></returns>
    /// <exception cref="PayloadException">Thrown if payload exceeds max size.</exception>
    private async Task SendFQY(string payload)
    {
        if (Server == null)
            throw new NullReferenceException("Server is null");
        
        int length = Encoding.UTF8.GetByteCount(payload);
        if (length > 1160)
            throw new PayloadException("Payload too big");

        Server.TransactionID++;
        string message = $"FQY {Server.TransactionID} {length}\r\n";

        // Send FQY and payload
        await Server.SendAsync(message + payload);
    }

    /// <summary>
    /// Removes a contact on the NS side.
    /// </summary>
    /// <param name="payload">RML list payload.</param>
    /// <returns></returns>
    /// <exception cref="PayloadException">Thrown if payload exceeds max size.</exception>
    private async Task SendRML(string payload)
    {
        if (Server == null)
            throw new NullReferenceException("Server is null");
        
        int length = Encoding.UTF8.GetByteCount(payload);
        if (length > 1160)
            throw new PayloadException("Payload too big");

        Server.TransactionID++;
        string message = $"RML {Server.TransactionID} {length}\r\n";

        // Send RML and payload
        await Server.SendAsync(message + payload);

        while (true)
        {
            // Receive RML
            string response = await Server.ReceiveStringAsync();

            // Break if response is a command reply and RML was successful
            if (response.StartsWith("RML")
                && response.Split(" ")[1] == Server.TransactionID.ToString()
                && response.Contains("OK"))
            {
                break;
            }

            // Handle normally if response wasn't a reply to this command
            await Server.HandleIncoming(response);
        }
    }
    
    /// <summary>
    /// Adds a new contact to the ABCH and NS.
    /// </summary>
    /// <param name="email">Contact email.</param>
    /// <param name="displayName">Contact display name. Will be the same as the email if not given.</param>
    /// <returns></returns>
    public async Task AddContact(string email, string displayName = "")
    {
        if (Server == null)
            throw new NullReferenceException("Server is null");
        
        if (displayName == "")
            displayName = email;

        Contact contact = new Contact
        {
            Type = "Passport",
            Email = email,
            DisplayName = displayName
        };

        // Add to ABCH forward list
        await ContactService.ABContactAdd(contact.Email);
        ContactList.Add(contact);

        // Add to allow list
        string payload = ContactPayloads.ListPayload(contact, new Lists
        {
            Allow = true
        });
        await SendADL(payload);

        // Add to forward list and send FQY
        payload = ContactPayloads.ListPayload(contact, new Lists
        {
            Forward = true
        });
        await SendADL(payload);
        await SendFQY(ContactPayloads.ContactPayload(contact));

        while (true)
        {
            // Receive ADL
            string response = await Server.ReceiveStringAsync();

            // Break if response is a command reply and ADL was successful
            if (response.StartsWith("ADL")
                && response.Split(" ")[1] == (Server.TransactionID - 2).ToString()
                && response.Contains("OK"))
            {
                break;
            }

            // Handle normally if response wasn't a reply to this command
            await Server.HandleIncoming(response);
        }

        // Start receiving incoming commands again
        _ = Server.ReceiveIncomingAsync();
    }

    /// <summary>
    /// Removes a contact from the ABCH and NS forward lists.
    /// </summary>
    /// <param name="email">Contact email.</param>
    /// <returns></returns>
    /// <exception cref="ContactException">Thrown if the provided email is not of any contact.</exception>
    public async Task RemoveContact(string email)
    {
        if (Server == null)
            throw new NullReferenceException("Server is null");
        
        // Remove from forward lists
        Contact contact = ContactList.FirstOrDefault(c => c.Email == email) ?? throw new ContactException("Contact not in list");
        await ContactService.ABContactDelete(contact.Email);

        string payload = ContactPayloads.ListPayload(contact, new Lists
        {
            Forward = true
        });
        await SendRML(payload);
        
        contact.InLists.Forward = false;

        // Start receiving incoming commands again
        _ = Server.ReceiveIncomingAsync();
    }

    /// <summary>
    /// Blocks a contact on the ABCH and NS sides.
    /// </summary>
    /// <param name="email">Contact email.</param>
    /// <returns></returns>
    /// <exception cref="ContactException">Thrown if the provided email is not of any contact.</exception>
    public async Task BlockContact(string email)
    {
        if (Server == null)
            throw new NullReferenceException("Server is null");
        
        Contact contact = ContactList.FirstOrDefault(c => c.Email == email) ?? throw new ContactException("Contact not in list");

        // Remove from allow lists
        await ContactService.DeleteMember("Allow", contact.Email);
        string payload = ContactPayloads.ListPayload(contact, new Lists
        {
            Allow = true
        });
        await SendRML(payload);

        // Add to block lists
        await ContactService.AddMember("Block", contact.Email);
        payload = ContactPayloads.ListPayload(contact, new Lists
        {
            Block = true
        });
        await SendADL(payload);

        // Start receiving incoming commands again
        _ = Server.ReceiveIncomingAsync();
    }

    /// <summary>
    /// Unblocks a contact on the ABCH and NS sides.
    /// </summary>
    /// <param name="email">Contact email.</param>
    /// <returns></returns>
    /// <exception cref="ContactException">Thrown if the provided email is not of any contact.</exception>
    public async Task UnblockContact(string email)
    {
        if (Server == null)
            throw new NullReferenceException("Server is null");
        
        Contact contact = ContactList.FirstOrDefault(c => c.Email == email) ?? throw new ContactException("Contact not in list");

        // Remove from block lists
        await ContactService.DeleteMember("Block", contact.Email);
        string payload = ContactPayloads.ListPayload(contact, new Lists
        {
            Block = true
        });
        await SendRML(payload);

        // Add to allow lists
        await ContactService.AddMember("Allow", contact.Email);
        payload = ContactPayloads.ListPayload(contact, new Lists
        {
            Allow = true
        });
        await SendADL(payload);

        // Start receiving incoming commands again
        _ = Server.ReceiveIncomingAsync();
    }
}