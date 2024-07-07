using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using AvaMSN.MSNP.Exceptions;
using AvaMSN.MSNP.Models;
using AvaMSN.MSNP.XML.SerializableClasses;

namespace AvaMSN.MSNP.NotificationServer.UserProfile;

public class UserProfile
{
    public NotificationServer? Server { get; init; }
    public User? User => Server?.User;
    
    // Default client capabilities
    public readonly uint ClientCapabilities = 0x80000000;
    
    /// <summary>
    /// Sets user privacy settings on the NS side.
    /// </summary>
    /// <returns></returns>
    internal async Task SendBLP()
    {
        if (Server == null)
            throw new NullReferenceException("Server is null");
        
        // Value the server reads
        string blp = User?.BLP switch
        {
            2 => "BL",
            _ => "AL"
        };

        Server.TransactionID++;

        // Send BLP
        string message = $"BLP {Server.TransactionID} {blp}\r\n";
        await Server.SendAsync(message);

        while (true)
        {
            // Receive BLP
            string response = await Server.ReceiveStringAsync();

            // Break if response contains a command reply
            if (response.Contains("BLP")
                && response.Contains(blp))
            {
                // Remove other data before reply
                string blpResponse = response[response.IndexOf("BLP")..];

                if (blpResponse.Split(" ")[1] == Server.TransactionID.ToString())
                    break;
            }

            // Handle normally if response wasn't a reply to this command
            await Server.HandleIncoming(response);
        }
    }
    
    /// <summary>
    /// Sets the user's display name on the NS side.
    /// </summary>
    /// <param name="displayName">User display name.</param>
    /// <returns></returns>
    internal async Task SendPRP(string displayName)
    {
        if (Server == null)
            throw new NullReferenceException("Server is null");

        Server.TransactionID++;
        
        // Send PRP
        string message = $"PRP {Server.TransactionID} MFN {Uri.EscapeDataString(displayName)}\r\n";
        await Server.SendAsync(message);

        while (true)
        {
            // Receive PRP
            string response = await Server.ReceiveStringAsync();

            // Break if response is a command reply
            if (response.StartsWith("PRP")
                && response.Split(" ")[1] == Server.TransactionID.ToString())
            {
                break;
            }

            // Handle normally if response wasn't a reply to this command
            await Server.HandleIncoming(response);
        }
    }

    /// <summary>
    /// Changes the user's presence status and includes the MSN object if present.
    /// </summary>
    /// <returns></returns>
    public async Task SendCHG()
    {
        if (Server == null)
            throw new NullReferenceException("Server is null");
        
        Server.TransactionID++;

        // Send CHG
        string message = $"CHG {Server.TransactionID} {User?.Presence} {ClientCapabilities}";

        // Add MSN object if present
        if (User?.DisplayPictureObject != null)
            message += $" {Uri.EscapeDataString(User.DisplayPictureObject)}\r\n";
        else
            message += "\r\n";

        await Server.SendAsync(message);
        while (true)
        {
            // Receive CHG
            string response = await Server.ReceiveStringAsync();

            // Make sure response contains a command reply
            if (response.Contains("CHG")
                && response.Contains(User?.Presence ?? string.Empty))
            {
                // Remove other data before reply
                string chgResponse = response[response.IndexOf("CHG")..];

                // Remove and handle other responses if they were also received
                string[] responses = chgResponse.Split("\r\n");
                string command = response.Replace(responses[0] + "\r\n", "");

                if (command != "")
                    await Server.HandleIncoming(command);

                // Break if response is a command reply
                if (chgResponse.Split(" ")[1] == Server.TransactionID.ToString())
                    break;
            }
        }

        // Start handling incoming commands
        _ = Server.ReceiveIncomingAsync();
    }

    /// <summary>
    /// Sends a user personal message.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="PayloadException">Thrown if payload exceeds max size.</exception>
    public async Task SendUUX()
    {
        if (Server == null)
            throw new NullReferenceException("Server is null");
        
        string payload = UserPayloads.UUXPayload(User?.PersonalMessage ?? string.Empty);
        int length = Encoding.UTF8.GetByteCount(payload);

        if (length > 1160)
            throw new PayloadException("Payload too big");

        Server.TransactionID++;
        string message = $"UUX {Server.TransactionID} {length}\r\n";

        // Send UUX and payload
        await Server.SendAsync(message + payload);

        while (true)
        {
            // Receive UUX
            string response = await Server.ReceiveStringAsync();

            // Break if response is a command reply
            if (response.StartsWith("UUX")
                && response.Split(" ")[1] == Server.TransactionID.ToString())
            {
                break;
            }
        }

        // Start handling incoming commands again
        _ = Server.ReceiveIncomingAsync();
    }
    
    
    
    /// <summary>
    /// Change the user's display name on the ABCH and NS sides.
    /// </summary>
    /// <returns></returns>
    public async Task ChangeDisplayName()
    {
        if (Server?.User == null)
            throw new NullReferenceException("User is null");
        
        await ContactService.ChangeDisplayName(Server.User.DisplayName);
        await SendPRP(Server.User.DisplayName);

        // Start receiving incoming commands again
        _ = Server?.ReceiveIncomingAsync();
    }

    /// <summary>
    /// Creates and assigns an MSN object if any display picture data is present.
    /// </summary>
    public void CreateMSNObject()
    {
        if (User?.DisplayPicture == null)
            return;

        msnobj displayPicture = new msnobj
        {
            Creator = User.Email,
            Size = (ushort)User.DisplayPicture.Length,
            Type = 3,
            Location = 0,
            Friendly = "AAA",
            SHA1D = Convert.ToBase64String(SHA1.HashData(User.DisplayPicture))
        };

        var settings = new XmlWriterSettings
        {
            OmitXmlDeclaration = true
        };

        // Remove namespaces
        var namespaces = new XmlSerializerNamespaces([XmlQualifiedName.Empty]);
        XmlSerializer msnobjectSerializer = new(typeof(msnobj));

        using StringWriter stream = new StringWriter();
        using XmlWriter writer = XmlWriter.Create(stream, settings);

        // Serialize with options
        msnobjectSerializer.Serialize(writer, displayPicture, namespaces);
        User.DisplayPictureObject = stream.ToString();
    }
}