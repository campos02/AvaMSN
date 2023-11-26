using AvaMSN.MSNP.Exceptions;
using AvaMSN.MSNP.XML.SerializableClasses;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;
using System.Xml;

namespace AvaMSN.MSNP;

public partial class NotificationServer : Connection
{
    public static string Protocol => "MSNP15";

    public ContactList ContactList { get; set; }
    public Profile Profile
    {
        get => ContactList.Profile;
        set => ContactList.Profile = value;
    }

    public SingleSignOn SSO { get; set; }
    public List<Switchboard> Switchboards { get; set; } = new List<Switchboard>();
    public readonly uint ClientCapabilities = 0x80000000;

    public NotificationServer(string host)
    {
        Host = host;

        SSO = new SingleSignOn(Host);
        ContactList = new ContactList(Host);

        // Add mobile device capability if on a mobile OS
        if (OperatingSystem.IsIOS() || OperatingSystem.IsAndroid())
        {
            ClientCapabilities += 0x01;
        }
    }

    /// <summary>
    /// Do all login commands
    /// </summary>
    /// <returns></returns>
    public async Task SendVersion()
    {
        await Connect();

        await SendVER();
        await SendCVR();
    }

    public async Task GetContactList()
    {
        await ContactList.FindMembership();
        await ContactList.ABFindAll();

        await SendBLP();
        await SendInitialADL(ContactList.InitialListPayload());
        await SendPRP();
    }

    /// <summary>
    /// Send version command if it's supported by the server
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ProtocolException">Throw if server does not support protocol version</exception>
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

            if (response.StartsWith("VER") && response.Split(" ")[1] == TransactionID.ToString())
            {
                if (!response.Contains(Protocol))
                    throw new ProtocolException("Protocol version not supported by the server");

                break;
            }
        }
    }

    /// <summary>
    /// Send CVR command and check for a response
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

            if (response.StartsWith("CVR") && response.Split(" ")[1] == TransactionID.ToString())
            {
                break;
            }
        }
    }

    /// <summary>
    /// Do authentication steps
    /// </summary>
    /// <returns></returns>
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

            if (responses.Contains("USR") && responses.StartsWith("GCF"))
            {
                USR = HandleGCF(responses);

                if (USR.Split(" ")[1] == TransactionID.ToString())
                    break;
            }

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

            if (responses.Contains("USR") && responses.StartsWith("GCF"))
            {
                USR = HandleGCF(responses);

                if (USR.Split(" ")[1] == TransactionID.ToString())
                    break;
            }

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
    /// Return USR part after GCF response XML
    /// </summary>
    /// <param name="response">USR response</param>
    /// <returns>USR response</returns>
    private string HandleGCF(string response)
    {
        return response[response.IndexOf("USR")..];
    }

    public async Task SendBLP()
    {
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

            if (response.Contains("BLP")
                && response.Contains(blp))
            {
                string blpResponse = response[response.IndexOf("BLP")..];

                if (blpResponse.Split(" ")[1] == TransactionID.ToString())
                    break;
            }

            await HandleIncoming(response);
        }
    }

    public async Task SendInitialADL(string payload)
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

            if (response.Contains("ADL")
                && response.Contains("OK"))
            {
                string adlResponse = response[response.IndexOf("ADL")..];

                if (adlResponse.Split(" ")[1] == TransactionID.ToString())
                    break;
            }

            await HandleIncoming(response);
        }
    }

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

    private async Task SendFQY(string payload)
    {
        int length = Encoding.UTF8.GetByteCount(payload);

        if (length > 1160)
            throw new PayloadException("Payload too big");

        TransactionID++;

        string message = $"FQY {TransactionID} {length}\r\n";

        // Send ADL and payload
        await SendAsync(message + payload);
    }

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

            if (response.StartsWith("RML")
                && response.Split(" ")[1] == TransactionID.ToString()
                && response.Contains("OK"))
            {
                break;
            }

            await HandleIncoming(response);
        }
    }

    public async Task SendPRP()
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

            if (response.StartsWith("PRP")
                && response.Split(" ")[1] == TransactionID.ToString()
                && response.Contains(encodedDisplayName))
            {
                break;
            }

            await HandleIncoming(response);
        }
    }

    public async Task SendCHG()
    {
        TransactionID++;

        // Send CHG
        string message = $"CHG {TransactionID} {Profile.Presence} {ClientCapabilities}";

        if (Profile.DisplayPictureObject != null)
            message += $" {Uri.EscapeDataString(Profile.DisplayPictureObject)}\r\n";
        else
            message += "\r\n";

        await SendAsync(message);

        while (true)
        {
            // Receive CHG
            string response = await ReceiveStringAsync();

            if (response.Contains("CHG")
                && response.Contains(ContactList.Profile.Presence))
            {
                string chgResponse = response[response.IndexOf("CHG")..];

                string[] responses = response.Split("\r\n");
                string command = response.Replace(responses[0] + "\r\n", "");

                if (command != "")
                    await HandleIncoming(command);

                if (chgResponse.Split(" ")[1] == TransactionID.ToString())
                    break;
            }
        }

        _ = ReceiveIncomingAsync();
    }

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

            if (response.StartsWith("UUX")
                && response.Split(" ")[1] == TransactionID.ToString())
            {
                break;
            }
        }

        _ = ReceiveIncomingAsync();
    }

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

            if (response.StartsWith("XFR")
                && response.Split(" ")[1] == TransactionID.ToString())
            {
                break;
            }
        }

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

    public void GenerateMSNObject()
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

        var namespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });

        XmlSerializer msnobjectSerializer = new(typeof(msnobj));

        using StringWriter stream = new StringWriter();
        using XmlWriter writer = XmlWriter.Create(stream, settings);

        msnobjectSerializer.Serialize(writer, displayPicture, namespaces);
        ContactList.Profile.DisplayPictureObject = stream.ToString();
    }
}