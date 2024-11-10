using System.Text;
using AvaMSN.MSNP.Exceptions;
using AvaMSN.MSNP.Messages;

namespace AvaMSN.MSNP.Switchboard.Messaging;

/// <summary>
/// Handles sending and receiving messages.
/// </summary>
public class Messaging : ISwitchboardWrapper
{
    public Switchboard? Server { get; set; }
    public IncomingMessaging? IncomingMessaging { get; private set; }

    /// <summary>
    /// Creates a new incoming object and assigns it to the Incoming propety.
    /// </summary>
    /// <exception cref="NullReferenceException">Thrown if the server is property is null.</exception>
    public void StartIncoming()
    {
        if (Server == null)
            throw new NullReferenceException("Server is null");

        IncomingMessaging = new IncomingMessaging
        {
            Server = Server,
            DisplayPictureTransfer = new DisplayPictureTransfer()
            {
                Server = Server
            }
        };
        Server.IncomingMessaging = IncomingMessaging;
    }
    
    /// <summary>
    /// Sends a plain text message.
    /// </summary>
    /// <param name="textMessage">Message object.</param>
    /// <returns></returns>
    /// <exception cref="CommandException">Thrown if the message couldn't be sent.</exception>
    public async Task SendTextMessage(TextPlain textMessage)
    {
        if (string.IsNullOrEmpty(textMessage.Text))
            return;
        
        if (Server == null)
            throw new NullReferenceException("Server is null");

        Server.ResetTimeout();
        Server.TransactionID++;
        string payload = textMessage.CreatePayload();
        int length = Encoding.UTF8.GetByteCount(payload);

        // Send MSG and message
        string message = $"MSG {Server.TransactionID} A {length}\r\n";
        await Server.SendAsync(message + payload);

        while (true)
        {
            // Receive ACK or NAK
            string response = await Server.ReceiveStringAsync();
            if (response.StartsWith("ACK")
                && response.Split(" ")[1].Replace("\r\n", "") == Server.TransactionID.ToString())
            {
                break;
            }

            if (response.StartsWith("NAK")
                && response.Split(" ")[1].Replace("\r\n", "") == Server.TransactionID.ToString())
            {
                throw new CommandException("Message failed to send");
            }
        }

        // Start receiving incoming commands again
        _ = Server.ReceiveIncomingAsync();
    }

    /// <summary>
    /// Sends an "is writing..." notification.
    /// </summary>
    /// <returns></returns>
    public async Task SendTypingUser()
    {
        if (Server == null)
            return;
        
        string payload = "MIME-Version: 1.0\r\n" +
                         "Content-Type: text/x-msmsgscontrol\r\n" +
                        $"TypingUser: {Server.User?.Email}\r\n\r\n\r\n";

        int length = Encoding.UTF8.GetByteCount(payload);

        // Send MSG and message
        string message = $"MSG {Server.TransactionID} U {length}\r\n";
        await Server.SendAsync(message + payload);
    }

    /// <summary>
    /// Sends a nudge.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="CommandException">Thrown if the message couldn't be sent.</exception>
    public async Task SendNudge()
    {
        if (Server == null)
            throw new NullReferenceException("Server is null");
        
        Server.ResetTimeout();
        Server.TransactionID++;

        string payload = "MIME-Version: 1.0\r\n" +
                         "Content-Type: text/x-msnmsgr-datacast\r\n\r\n" +
                         "ID: 1\r\n\r\n";

        int length = Encoding.UTF8.GetByteCount(payload);

        // Send MSG and message
        string message = $"MSG {Server.TransactionID} A {length}\r\n";
        await Server.SendAsync(message + payload);

        while (true)
        {
            // Receive ACK or NAK
            string response = await Server.ReceiveStringAsync();
            if (response.StartsWith("ACK")
                && response.Split(" ")[1].Replace("\r\n", "") == Server.TransactionID.ToString())
            {
                break;
            }

            if (response.StartsWith("NAK")
                && response.Split(" ")[1].Replace("\r\n", "") == Server.TransactionID.ToString())
            {
                throw new CommandException("Message failed to send");
            }
        }

        // Start receiving incoming commands again
        _ = Server.ReceiveIncomingAsync();
    }
}