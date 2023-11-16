using AvaMSN.MSNP.Exceptions;

namespace AvaMSN.MSNP;

public partial class NotificationServer : Connection
{
    public async Task ChangeDisplayName()
    {
        await ContactList.ChangeDisplayName();
        await SendPRP();

        _ = ReceiveIncomingAsync();
    }

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

        await ContactList.ABContactAdd(contact.Email);
        ContactList.Contacts.Add(contact);

        string payload = ContactList.ListPayload(contact, new Lists
        {
            Allow = true
        });

        await SendADL(payload);

        payload = ContactList.ListPayload(contact, new Lists
        {
            Forward = true
        });

        await SendADL(payload);
        await SendFQY(ContactList.ContactPayload(contact));

        while (true)
        {
            // Receive First ADL
            string response = await ReceiveStringAsync();

            if (response.StartsWith("ADL")
                && response.Split(" ")[1] == (TransactionID - 2).ToString()
                && response.Contains("OK"))
            {
                break;
            }

            else
            {
                await HandleIncoming(response);
            }
        }

        _ = ReceiveIncomingAsync();
    }

    public async Task RemoveContact(string email)
    {
        Contact? contact = ContactList.Contacts.FirstOrDefault(c => c.Email == email) ?? throw new ContactException("Contact not in list");
        await ContactList.ABContactDelete(contact.Email);

        string payload = ContactList.ListPayload(contact, new Lists
        {
            Forward = true
        });

        await SendRML(payload);
        contact.InLists.Forward = false;

        _ = ReceiveIncomingAsync();
    }

    public async Task BlockContact(string email)
    {
        Contact? contact = ContactList.Contacts.FirstOrDefault(c => c.Email == email) ?? throw new ContactException("Contact not in list");

        await ContactList.AddMember("Block", contact.Email);

        string payload = ContactList.ListPayload(contact, new Lists
        {
            Block = true
        });

        await SendInitialADL(payload);

        if (contact.InLists.Allow)
        {
            await ContactList.DeleteMember("Allow", contact.Email);

            payload = ContactList.ListPayload(contact, new Lists
            {
                Allow = true
            });

            await SendRML(payload);
        }

        _ = ReceiveIncomingAsync();
    }

    public async Task UnblockContact(string email)
    {
        Contact? contact = ContactList.Contacts.FirstOrDefault(c => c.Email == email) ?? throw new ContactException("Contact not in list");

        await ContactList.DeleteMember("Block", contact.Email);

        string payload = ContactList.ListPayload(contact, new Lists
        {
            Block = true
        });

        await SendRML(payload);

        _ = ReceiveIncomingAsync();
    }
}
