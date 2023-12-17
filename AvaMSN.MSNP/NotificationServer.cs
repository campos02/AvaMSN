using AvaMSN.MSNP.Exceptions;
using AvaMSN.MSNP.XML.SerializableClasses;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;
using System.Xml;

namespace AvaMSN.MSNP;

/// <summary>
/// Represents a connection to the Notification Server (NS).
/// </summary>
public partial class NotificationServer : Connection
{
    // Protocol version
    public static string Protocol => "MSNP15";

    public ContactList ContactList { get; }
    public Profile Profile
    {
        get => ContactList.Profile;
        set => ContactList.Profile = value;
    }

    public SingleSignOn SSO { get; }
    public List<Switchboard> Switchboards { get; set; } = new List<Switchboard>();
    
    // Default client capabilities
    public readonly uint ClientCapabilities = 0x80000000;

    public NotificationServer(string host)
    {
        Host = host;

        SSO = new SingleSignOn(Host);
        ContactList = new ContactList(Host);
    }

    /// <summary>
    /// Stablishes a connection, does version negotiation and sends client info.
    /// </summary>
    /// <returns></returns>
    public async Task SendVersion()
    {
        await Connect();
        await SendVER();
        await SendCVR();
    }

    /// <summary>
    /// Gets ABCH data and sends it to the server.
    /// </summary>
    /// <returns></returns>
    public async Task GetContactList()
    {
        await ContactList.FindMembership();
        await ContactList.ABFindAll();

        await SendBLP();
        await SendInitialADL(ContactList.InitialListPayload());
        await SendPRP();
    }

    /// <summary>
    /// Negotiates protocol version with the server.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ProtocolException">Thrown if server does not support protocol version.</exception>
    private async Task SendVER()
    {
        TransactionID++;

        while (true)
        {
            // Send version
            var message = $"VER {TransactionID} {Protocol} CVR0\r\n";
            await SendAsync(message);

            // Receive version
            string response = await ReceiveStringAsync();

            // Break if response is a command reply
            if (response.StartsWith("VER") && response.Split(" ")[1] == TransactionID.ToString())
            {
                if (!response.Contains(Protocol))
                    throw new ProtocolException("Protocol version not supported by the server");

                break;
            }
        }
    }

    /// <summary>
    /// Sends client info.
    /// </summary>
    /// <returns></returns>
    private async Task SendCVR()
    {
        TransactionID++;

        while (true)
        {
            // Send CVR
            var message = $"CVR {TransactionID} 0x0409 winnt 10 i386 AvaMSN 0.8 msmsgs\r\n";
            await SendAsync(message);

            // Receive CVR
            string response = await ReceiveStringAsync();

            // Break if response is a command reply
            if (response.StartsWith("CVR") && response.Split(" ")[1] == TransactionID.ToString())
            {
                break;
            }
        }
    }

    /// <summary>
    /// Does SSO authentication.
    /// </summary>
    /// <param name="password">User password.</param>
    /// <returns></returns>
    /// <exception cref="AuthException">Thrown if authentication isn't successful.</exception>
    public async Task Authenticate(string password)
    {
        TransactionID++;

        string USR = string.Empty;

        // Send USR I
        var message = $"USR {TransactionID} SSO I {ContactList.Profile.Email}\r\n";
        await SendAsync(message);

        string responses = string.Empty;

        while (true)
        {
            // Receive GCF and USR S
            string response = await ReceiveStringAsync();
            responses += response;

            // Remove GCF response and break if USR reply is present
            if (responses.Contains("USR") && responses.StartsWith("GCF"))
            {
                USR = HandleGCF(responses);

                if (USR.Split(" ")[1] == TransactionID.ToString())
                    break;
            }

            // Break if response is a command reply
            if (response.StartsWith("USR")
                && response.Split(" ")[1] == TransactionID.ToString())
                break;
        }

        try
        {
            await SSO.RstRequest(ContactList.Profile.Email, password);
        }
        catch (NullReferenceException)
        {
            throw new AuthException("Could not get authentication token. Make sure email and password are correct.");
        }

        string nonce = USR.Split(" ")[5];
        nonce = nonce.Remove(nonce.IndexOf("\r\n"));
        string returnValue = SSO.GetReturnValue(nonce);

        TransactionID++;

        // Send USR S
        message = $"USR {TransactionID} SSO S {SSO.Ticket} {returnValue}\r\n";
        await SendAsync(message);

        while (true)
        {
            // Receive USR S
            string response = await ReceiveStringAsync();

            // Assign ticket and break if response is a command reply and authentication was successful
            if (response.StartsWith("USR")
                && response.Split(" ")[1] == TransactionID.ToString()
                && response.Contains("OK"))
            {
                ContactList.TicketToken = SSO.TicketToken;
                break;
            }

            else if (response.Contains("911"))
                throw new AuthException("Authentication failed. Make sure email and password are correct.");
        }
    }

    /// <summary>
    /// Authenticates using an existing ticket and binary secret.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="AuthException">Thrown if authentication isn't successful.</exception>
    public async Task AuthenticateWithToken()
    {
        TransactionID++;

        string USR = string.Empty;

        // Send USR I
        var message = $"USR {TransactionID} SSO I {ContactList.Profile.Email}\r\n";
        await SendAsync(message);

        string responses = string.Empty;

        while (true)
        {
            // Receive GCF and USR S
            string response = await ReceiveStringAsync();
            responses += response;

            // Remove GCF response and Break if USR reply is present
            if (responses.Contains("USR") && responses.StartsWith("GCF"))
            {
                USR = HandleGCF(responses);

                if (USR.Split(" ")[1] == TransactionID.ToString())
                    break;
            }

            // Break if response is a command reply
            if (response.StartsWith("USR")
                && response.Split(" ")[1] == TransactionID.ToString())
                break;
        }

        string nonce = USR.Split(" ")[5];
        nonce = nonce.Remove(nonce.IndexOf("\r\n"));
        string returnValue = SSO.GetReturnValue(nonce);

        TransactionID++;

        // Send USR S
        message = $"USR {TransactionID} SSO S {SSO.Ticket} {returnValue}\r\n";
        await SendAsync(message);

        while (true)
        {
            // Receive USR S
            string response = await ReceiveStringAsync();

            // Break if response is a command reply and authentication was successful
            if (response.StartsWith("USR")
                && response.Split(" ")[1] == TransactionID.ToString()
                && response.Contains("OK"))
            {
                ContactList.TicketToken = SSO.TicketToken;
                break;
            }

            else if (response.Contains("911"))
                throw new AuthException("Authentication failed. Make sure email and password are correct.");
        }
    }

    /// <summary>
    /// Remove GCF command and payload, returning only an USR response.
    /// </summary>
    /// <param name="response">GCF response.</param>
    /// <returns>USR response.</returns>
    private static string HandleGCF(string response)
    {
        return response[response.IndexOf("USR")..];
    }

    /// <summary>
    /// Sets user privacy settings in the NS side.
    /// </summary>
    /// <returns></returns>
    private async Task SendBLP()
    {
        // Value the server reads
        string blp = ContactList.Profile.BLP switch
        {
            2 => "BL",
            _ => "AL"
        };

        TransactionID++;

        // Send BLP
        string message = $"BLP {TransactionID} {blp}\r\n";
        await SendAsync(message);

        while (true)
        {
            // Receive BLP
            string response = await ReceiveStringAsync();

            // Break if response contains a command reply
            if (response.Contains("BLP")
                && response.Contains(blp))
            {
                // Remove other data before reply
                string blpResponse = response[response.IndexOf("BLP")..];

                if (blpResponse.Split(" ")[1] == TransactionID.ToString())
                    break;
            }

            // Handle normally if response wasn't a reply to this command
            await HandleIncoming(response);
        }
    }

    /// <summary>
    /// Adds all contacts retrived from the ABCH in the NS side.
    /// </summary>
    /// <param name="payload">Initial ADL payload.</param>
    /// <returns></returns>
    /// <exception cref="PayloadException">Thrown if payload exceeds max size.</exception>
    private async Task SendInitialADL(string payload)
    {
        int length = Encoding.UTF8.GetByteCount(payload);

        if (length > 1160)
            throw new PayloadException("Payload too big");

        TransactionID++;
        string message = $"ADL {TransactionID} {length}\r\n";

        // Send ADL and payload
        await SendAsync(message + payload);

        while (true)
        {
            // Receive ADL
            string response = await ReceiveStringAsync();

            // Break if response contains a command reply and ADL was successful
            if (response.Contains("ADL")
                && response.Contains("OK"))
            {
                // Remove other data before reply
                string adlResponse = response[response.IndexOf("ADL")..];

                if (adlResponse.Split(" ")[1] == TransactionID.ToString())
                    break;
            }

            // Handle normally if response wasn't a reply to this command
            await HandleIncoming(response);
        }
    }

    /// <summary>
    /// Adds a contact in the NS side.
    /// </summary>
    /// <param name="payload">ADL list payload.</param>
    /// <returns></returns>
    /// <exception cref="PayloadException">Thrown if payload exceeds max size.</exception>
    private async Task SendADL(string payload)
    {
        int length = Encoding.UTF8.GetByteCount(payload);

        if (length > 1160)
            throw new PayloadException("Payload too big");

        TransactionID++;
        string message = $"ADL {TransactionID} {length}\r\n";

        // Send ADL and payload
        await SendAsync(message + payload);
    }

    /// <summary>
    /// Sends an FQY command, used when adding a contact.
    /// </summary>
    /// <param name="payload">FQY contact payload.</param>
    /// <returns></returns>
    /// <exception cref="PayloadException">Thrown if payload exceeds max size.</exception>
    private async Task SendFQY(string payload)
    {
        int length = Encoding.UTF8.GetByteCount(payload);

        if (length > 1160)
            throw new PayloadException("Payload too big");

        TransactionID++;
        string message = $"FQY {TransactionID} {length}\r\n";

        // Send FQY and payload
        await SendAsync(message + payload);
    }

    /// <summary>
    /// Removes a contact in the NS side.
    /// </summary>
    /// <param name="payload">RML list payload.</param>
    /// <returns></returns>
    /// <exception cref="PayloadException">Thrown if payload exceeds max size.</exception>
    private async Task SendRML(string payload)
    {
        int length = Encoding.UTF8.GetByteCount(payload);

        if (length > 1160)
            throw new PayloadException("Payload too big");

        TransactionID++;

        string message = $"RML {TransactionID} {length}\r\n";

        // Send RML and payload
        await SendAsync(message + payload);

        while (true)
        {
            // Receive RML
            string response = await ReceiveStringAsync();

            // Break if response is a command reply and RML was successful
            if (response.StartsWith("RML")
                && response.Split(" ")[1] == TransactionID.ToString()
                && response.Contains("OK"))
            {
                break;
            }

            // Handle normally if response wasn't a reply to this command
            await HandleIncoming(response);
        }
    }

    /// <summary>
    /// Sets the user's display name in the NS side.
    /// </summary>
    /// <returns></returns>
    private async Task SendPRP()
    {
        TransactionID++;
        string encodedDisplayName = Uri.EscapeDataString(ContactList.Profile.DisplayName);

        // Send PRP
        string message = $"PRP {TransactionID} MFN {encodedDisplayName}\r\n";
        await SendAsync(message);

        while (true)
        {
            // Receive PRP
            string response = await ReceiveStringAsync();

            // Break if response is a command reply
            if (response.StartsWith("PRP")
                && response.Split(" ")[1] == TransactionID.ToString()
                && response.Contains(encodedDisplayName))
            {
                break;
            }

            // Handle normally if response wasn't a reply to this command
            await HandleIncoming(response);
        }
    }

    /// <summary>
    /// Changes the user's presence status, including the MSN object if present.
    /// </summary>
    /// <returns></returns>
    public async Task SendCHG()
    {
        TransactionID++;

        // Send CHG
        string message = $"CHG {TransactionID} {Profile.Presence} {ClientCapabilities}";

        // Add MSN object if present
        if (Profile.DisplayPictureObject != null)
            message += $" {Uri.EscapeDataString(Profile.DisplayPictureObject)}\r\n";
        else
            message += "\r\n";

        await SendAsync(message);

        while (true)
        {
            // Receive CHG
            string response = await ReceiveStringAsync();

            // Make sure response contains a command reply
            if (response.Contains("CHG")
                && response.Contains(ContactList.Profile.Presence))
            {
                // Remove other data before reply
                string chgResponse = response[response.IndexOf("CHG")..];

                // Remove and handle other responses if they were also received
                string[] responses = response.Split("\r\n");
                string command = response.Replace(responses[0] + "\r\n", "");

                if (command != "")
                    await HandleIncoming(command);

                // Break if response is a command reply
                if (chgResponse.Split(" ")[1] == TransactionID.ToString())
                    break;
            }
        }

        // Start handling incoming commands
        _ = ReceiveIncomingAsync();
    }

    /// <summary>
    /// Sends user personal message.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="PayloadException">Thrown if payload exceeds max size.</exception>
    public async Task SendUUX()
    {
        string payload = ContactList.UUXPayload();
        int length = Encoding.UTF8.GetByteCount(payload);

        if (length > 1160)
            throw new PayloadException("Payload too big");

        TransactionID++;
        string message = $"UUX {TransactionID} {length}\r\n";

        // Send UUX and payload
        await SendAsync(message + payload);

        while (true)
        {
            // Receive UUX
            string response = await ReceiveStringAsync();

            // Break if response is a command reply
            if (response.StartsWith("UUX")
                && response.Split(" ")[1] == TransactionID.ToString())
            {
                break;
            }
        }

        // Start handling incoming commands again
        _ = ReceiveIncomingAsync();
    }

    /// <summary>
    /// Requests a new switchboard session, connects to it, then invites a contact.
    /// </summary>
    /// <param name="contact">Contact to invite for the session.</param>
    /// <returns>New switchboard session.</returns>
    public async Task<Switchboard> SendXFR(Contact contact)
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

        Switchboard switchboard = new Switchboard()
        {
            Host = host,
            Port = Convert.ToInt32(port),
            Profile = ContactList.Profile,
            Contact = contact
        };

        // Authenticate and invite contact
        await switchboard.SendUSR(authString);
        await switchboard.SendCAL();

        SwitchboardChanged?.Invoke(this, new SwitchboardEventArgs
        {
            Email = contact.Email,
            Switchboard = switchboard
        });

        Switchboards.Add(switchboard);

        return switchboard;
    }

    /// <summary>
    /// Searches for a contact using its email then calls contact parameter overload.
    /// </summary>
    /// <param name="contactEmail">Contact email.</param>
    /// <returns>New switchboard session.</returns>
    public async Task<Switchboard> SendXFR(string contactEmail)
    {
        Contact? contact = ContactList.Contacts.FirstOrDefault(ct => ct.Email == contactEmail);

        if (contact != null)
            return await SendXFR(contact);
        else
            throw new ContactException("Contact does not exist");
    }

    /// <summary>
    /// Creates and assigns an MSN object if any display picture data is present.
    /// </summary>
    public void CreateMSNObject()
    {
        if (ContactList.Profile.DisplayPicture == null)
            return;

        msnobj displayPicture = new msnobj()
        {
            Creator = ContactList.Profile.Email,
            Size = (ushort)ContactList.Profile.DisplayPicture.Length,
            Type = 3,
            Location = 0,
            Friendly = "AAA",
            SHA1D = Convert.ToBase64String(SHA1.HashData(ContactList.Profile.DisplayPicture))
        };

        var settings = new XmlWriterSettings()
        {
            OmitXmlDeclaration = true
        };

        // Remove namespaces
        var namespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });

        XmlSerializer msnobjectSerializer = new(typeof(msnobj));

        using StringWriter stream = new StringWriter();
        using XmlWriter writer = XmlWriter.Create(stream, settings);

        // Serialize with options
        msnobjectSerializer.Serialize(writer, displayPicture, namespaces);
        ContactList.Profile.DisplayPictureObject = stream.ToString();
    }

    /// <summary>
    /// Sends a disconnection command and invokes the Disconnected event for the NS and every connected SB.
    /// </summary>
    /// <param name="requested">Whether the disconnection was requested by the user.</param>
    /// <returns></returns>
    public override async Task DisconnectAsync(bool requested = true)
    {
        await base.DisconnectAsync(requested);
        
        foreach (Switchboard switchboard in Switchboards)
        {
            if (switchboard.Connected)
                await switchboard.DisconnectAsync();
        }
    }
}