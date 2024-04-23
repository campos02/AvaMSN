using AvaMSN.MSNP.Exceptions;
using AvaMSN.MSNP.Utils;

namespace AvaMSN.MSNP;

public partial class NotificationServer : Connection
{
    /// <summary>
    /// Change the user's display name on the ABCH and NS sides.
    /// </summary>
    /// <returns></returns>
    public async Task ChangeDisplayName()
    {
        await ContactList.ChangeDisplayName();
        await SendPRP();

        // Start receiving incoming commands again
        _ = ReceiveIncomingAsync();
    }

    /// <summary>
    /// Adds a new contact to the ABCH and NS.
    /// </summary>
    /// <param name="email">Contact email.</param>
    /// <param name="displayName">Contact display name. Will be the same as the email if not given.</param>
    /// <returns></returns>
    public async Task AddContact(string email, string displayName = "")
    {
        if (displayName == "")
            displayName = email;

        Contact contact = new Contact()
        {
            Type = "Passport",
            Email = email,
            DisplayName = displayName
        };

        // Add to ABCH forward list
        await ContactList.ABContactAdd(contact.Email);
        ContactList.Contacts.Add(contact);

        // Add to allow list
        string payload = ContactService.ListPayload(contact, new Lists
        {
            Allow = true
        });
        await SendADL(payload);

        // Add to forward list and send FQY
        payload = ContactService.ListPayload(contact, new Lists
        {
            Forward = true
        });
        await SendADL(payload);
        await SendFQY(ContactService.ContactPayload(contact));

        while (true)
        {
            // Receive ADL
            string response = await ReceiveStringAsync();

            // Break if response is a command reply and ADL was successful
            if (response.StartsWith("ADL")
                && response.Split(" ")[1] == (TransactionID - 2).ToString()
                && response.Contains("OK"))
            {
                break;
            }

            // Handle normally if response wasn't a reply to this command
            else
            {
                await HandleIncoming(response);
            }
        }

        // Start receiving incoming commands again
        _ = ReceiveIncomingAsync();
    }

    /// <summary>
    /// Removes a contact from the ABCH and NS forward lists.
    /// </summary>
    /// <param name="email">Contact email.</param>
    /// <returns></returns>
    /// <exception cref="ContactException">Thrown if the provided email is not of any contact.</exception>
    public async Task RemoveContact(string email)
    {
        // Remove from forward lists
        Contact? contact = ContactList.Contacts.FirstOrDefault(c => c.Email == email) ?? throw new ContactException("Contact not in list");
        await ContactList.ABContactDelete(contact.Email);

        string payload = ContactService.ListPayload(contact, new Lists
        {
            Forward = true
        });
        await SendRML(payload);
        
        contact.InLists.Forward = false;

        // Start receiving incoming commands again
        _ = ReceiveIncomingAsync();
    }

    /// <summary>
    /// Blocks a contact on the ABCH and NS sides.
    /// </summary>
    /// <param name="email">Contact email.</param>
    /// <returns></returns>
    /// <exception cref="ContactException">Thrown if the provided email is not of any contact.</exception>
    public async Task BlockContact(string email)
    {
        Contact? contact = ContactList.Contacts.FirstOrDefault(c => c.Email == email) ?? throw new ContactException("Contact not in list");

        // Remove from allow lists
        await ContactList.DeleteMember("Allow", contact.Email);
        string payload = ContactService.ListPayload(contact, new Lists
        {
            Allow = true
        });
        await SendRML(payload);

        // Add to block lists
        await ContactList.AddMember("Block", contact.Email);
        payload = ContactService.ListPayload(contact, new Lists
        {
            Block = true
        });
        await SendADL(payload);

        // Start receiving incoming commands again
        _ = ReceiveIncomingAsync();
    }

    /// <summary>
    /// Unblocks a contact on the ABCH and NS sides.
    /// </summary>
    /// <param name="email">Contact email.</param>
    /// <returns></returns>
    /// <exception cref="ContactException">Thrown if the provided email is not of any contact.</exception>
    public async Task UnblockContact(string email)
    {
        Contact? contact = ContactList.Contacts.FirstOrDefault(c => c.Email == email) ?? throw new ContactException("Contact not in list");

        // Remove from block lists
        await ContactList.DeleteMember("Block", contact.Email);
        string payload = ContactService.ListPayload(contact, new Lists
        {
            Block = true
        });
        await SendRML(payload);

        // Add to allow lists
        await ContactList.AddMember("Allow", contact.Email);
        payload = ContactService.ListPayload(contact, new Lists
        {
            Allow = true
        });
        await SendADL(payload);

        // Start receiving incoming commands again
        _ = ReceiveIncomingAsync();
    }
}
