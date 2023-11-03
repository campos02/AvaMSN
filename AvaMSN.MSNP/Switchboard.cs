using AvaMSN.MSNP.Exceptions;
using System.Text;

namespace AvaMSN.MSNP;

public partial class Switchboard : Connection
{
    public Profile Profile { get; set; } = new Profile();
    public Contact Contact { get; set; } = new Contact();

    /// <summary>
    /// Do USR authentication steps
    /// </summary>
    /// <returns></returns>
    public async Task SendUSR(string authenticationString)
    {
        await Connect();

        TransactionID++;

        // Send USR
        string message = $"USR {TransactionID} {Profile.Email} {authenticationString}\r\n";
        await SendAsync(message);

        while (true)
        {
            // Receive USR
            string response = await ReceiveAsync();

            if (response.StartsWith("USR")
                && response.Split(" ")[1] == TransactionID.ToString()
                && response.Contains("OK"))
            {
                break;
            }

            else if (response.Contains("911"))
                throw new AuthException("Authentication failed");
        }

        _ = ReceiveIncomingAsync();
    }

    public async Task SendANS(string sessionID, string authenticationString)
    {
        await Connect();

        TransactionID++;

        // Send ANS
        string message = $"ANS {TransactionID} {Profile.Email} {authenticationString} {sessionID}\r\n";
        await SendAsync(message);

        while (true)
        {
            // Receive ANS
            string response = await ReceiveAsync();

            if (response.StartsWith("ANS")
                && response.Split(" ")[1] == TransactionID.ToString()
                && response.Contains("OK"))
            {
                break;
            }

            else if (response.Contains("911"))
                throw new AuthException("Authentication failed");
        }

        _ = ReceiveIncomingAsync();
    }

    public async Task SendCAL()
    {
        TransactionID++;

        // Send CAL
        string message = $"CAL {TransactionID} {Contact.Email}\r\n";
        await SendAsync(message);

        while (true)
        {
            // Receive CAL
            string response = await ReceiveAsync();

            if (response.StartsWith("CAL")
                && response.Split(" ")[1] == TransactionID.ToString())
            {
                break;
            }
        }

        _ = ReceiveIncomingAsync();
    }

    public async Task SendTextMessage(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        TransactionID++;

        string payload = "MIME-Version: 1.0\r\n" +
                         "Content-Type: text/plain; charset=UTF-8\r\n" +
                         "X-MMS-IM-Format: FN=Segoe%20UI; EF=; CO=0; CS=0; PF=22\r\n\r\n" +
                          text;

        int length = Encoding.UTF8.GetByteCount(payload);

        // Send MSG and message
        string message = $"MSG {TransactionID} A {length}\r\n";
        await SendAsync(message + payload);

        while (true)
        {
            // Receive ACK or NAK
            string response = await ReceiveAsync();

            if (response.StartsWith("ACK")
                && response.Split(" ")[1].Replace("\r\n", "") == TransactionID.ToString())
            {
                break;
            }

            if (response.StartsWith("NAK")
                && response.Split(" ")[1].Replace("\r\n", "") == TransactionID.ToString())
            {
                throw new PayloadException("Message failed to send");
            }
        }

        _ = ReceiveIncomingAsync();
    }

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
            string response = await ReceiveAsync();

            if (response.StartsWith("ACK")
                && response.Split(" ")[1].Replace("\r\n", "") == TransactionID.ToString())
            {
                break;
            }

            if (response.StartsWith("NAK")
                && response.Split(" ")[1].Replace("\r\n", "") == TransactionID.ToString())
            {
                throw new PayloadException("Message failed to send");
            }
        }

        _ = ReceiveIncomingAsync();
    }
}
