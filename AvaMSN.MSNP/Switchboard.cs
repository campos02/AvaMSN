using AvaMSN.MSNP.Exceptions;
using AvaMSN.MSNP.Messages;
using System.Text;

namespace AvaMSN.MSNP;

/// <summary>
/// Represents a connection to the Switchboard (SB).
/// </summary>
public partial class Switchboard : Connection
{
    public Profile Profile { get; set; } = new Profile();
    public Contact Contact { get; set; } = new Contact();

    /// <summary>
    /// Authenticates in a requested switchboard session.
    /// </summary>
    /// <param name="authString">Auth string sent by the NS.</param>
    /// <returns></returns>
    /// <exception cref="AuthException">Thrown if authentication isn't successful.</exception>
    public async Task SendUSR(string authString)
    {
        await Connect();

        TransactionID++;

        // Send USR
        string message = $"USR {TransactionID} {Profile.Email} {authString}\r\n";
        await SendAsync(message);

        while (true)
        {
            // Receive USR
            string response = await ReceiveStringAsync();

            // Break if response is a USR reply and authentication was successful
            if (response.StartsWith("USR")
                && response.Split(" ")[1] == TransactionID.ToString()
                && response.Contains("OK"))
            {
                break;
            }

            else if (response.Contains("911"))
                throw new AuthException("Authentication failed");
        }

        // Start receiving incoming commands
        _ = ReceiveIncomingAsync();
    }

    /// <summary>
    /// Authenticates in a switchboard session the user was invited into.
    /// </summary>
    /// <param name="sessionID">Session ID sent by the NS.</param>
    /// <param name="authString">Auth string sent by the NS.</param>
    /// <returns></returns>
    /// <exception cref="AuthException">Thrown if authentication isn't successful.</exception>
    public async Task SendANS(string sessionID, string authString)
    {
        await Connect();

        TransactionID++;

        // Send ANS
        string message = $"ANS {TransactionID} {Profile.Email} {authString} {sessionID}\r\n";
        await SendAsync(message);

        while (true)
        {
            // Receive ANS
            string response = await ReceiveStringAsync();

            // Break if response is an ANS reply and authentication was successful
            if (response.StartsWith("ANS")
                && response.Split(" ")[1] == TransactionID.ToString()
                && response.Contains("OK"))
            {
                break;
            }

            else if (response.Contains("911"))
                throw new AuthException("Authentication failed");
        }

        // Start receiving incoming commands
        _ = ReceiveIncomingAsync();
    }

    /// <summary>
    /// Invites a contact into the session and waits for it to join.
    /// </summary>
    /// <returns></returns>
    public async Task SendCAL()
    {
        TransactionID++;

        // Send CAL
        string message = $"CAL {TransactionID} {Contact.Email}\r\n";
        await SendAsync(message);

        // Receive CAL
        while (true)
        {
            string response = await ReceiveStringAsync();

            // Break if response is a command reply
            if (response.StartsWith("CAL")
                && response.Split(" ")[1] == TransactionID.ToString())
            {
                break;
            }
        }

        // Wait for the contact to join
        while (true)
        {
            string response = await ReceiveStringAsync();

            // Break if the contact has joined
            if (response.StartsWith("JOI")
                && Uri.UnescapeDataString(response.Split(" ")[1]) == Contact.Email)
            {
                break;
            }
        }

        // Start receiving incoming commands again
        _ = ReceiveIncomingAsync();
    }

    /// <summary>
    /// Sends a plain text message.
    /// </summary>
    /// <param name="textMessage">Message object.</param>
    /// <returns></returns>
    /// <exception cref="CommandException">Thrown if the message couldn't be sent.</exception>
    public async Task SendTextMessage(TextPlain textMessage)
    {
        if (string.IsNullOrEmpty(textMessage.Content))
            return;

        TransactionID++;

        string payload = textMessage.CreatePayload();
        int length = Encoding.UTF8.GetByteCount(payload);

        // Send MSG and message
        string message = $"MSG {TransactionID} A {length}\r\n";
        await SendAsync(message + payload);

        while (true)
        {
            // Receive ACK or NAK
            string response = await ReceiveStringAsync();

            if (response.StartsWith("ACK")
                && response.Split(" ")[1].Replace("\r\n", "") == TransactionID.ToString())
            {
                break;
            }

            if (response.StartsWith("NAK")
                && response.Split(" ")[1].Replace("\r\n", "") == TransactionID.ToString())
            {
                throw new CommandException("Message failed to send");
            }
        }

        // Start receiving incoming commands again
        _ = ReceiveIncomingAsync();
    }

    /// <summary>
    /// Sends an "is writing..." notification.
    /// </summary>
    /// <returns></returns>
    public async Task SendTypingUser()
    {
        string payload = "MIME-Version: 1.0\r\n" +
                         "Content-Type: text/x-msmsgscontrol\r\n" +
                        $"TypingUser: {Profile.Email}\r\n\r\n\r\n";

        int length = Encoding.UTF8.GetByteCount(payload);

        // Send MSG and message
        string message = $"MSG {TransactionID} U {length}\r\n";
        await SendAsync(message + payload);
    }

    /// <summary>
    /// Sends a nudge.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="CommandException">Thrown if the message couldn't be sent.</exception>
    public async Task SendNudge()
    {
        TransactionID++;

        string payload = "MIME-Version: 1.0\r\n" +
                         "Content-Type: text/x-msnmsgr-datacast\r\n\r\n" +
                         "ID: 1\r\n\r\n";

        int length = Encoding.UTF8.GetByteCount(payload);

        // Send MSG and message
        string message = $"MSG {TransactionID} A {length}\r\n";
        await SendAsync(message + payload);

        while (true)
        {
            // Receive ACK or NAK
            string response = await ReceiveStringAsync();

            if (response.StartsWith("ACK")
                && response.Split(" ")[1].Replace("\r\n", "") == TransactionID.ToString())
            {
                break;
            }

            if (response.StartsWith("NAK")
                && response.Split(" ")[1].Replace("\r\n", "") == TransactionID.ToString())
            {
                throw new CommandException("Message failed to send");
            }
        }

        // Start receiving incoming commands again
        _ = ReceiveIncomingAsync();
    }
}
