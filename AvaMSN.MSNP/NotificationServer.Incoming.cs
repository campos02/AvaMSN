using AvaMSN.MSNP.Exceptions;
using AvaMSN.MSNP.XML.SerializableClasses;
using System.Text;
using System.Xml.Serialization;

namespace AvaMSN.MSNP;

public partial class NotificationServer : Connection
{
    public event EventHandler<PresenceEventArgs>? PresenceChanged;
    public event EventHandler<PersonalMessageEventArgs>? PersonalMessageChanged;
    public event EventHandler<SwitchboardEventArgs>? SwitchboardChanged;

    protected override async Task HandleIncoming(byte[] responseBytes)
    {
        string response = Encoding.UTF8.GetString(responseBytes);
        string command = response.Split(" ")[0];

        await (command switch
        {
            "ILN" => HandleILN(response),
            "NLN" => Task.Run(() => HandleNLN(response)),
            "FLN" => Task.Run(() => HandleFLN(response)),
            "UBX" => HandleUBX(response),
            "RNG" => HandleRNG(response),
            _ => Task.CompletedTask
        });
    }

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
            _ => Task.CompletedTask
        });
    }

    private async Task HandleILN(string response)
    {
        string[] parameters = response.Split(" ");

        Contact? contact = ContactList.Contacts.FirstOrDefault(c => c.Email == parameters[3]) ?? throw new ContactException("Contact does not exist");
        contact.Presence = parameters[2];
        contact.DisplayName = Uri.UnescapeDataString(parameters[5]);
        
        try
        {
            contact.DisplayPictureObject = Uri.UnescapeDataString(parameters[7]);
            contact.DisplayPictureObject = contact.DisplayPictureObject.Remove(contact.DisplayPictureObject.LastIndexOf("/>") + "/>".Length);
        }
        catch (IndexOutOfRangeException)
        {
            contact.DisplayPictureObject = null;
        }

        bool hasDisplayPicture = false;
        XmlSerializer serializer = new XmlSerializer(typeof(msnobj));

        if (contact.DisplayPictureObject != null)
        {
            using (StringReader reader = new StringReader(contact.DisplayPictureObject))
            {
                msnobj? msnobj = (msnobj?)serializer.Deserialize(reader);

                if (msnobj != null)
                {
                    if (msnobj.Size > 0)
                        hasDisplayPicture = true;
                }
            }
        }

        PresenceEventArgs eventArgs = new PresenceEventArgs()
        {
            Email = contact.Email,
            Presence = contact.Presence,
            HasDisplayPicture = hasDisplayPicture
        };

        PresenceChanged?.Invoke(this, eventArgs);

        string[] responses = response.Split("\r\n");
        string command = response.Replace(responses[0] + "\r\n", "");

        if (command != "")
            await HandleIncoming(command);
    }

    private void HandleNLN(string response)
    {
        string[] parameters = response.Split(" ");

        Contact? contact = ContactList.Contacts.FirstOrDefault(c => c.Email == parameters[2]) ?? throw new ContactException("Contact does not exist");
        contact.Presence = parameters[1];
        contact.DisplayName = Uri.UnescapeDataString(parameters[4]);

        try
        {
            contact.DisplayPictureObject = Uri.UnescapeDataString(parameters[6]);
        }
        catch (IndexOutOfRangeException)
        {
            contact.DisplayPictureObject = null;
        }

        bool hasDisplayPicture = false;
        XmlSerializer serializer = new XmlSerializer(typeof(msnobj));

        if (contact.DisplayPictureObject != null)
        {
            using (StringReader reader = new StringReader(contact.DisplayPictureObject))
            {
                msnobj? msnobj = (msnobj?)serializer.Deserialize(reader);

                if (msnobj != null)
                {
                    if (msnobj.Size > 0)
                        hasDisplayPicture = true;
                }
            }
        }

        PresenceEventArgs eventArgs = new PresenceEventArgs()
        {
            Email = contact.Email,
            Presence = contact.Presence,
            HasDisplayPicture = hasDisplayPicture
        };

        PresenceChanged?.Invoke(this, eventArgs);
    }

    private void HandleFLN(string response)
    {
        string[] parameters = response.Split(" ");

        Contact? contact = ContactList.Contacts.FirstOrDefault(c => c.Email == parameters[1]) ?? throw new ContactException("Contact does not exist");
        contact.Presence = string.Empty;

        PresenceEventArgs eventArgs = new PresenceEventArgs()
        {
            Email = contact.Email,
            Presence = contact.Presence
        };

        PresenceChanged?.Invoke(this, eventArgs);
    }

    private async Task HandleUBX(string response)
    {
        string[] responses = response.Split("\r\n");
        string[] parameters = responses[0].Split(" ");

        int length = Convert.ToInt32(parameters[3]);

        byte[] totalbytes = Encoding.UTF8.GetBytes(responses[1]);
        byte[] payloadBytes = new Span<byte>(totalbytes, 0, length).ToArray();
        string payload = Encoding.UTF8.GetString(payloadBytes);

        XmlSerializer serializer = new XmlSerializer(typeof(Data));

        using (StringReader reader = new StringReader(payload))
        {
            var data = (Data?)serializer.Deserialize(reader);

            Contact? contact = ContactList.Contacts.FirstOrDefault(c => c.Email == parameters[1]);

            if (contact == null || data == null)
                throw new ContactException("Contact does not exist");

            contact.PersonalMessage = data.PSM;

            PersonalMessageChanged?.Invoke(this, new PersonalMessageEventArgs()
            {
                Email = contact.Email,
                PersonalMessage = contact.PersonalMessage
            });
        }

        string commandAndPayload = response.Replace(responses[0] + "\r\n" + payload, "");
        if (commandAndPayload != "")
            await HandleIncoming(commandAndPayload);
    }

    private async Task HandleRNG(string response)
    {
        string[] parameters = response.Split(" ");

        string host = parameters[2].Split(":")[0];
        string port = parameters[2].Split(":")[1];

        string sessionID = parameters[1];
        string authString = parameters[4];
        string email = parameters[5];
        string displayName = Uri.UnescapeDataString(parameters[6]);

        Contact contact = ContactList.Contacts.FirstOrDefault(c => c.Email == email) ?? new Contact()
        {
            Email = email
        };

        if (string.IsNullOrEmpty(contact.DisplayName))
            contact.DisplayName = Uri.UnescapeDataString(displayName);

        Switchboard switchboard = new Switchboard()
        {
            Host = host,
            Port = Convert.ToInt32(port),
            Profile = ContactList.Profile,
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
