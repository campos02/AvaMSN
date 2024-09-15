using System.Text;
using System.Xml.Serialization;
using AvaMSN.MSNP.Exceptions;
using AvaMSN.MSNP.Models;
using AvaMSN.MSNP.XML.SerializableClasses;

namespace AvaMSN.MSNP.NotificationServer.Contacts;

/// <summary>
/// Handles incoming contact list responses.
/// </summary>
public class IncomingContacts
{
    public NotificationServer? Server { get; init; }
    public List<Contact> ContactList { get; init; } = [];
    
    public event EventHandler<PresenceEventArgs>? PresenceChanged;
    public event EventHandler<PersonalMessageEventArgs>? PersonalMessageChanged;

    /// <summary>
    /// Handles responses that aren't the result of a command.
    /// </summary>
    /// <param name="response">Incoming response.</param>
    /// <returns></returns>
    internal async Task HandleIncoming(string response)
    {
        string command = response.Split(" ")[0];
        await (command switch
        {
            "ILN" => HandlePresence(response),
            "NLN" => HandlePresence(response),
            "FLN" => HandleFLN(response),
            "UBX" => HandleUBX(response),
            _ => Task.CompletedTask
        });
    }
    
    /// <summary>
    /// Handles a contact presence change.
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    /// <exception cref="ContactException">Thrown if response is of a non-existent contact.</exception>
    private async Task HandlePresence(string response)
    {
        if (Server == null)
            throw new NullReferenceException("Server is null");
        
        string[] parameters = response.Split(" ");
        int firstIndex = 0;
        
        // NLN response has one parameter less
        if (parameters[0] == "NLN")
            firstIndex = -1;
        
        Contact contact = ContactList.FirstOrDefault(c => c.Email == parameters[firstIndex + 3]) ?? throw new ContactException("Contact does not exist");
        contact.Presence = parameters[firstIndex + 2];
        contact.DisplayName = Uri.UnescapeDataString(parameters[firstIndex + 5]);
        
        try
        {
            contact.DisplayPictureObject = Uri.UnescapeDataString(parameters[firstIndex + 7]);
            contact.DisplayPictureObject = contact.DisplayPictureObject.Replace("\r\n", "");
            contact.DisplayPictureObject = contact.DisplayPictureObject.Remove(contact.DisplayPictureObject.LastIndexOf("/>") + "/>".Length);

            if (contact.DisplayPictureObject == "0")
                contact.DisplayPictureObject = null;
        }
        catch (IndexOutOfRangeException)
        {
            contact.DisplayPictureObject = null;
        }

        bool hasDisplayPicture = false;
        XmlSerializer serializer = new XmlSerializer(typeof(msnobj));

        if (contact.DisplayPictureObject != null)
        {
            using StringReader reader = new StringReader(contact.DisplayPictureObject);
            msnobj? msnobj = (msnobj?)serializer.Deserialize(reader);
            contact.DisplayPictureHash = msnobj?.SHA1D;
            
            if (msnobj != null && msnobj.Size > 0)
                hasDisplayPicture = true;
        }

        PresenceChanged?.Invoke(this, new PresenceEventArgs
        {
            Email = contact.Email,
            Presence = contact.Presence,
            HasDisplayPicture = hasDisplayPicture,
            DisplayPictureHash = contact.DisplayPictureHash
        });

        // Handle other responses if found
        string[] responses = response.Split("\r\n");
        string command = response.Replace(responses[0] + "\r\n", "");

        if (command != "")
            await Server.HandleIncoming(command);
    }

    /// <summary>
    /// Handles a contact going offline.
    /// </summary>
    /// <param name="response"></param>
    /// <exception cref="ContactException">Thrown if response is of a non-existent contact.</exception>
    private async Task HandleFLN(string response)
    {
        string[] parameters = response.Split(" ");
        Contact contact = ContactList.FirstOrDefault(c => c.Email == parameters[1]) ?? throw new ContactException("Contact does not exist");
        contact.Presence = PresenceStatus.Offline;

        List<Switchboard.Switchboard>? contactSwitchboards = Server?.Switchboards.Where(sb => sb.Contact?.Email == contact.Email).ToList();
        if (contactSwitchboards != null)
            foreach (Switchboard.Switchboard? switchboard in contactSwitchboards)
                await switchboard.DisconnectAsync();

        PresenceChanged?.Invoke(this, new PresenceEventArgs
        {
            Email = contact.Email,
            Presence = contact.Presence
        });
    }

    /// <summary>
    /// Handles a contact setting or changing its personal message.
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    /// <exception cref="ContactException">Thrown if response is of a non-existent contact.</exception>
    private async Task HandleUBX(string response)
    {
        if (Server == null)
            throw new NullReferenceException("Server is null");
        
        string[] responses = response.Split("\r\n");
        string[] parameters = responses[0].Split(" ");
        int length = Convert.ToInt32(parameters[3]);

        // Get payload and deserialize it
        byte[] totalBytes = Encoding.UTF8.GetBytes(responses[1]);
        byte[] payloadBytes = new Span<byte>(totalBytes, 0, length).ToArray();
        string payload = Encoding.UTF8.GetString(payloadBytes);

        XmlSerializer serializer = new XmlSerializer(typeof(Data));
        using (StringReader reader = new StringReader(payload))
        {
            var data = (Data?)serializer.Deserialize(reader);

            Contact? contact = ContactList.FirstOrDefault(c => c.Email == parameters[1]);
            if (contact == null || data == null)
                throw new ContactException("Contact does not exist");

            contact.PersonalMessage = data.PSM;
            PersonalMessageChanged?.Invoke(this, new PersonalMessageEventArgs
            {
                Email = contact.Email,
                PersonalMessage = contact.PersonalMessage
            });
        }

        // Handle other responses if found
        string commandAndPayload = response.Replace(responses[0] + "\r\n" + payload, "");
        if (commandAndPayload != "")
            await Server.HandleIncoming(commandAndPayload);
    }
}