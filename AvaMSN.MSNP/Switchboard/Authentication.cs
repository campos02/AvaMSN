using AvaMSN.MSNP.Exceptions;

namespace AvaMSN.MSNP.Switchboard;

internal class Authentication : ISwitchboardWrapper
{
    public Switchboard? Server { get; set; }
    
    /// <summary>
    /// Authenticates in a requested switchboard session.
    /// </summary>
    /// <param name="authString">Auth string sent by the NS.</param>
    /// <returns></returns>
    /// <exception cref="AuthException">Thrown if authentication isn't successful.</exception>
    public async Task SendUSR(string authString)
    {
        if (Server == null)
            throw new NullReferenceException("Server is null");
        
        await Server.Connect();
        Server.TransactionID++;

        // Send USR
        string message = $"USR {Server.TransactionID} {Server.User?.Email} {authString}\r\n";
        await Server.SendAsync(message);

        while (true)
        {
            // Receive USR
            string response = await Server.ReceiveStringAsync();

            // Break if response is a USR reply and authentication was successful
            if (response.StartsWith("USR")
                && response.Split(" ")[1] == Server.TransactionID.ToString()
                && response.Contains("OK"))
            {
                break;
            }

            if (response.Contains("911"))
                throw new AuthException("Authentication failed");
        }

        // Start receiving incoming commands
        _ = Server.ReceiveIncomingAsync();
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
        if (Server == null)
            throw new NullReferenceException("Server is null");
        
        await Server.Connect();
        Server.TransactionID++;

        // Send ANS
        string message = $"ANS {Server.TransactionID} {Server.User?.Email} {authString} {sessionID}\r\n";
        await Server.SendAsync(message);

        while (true)
        {
            // Receive ANS
            string response = await Server.ReceiveStringAsync();

            // Make sure response contains a command reply
            if (response.Contains("ANS")
                && response.Contains("OK"))
            {
                // Remove other data before reply
                string ansResponse = response[response.IndexOf("ANS")..];

                // Remove and handle other responses if they were also received
                string[] responses = ansResponse.Split("\r\n");
                string command = response.Replace(responses[0] + "\r\n", "");

                if (command != "")
                    await Server.HandleIncoming(command);

                // Break if response is a command reply
                if (ansResponse.Split(" ")[1] == Server.TransactionID.ToString())
                    break;
            }

            else if (response.Contains("911"))
                throw new AuthException("Authentication failed");
        }

        // Start receiving incoming commands
        _ = Server.ReceiveIncomingAsync();
    }

    /// <summary>
    /// Invites a contact into the session
    /// </summary>
    /// <returns></returns>
    public async Task SendCAL()
    {
        if (Server == null)
            throw new NullReferenceException("Server is null");
        
        // Send CAL
        Server.TransactionID++;
        string message = $"CAL {Server.TransactionID} {Server.Contact?.Email}\r\n";
        await Server.SendAsync(message);
    }
}