using AvaMSN.MSNP.Exceptions;

namespace AvaMSN.MSNP.NotificationServer.Authentication;

/// <summary>
/// Handles authentication in the Notification Server.
/// </summary>
public class Authentication
{
    public NotificationServer Server { get; }
    public SingleSignOn SSO { get; }

    public Authentication(NotificationServer server)
    {
        Server = server;
        SSO = new SingleSignOn
        {
            Email = Server.User.Email,
            RstAddress = $"https://{Server.Host}/RST.srf"
        };
    }

    /// <summary>
    /// Negotiates protocol version with the server.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ProtocolException">Thrown if server does not support protocol version.</exception>
    public async Task SendVER()
    {
        // Send version
        Server.TransactionID++;
        var message = $"VER {Server.TransactionID} {NotificationServer.Protocol} CVR0\r\n";
        await Server.SendAsync(message);

        while (true)
        {
            // Receive version
            string response = await Server.ReceiveStringAsync();

            // Break if response is a command reply
            if (response.StartsWith("VER") && response.Split(" ")[1] == Server.TransactionID.ToString())
            {
                if (!response.Contains(NotificationServer.Protocol))
                    throw new ProtocolException("Protocol version is not supported by the server");

                break;
            }
        }
    }

    /// <summary>
    /// Sends client info.
    /// </summary>
    /// <returns></returns>
    public async Task SendCVR()
    {
        // Send CVR
        Server.TransactionID++;
        var message = $"CVR {Server.TransactionID} 0x0409 winnt 10 i386 AvaMSN 0.11.2 msmsgs\r\n";
        await Server.SendAsync(message);

        while (true)
        {
            // Receive CVR
            string response = await Server.ReceiveStringAsync();

            // Break if response is a command reply
            if (response.StartsWith("CVR") && response.Split(" ")[1] == Server.TransactionID.ToString())
                break;
        }
    }

    /// <summary>
    /// Does SSO authentication.
    /// </summary>
    /// <param name="password">User password.</param>
    /// <returns></returns>
    /// <exception cref="MsnpServerAuthException">Thrown if authentication isn't successful.</exception>
    public async Task Authenticate(string password)
    {
        try
        {
            await SSO.RstRequest(SSO.Email, password);
        }
        catch (NullReferenceException)
        {
            throw new MsnpServerAuthException("Could not get authentication token. Make sure email and password are correct.");
        }

        await AuthenticateWithTicket();
    }

    /// <summary>
    /// Authenticates using an existing ticket and binary secret
    /// </summary>
    /// <returns></returns>
    /// <exception cref="RedirectedByTheServerException">Thrown if the server redirects the client to another server.</exception>
    /// <exception cref="MsnpServerAuthException">Thrown if authentication isn't successful.</exception>
    public async Task AuthenticateWithTicket()
    {
        ContactService.TicketToken = SSO.TicketToken;

        // Send USR I
        Server.TransactionID++;
        var message = $"USR {Server.TransactionID} SSO I {SSO.Email}\r\n";
        await Server.SendAsync(message);

        string response = string.Empty;
        while (true)
        {
            // Receive GCF and USR S
            response = await Server.ReceiveStringAsync();

            // Throw exception in case the user is redirected
            if (response.StartsWith("XFR") && response.Split(" ")[2] == "NS")
            {
                string address = response.Split(" ")[3];
                string server = address.Split(":")[0];
                int port = Convert.ToInt32(address.Split(":")[1]);
                throw new RedirectedByTheServerException("Redirected to a different server, connecting...", server, port);
            }

            // Remove GCF response and break if USR reply is present
            if (response.Contains("USR") && response.StartsWith("GCF"))
            {
                response = HandleGCF(response);
                if (response.Split(" ")[1] == Server.TransactionID.ToString())
                    break;
            }

            // Break if response is a command reply
            if (response.StartsWith("USR")
                && response.Split(" ")[1] == Server.TransactionID.ToString())
                break;
        }

        string nonce = response.Split(" ")[5];
        nonce = nonce.Remove(nonce.IndexOf("\r\n"));
        string returnValue = SSO.GetReturnValue(nonce);

        // Send USR S
        Server.TransactionID++;
        message = $"USR {Server.TransactionID} SSO S {SSO.Ticket} {returnValue}\r\n";
        await Server.SendAsync(message);

        while (true)
        {
            // Receive USR S
            response = await Server.ReceiveStringAsync();

            // Break if response is a command reply and authentication was successful
            if (response.StartsWith("USR")
                && response.Split(" ")[1] == Server.TransactionID.ToString()
                && response.Contains("OK"))
            {
                break;
            }

            if (response.StartsWith("911"))
                throw new MsnpServerAuthException("Authentication failed. Make sure email and password are correct.");
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
}