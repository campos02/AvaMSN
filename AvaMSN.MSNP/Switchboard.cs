using AvaMSN.MSNP.Exceptions;
using AvaMSN.MSNP.Messages;
using AvaMSN.MSNP.Messages.MSNSLP;
using System.Text;
using System.Timers;
using AvaMSN.MSNP.Utils;
using Serilog;

namespace AvaMSN.MSNP;

/// <summary>
/// Represents a connection to the Switchboard (SB).
/// </summary>
public partial class Switchboard : Connection
{
    public Profile Profile { get; set; } = new Profile();
    public Contact Contact { get; set; } = new Contact();
    private readonly System.Timers.Timer timeout;

    public Switchboard()
    {
        // Set 10 minute timeout
        timeout = new System.Timers.Timer(600000)
        {
            AutoReset = false
        };
        timeout.Elapsed += Timer_Elapsed;
    }

    protected override async Task Connect()
    {
        await base.Connect();
        ResetTimeout();
        Log.Information("Connected to SB {Server} on port {Port}", Host, Port);
    }

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

            if (response.Contains("911"))
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
                    await HandleIncoming(command);

                // Break if response is a command reply
                if (ansResponse.Split(" ")[1] == TransactionID.ToString())
                    break;
            }

            else if (response.Contains("911"))
                throw new AuthException("Authentication failed");
        }

        // Start receiving incoming commands
        _ = ReceiveIncomingAsync();
    }

    /// <summary>
    /// Invites a contact into the session
    /// </summary>
    /// <returns></returns>
    public async Task SendCAL()
    {
        // Send CAL
        TransactionID++;
        string message = $"CAL {TransactionID} {Contact.Email}\r\n";
        await SendAsync(message);
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

        ResetTimeout();
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
        ResetTimeout();
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

    /// <summary>
    /// Creates a P2P session to receive a contact's display picture data.
    /// </summary>
    /// <returns></returns>
    public async Task GetDisplayPicture()
    {
        if (Contact.DisplayPictureObject == null)
            return;

        ReceiveDisplayPicture displayPicture = new ReceiveDisplayPicture()
        {
            To = Contact.Email,
            From = Profile.Email
        };

        TransactionID++;
        byte[] messagePayload = displayPicture.InvitePayload(Contact.DisplayPictureObject);
        BinaryHeader binaryHeader;

        // Send MSG with invitation
        byte[] message = Encoding.UTF8.GetBytes($"MSG {TransactionID} D {messagePayload.Length}\r\n");
        await SendAsync(message.Concat(messagePayload).ToArray());

        byte[] response;
        while (true)
        {
            // Receive MSG with acknowledgement
            response = await ReceiveAsync();
            string responseString = Encoding.UTF8.GetString(response);

            string[] responses = responseString.Split("\r\n");
            if (responses[0].Contains("ACK"))
                responses = responses.Skip(1).ToArray();

            if (responseString.Contains("MSG"))
            {
                string[] parameters = responses[0].Split(" ");
                int length = Convert.ToInt32(parameters[3]);

                response = response.Skip(Encoding.UTF8.GetBytes(responses[0] + "\r\n").Length).ToArray();
                byte[] payload = new Span<byte>(response, 0, length).ToArray();
                string payloadString = Encoding.UTF8.GetString(payload);
                string[] payloadParameters = payloadString.Split("\r\n");

                response = response.Skip(length).ToArray();
                if (payloadParameters[1] == "Content-Type: application/x-msnmsgrp2p")
                {
                    string messageHeaders = payloadString.Split("\r\n\r\n")[0] + "\r\n\r\n";
                    string[] payloadHeaderParameters = payloadString.Split("\r\n\r\n")[1].Split("\r\n");
                    if (messageHeaders.Split("\r\n")[2].Contains(Profile.Email) && payloadHeaderParameters[0].Contains("200 OK"))
                    {
                        byte[] header = payload.Skip(Encoding.UTF8.GetBytes(messageHeaders).Length).Take(BinaryHeader.HeaderSize).ToArray();
                        binaryHeader = new BinaryHeader(header);
                        break;
                    }
                }
                else
                {
                    await HandleIncoming(response);
                }
            }
            else
            {
                await HandleIncoming(response);
            }
        }

        TransactionID++;
        messagePayload = displayPicture.AcknowledgePayload(binaryHeader);

        // Send MSG with acknowledgement
        message = Encoding.UTF8.GetBytes($"MSG {TransactionID} D {messagePayload.Length}\r\n");
        await SendAsync(message.Concat(messagePayload).ToArray());

        byte[] picture = Array.Empty<byte>();
        while (true)
        {
            if (response.Length == 0)
            {
                // Receive MSG with data
                response = await ReceiveAsync();
            }

            string responseString = Encoding.UTF8.GetString(response);
            if (responseString.Contains("MSG"))
            {
                if (!responseString.StartsWith("MSG"))
                {
                    int msgIndex = IndexOf(response, Encoding.UTF8.GetBytes("MSG"));
                    response = response.Skip(msgIndex).ToArray();
                    continue;
                }

                string[] responses = responseString.Split("\r\n");
                string[] parameters = responses[0].Split(" ");
                if (parameters.Length < 4)
                {
                    response = response.Concat(await ReceiveAsync()).ToArray();
                    continue;
                }

                int length = Convert.ToInt32(parameters[3]);
                if (length > response.Length - Encoding.UTF8.GetBytes(responses[0] + "\r\n").Length)
                {
                    response = response.Concat(await ReceiveAsync()).ToArray();
                    continue;
                }

                response = response.Skip(Encoding.UTF8.GetBytes(responses[0] + "\r\n").Length).ToArray();
                byte[] payload = new Span<byte>(response, 0, length).ToArray();
                string payloadString = Encoding.UTF8.GetString(payload);
                string[] payloadParameters = payloadString.Split("\r\n");
                response = response.Skip(length).ToArray();

                // Ignore invites other than the current one
                if (payloadString.Contains("INVITE"))
                    continue;

                if (payloadParameters[1] == "Content-Type: application/x-msnmsgrp2p")
                {
                    string messageHeaders = payloadString.Split("\r\n\r\n")[0] + "\r\n\r\n";
                    if (messageHeaders.Split("\r\n")[2].Contains(Profile.Email))
                    {
                        byte[] binaryPayload = payload.Skip(Encoding.UTF8.GetBytes(messageHeaders).Length).ToArray();
                        byte[] header = binaryPayload.Take(BinaryHeader.HeaderSize).ToArray();
                        binaryPayload = binaryPayload.Skip(BinaryHeader.HeaderSize).ToArray();
                        binaryHeader = new BinaryHeader(header);
                        binaryPayload = binaryPayload[..^4];

                        // Handle data preparation
                        if (binaryPayload.Length == 4)
                        {
                            TransactionID++;
                            messagePayload = displayPicture.AcknowledgePayload(binaryHeader);

                            // Send MSG with acknowledgement
                            message = Encoding.UTF8.GetBytes($"MSG {TransactionID} D {messagePayload.Length}\r\n");
                            await SendAsync(message.Concat(messagePayload).ToArray());

                            continue;
                        }

                        picture = picture.Concat(binaryPayload).ToArray();
                        if (binaryHeader.DataOffset + binaryHeader.Length == binaryHeader.DataSize)
                            break;
                    }
                    else
                    {
                        await HandleIncoming(response);
                        response = Array.Empty<byte>();
                    }
                }
            }
            else
            {
                await HandleIncoming(response);
                response = Array.Empty<byte>();
            }
        }

        TransactionID++;
        messagePayload = displayPicture.ByePayload();

        // Send MSG and BYE
        message = Encoding.UTF8.GetBytes($"MSG {TransactionID} D {messagePayload.Length}\r\n");
        await SendAsync(message.Concat(messagePayload).ToArray());

        // Set display picture and invoke picture event
        Contact.DisplayPicture = picture;
        DisplayPictureUpdated?.Invoke(this, new DisplayPictureEventArgs()
        {
            Email = Contact.Email,
            DisplayPicture = Contact.DisplayPicture,
            DisplayPictureHash = Contact.DisplayPictureHash
        });

        // Start receiving incoming commands again
        _ = ReceiveIncomingAsync();
    }

    /// <summary>
    /// Returns the first index of a subarray inside an array.
    /// </summary>
    /// <param name="array"></param>
    /// <param name="subArray"></param>
    /// <returns>First index of subarray in array or -1 if not found.</returns>
    private static int IndexOf(byte[] array, byte[] subArray)
    {
        for (int i = 0; i < array.Length; i++)
        {
            if (array.Skip(i).Take(subArray.Length).SequenceEqual(subArray))
                return i;
        }

        return -1;
    }

    /// <summary>
    /// Starts or restarts the timeout timer.
    /// </summary>
    private void ResetTimeout()
    {
        timeout.Stop();
        timeout.Start();
    }

    public override async Task DisconnectAsync(bool requested = true)
    {
        await base.DisconnectAsync(requested);
        timeout.Stop();
    }

    private async void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        Log.Information("SB {Server} on port {Port} has timed out", Host, Port);
        await DisconnectAsync();
    }
}
