using AvaMSN.MSNP.Exceptions;
using AvaMSN.MSNP.XML.SerializableClasses;
using System.Text;
using System.Xml.Serialization;
using AvaMSN.MSNP.Utils;

namespace AvaMSN.MSNP;

public partial class NotificationServer : Connection
{
    public event EventHandler<PresenceEventArgs>? PresenceChanged;
    public event EventHandler<PersonalMessageEventArgs>? PersonalMessageChanged;
    public event EventHandler<SwitchboardEventArgs>? SwitchboardChanged;

    /// <summary>
    /// Handles responses that aren't the result of a command.
    /// </summary>
    /// <param name="responseBytes">Incoming binary response.</param>
    /// <returns></returns>
    protected override async Task HandleIncoming(byte[] responseBytes)
    {
        string response = Encoding.UTF8.GetString(responseBytes);
        await HandleIncoming(response);
    }

    /// <summary>
    /// Handles responses that aren't the result of a command.
    /// </summary>
    /// <param name="response">Incoming response.</param>
    /// <returns></returns>
    private async Task HandleIncoming(string response)
    {
        string command = response.Split(" ")[0];

        await (command switch
        {
            "ILN" => HandleILN(response),
            "NLN" => Task.Run(() => HandleNLN(response)),
            "FLN" => Task.Run(() => HandleFLN(response)),
            "UBX" => HandleUBX(response),
            "RNG" => HandleRNG(response),
            "OUT" => DisconnectAsync(requested: false),
            _ => Task.CompletedTask
        });
    }

    /// <summary>
    /// Handles an initial contact presence response.
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    /// <exception cref="ContactException">Thrown if the response is of a non existant contact.</exception>
    private async Task HandleILN(string response)
    {
        string[] parameters = response.Split(" ");

        Contact? contact = ContactService.Contacts.FirstOrDefault(c => c.Email == parameters[3]) ?? throw new ContactException("Contact does not exist");
        contact.Presence = parameters[2];
        contact.DisplayName = Uri.UnescapeDataString(parameters[5]);
        
        try
        {
            contact.DisplayPictureObject = Uri.UnescapeDataString(parameters[7]);
            contact.DisplayPictureObject = contact.DisplayPictureObject.Replace("\r\n", "");
            contact.DisplayPictureObject = contact.DisplayPictureObject.Remove(contact.DisplayPictureObject.LastIndexOf("/>") + "/>".Length);

            if (contact.DisplayPictureObject == "0")
                contact.DisplayPictureObject = null;
        }
        catch (IndexOutOfRangeException)
        {
            contact.DisplayPictureObject = null;
        }

        bool hasDisplayPicture = true;
        XmlSerializer serializer = new XmlSerializer(typeof(msnobj));

        if (contact.DisplayPictureObject != null)
        {
            using (StringReader reader = new StringReader(contact.DisplayPictureObject))
            {
                msnobj? msnobj = (msnobj?)serializer.Deserialize(reader);
                contact.DisplayPictureHash = msnobj?.SHA1D;

                if (msnobj == null || msnobj.Size <= 0)
                {
                    hasDisplayPicture = false;
                }
            }
        }

        PresenceChanged?.Invoke(this, new PresenceEventArgs()
        {
            Email = contact.Email,
            Presence = contact.Presence,
            HasDisplayPicture = hasDisplayPicture
        });

        // Handle other responses if found
        string[] responses = response.Split("\r\n");
        string command = response.Replace(responses[0] + "\r\n", "");

        if (command != "")
            await HandleIncoming(command);
    }

    /// <summary>
    /// Handles a contact presence change.
    /// </summary>
    /// <param name="response"></param>
    /// <exception cref="ContactException">Thrown if the response is of a non existant contact.</exception>
    private void HandleNLN(string response)
    {
        string[] parameters = response.Split(" ");

        Contact? contact = ContactService.Contacts.FirstOrDefault(c => c.Email == parameters[2]) ?? throw new ContactException("Contact does not exist");
        contact.Presence = parameters[1];
        contact.DisplayName = Uri.UnescapeDataString(parameters[4]);

        try
        {
            contact.DisplayPictureObject = Uri.UnescapeDataString(parameters[6]);
            contact.DisplayPictureObject = contact.DisplayPictureObject.Replace("\r\n", "");

            if (contact.DisplayPictureObject == "0")
                contact.DisplayPictureObject = null;
        }
        catch (IndexOutOfRangeException)
        {
            contact.DisplayPictureObject = null;
        }

        bool hasDisplayPicture = true;
        XmlSerializer serializer = new XmlSerializer(typeof(msnobj));

        if (contact.DisplayPictureObject != null)
        {
            using (StringReader reader = new StringReader(contact.DisplayPictureObject))
            {
                msnobj? msnobj = (msnobj?)serializer.Deserialize(reader);
                contact.DisplayPictureHash = msnobj?.SHA1D;

                if (msnobj == null || msnobj.Size <= 0)
                {
                    hasDisplayPicture = false;
                }
            }
        }

        PresenceChanged?.Invoke(this, new PresenceEventArgs()
        {
            Email = contact.Email,
            Presence = contact.Presence,
            HasDisplayPicture = hasDisplayPicture
        });
    }

    /// <summary>
    /// Handles a contact going offline.
    /// </summary>
    /// <param name="response"></param>
    /// <exception cref="ContactException">Thrown if the response is of a non existant contact.</exception>
    private async void HandleFLN(string response)
    {
        string[] parameters = response.Split(" ");

        Contact? contact = ContactService.Contacts.FirstOrDefault(c => c.Email == parameters[1]) ?? throw new ContactException("Contact does not exist");
        contact.Presence = PresenceStatus.Offline;

        List<Switchboard> switchboards = Switchboards.Where(sb => sb.Contact.Email == contact.Email && sb.Connected).ToList();
        foreach (Switchboard switchboard in switchboards)
        {
            await switchboard.DisconnectAsync();
        }

        PresenceChanged?.Invoke(this, new PresenceEventArgs()
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
    /// <exception cref="ContactException">Thrown if the response is of a non existant contact.</exception>
    private async Task HandleUBX(string response)
    {
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

            Contact? contact = ContactService.Contacts.FirstOrDefault(c => c.Email == parameters[1]);

            if (contact == null || data == null)
                throw new ContactException("Contact does not exist");

            contact.PersonalMessage = data.PSM;

            PersonalMessageChanged?.Invoke(this, new PersonalMessageEventArgs()
            {
                Email = contact.Email,
                PersonalMessage = contact.PersonalMessage
            });
        }

        // Handle other responses if found
        string commandAndPayload = response.Replace(responses[0] + "\r\n" + payload, "");
        if (commandAndPayload != "")
            await HandleIncoming(commandAndPayload);
    }

    /// <summary>
    /// Handles an invitation to a switchboard session by connecting to it.
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    private async Task HandleRNG(string response)
    {
        string[] parameters = response.Split(" ");

        string host = parameters[2].Split(":")[0];
        string port = parameters[2].Split(":")[1];
        
        string sessionID = parameters[1];
        string authString = parameters[4];
        string email = parameters[5];
        string displayName = Uri.UnescapeDataString(parameters[6]);

        Contact? contact = ContactService.Contacts.FirstOrDefault(c => c.Email == email);
        if (contact == null)
        {
            contact = new Contact()
            {
                Email = email
            };

            if (string.IsNullOrEmpty(contact.DisplayName))
                contact.DisplayName = displayName;

            ContactService.Contacts.Add(contact);
        }

        Switchboard switchboard = new Switchboard()
        {
            Host = host,
            Port = Convert.ToInt32(port),
            Profile = ContactService.Profile,
            Contact = contact
        };

        await switchboard.SendANS(sessionID, authString);
        SwitchboardChanged?.Invoke(this, new SwitchboardEventArgs
        {
            Switchboard = switchboard
        });

        Switchboards.Add(switchboard);
    }
}
