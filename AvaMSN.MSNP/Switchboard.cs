using AvaMSN.MSNP.Exceptions;
using AvaMSN.MSNP.Messages;
using System.Text;

namespace AvaMSN.MSNP;

public partial class Switchboard : Connection
{
    public Profile Profile { get; set; } = new Profile();
    public Contact Contact { get; set; } = new Contact();
    public event EventHandler<DisplayPictureEventArgs>? DisplayPictureUpdated;
    private bool displayPictureInviteSent;

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
            string response = await ReceiveStringAsync();

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
            string response = await ReceiveStringAsync();

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
            string response = await ReceiveStringAsync();

            if (response.StartsWith("CAL")
                && response.Split(" ")[1] == TransactionID.ToString())
            {
                break;
            }
        }

        _ = ReceiveIncomingAsync();
    }

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
            string response = await ReceiveStringAsync();

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

    public async Task SendDisplayPictureInvite()
    {
        if (displayPictureInviteSent)
            throw new CommandException("Display picture invite has already been sent");

        displayPictureInviteSent = true;

        if (Contact.DisplayPictureObject == null)
            return;

        ReceiveDisplayPicture displayPicture = new ReceiveDisplayPicture()
        {
            To = Contact.Email,
            From = Profile.Email
        };

        TransactionID++;
        byte[] messagePayload = displayPicture.InvitePayload(Contact.DisplayPictureObject!);
        BinaryHeader binaryHeader;

        // Send MSG and invitation
        byte[] message = Encoding.UTF8.GetBytes($"MSG {TransactionID} D {messagePayload.Length}\r\n");
        await SendAsync(message.Concat(messagePayload).ToArray());

        while (true)
        {
            // Receive MSG with acknowledgement
            byte[] response = await ReceiveAsync();
            string responseString = Encoding.UTF8.GetString(response);

            string[] responses = responseString.Split("\r\n");
            if (responses[0].Contains("ACK"))
                responses = responses.Skip(1).ToArray();

            if (responseString.Contains("MSG"))
            {
                string[] parameters = responses[0].Split(" ");

                int length = Convert.ToInt32(parameters[3]);

                byte[] payloadResponse = response.Skip(Encoding.UTF8.GetBytes(responses[0] + "\r\n").Length).ToArray();
                byte[] payload = new Span<byte>(payloadResponse, 0, length).ToArray();
                string payloadString = Encoding.UTF8.GetString(payload);

                string[] payloadParameters = payloadString.Split("\r\n");

                if (payloadParameters[1] == "Content-Type: application/x-msnmsgrp2p")
                {
                    string messageHeaders = payloadString.Split("\r\n\r\n")[0] + "\r\n\r\n";
                    string[] payloadHeaderParameters = payloadString.Split("\r\n\r\n")[1].Split("\r\n");

                    if (messageHeaders.Split("\r\n")[2].Contains(Profile.Email) && payloadHeaderParameters[0].Contains("200 OK"))
                    {
                        byte[] header = payload.Skip(Encoding.UTF8.GetBytes(messageHeaders).Length).Take(48).ToArray();
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

        // Send MSG and acknowledgement
        message = Encoding.UTF8.GetBytes($"MSG {TransactionID} D {messagePayload.Length}\r\n");
        await SendAsync(message.Concat(messagePayload).ToArray());

        while (true)
        {
            // Receive MSG with data preparation
            byte[] response = await ReceiveAsync();
            string responseString = Encoding.UTF8.GetString(response);

            string[] responses = responseString.Split("\r\n");
            if (responses[0].Contains("ACK"))
                responses = responses.Skip(1).ToArray();

            if (responseString.Contains("MSG"))
            {
                string[] parameters = responses[0].Split(" ");

                int length = Convert.ToInt32(parameters[3]);

                byte[] payloadResponse = response.Skip(Encoding.UTF8.GetBytes(responses[0] + "\r\n").Length).ToArray();
                byte[] payload = new Span<byte>(payloadResponse, 0, length).ToArray();
                string payloadString = Encoding.UTF8.GetString(payload);

                string[] payloadParameters = payloadString.Split("\r\n");

                if (payloadParameters[1] == "Content-Type: application/x-msnmsgrp2p")
                {
                    string messageHeaders = payloadString.Split("\r\n\r\n")[0] + "\r\n\r\n";

                    if (messageHeaders.Split("\r\n")[2].Contains(Profile.Email))
                    {
                        byte[] header = payload.Skip(Encoding.UTF8.GetBytes(messageHeaders).Length).Take(48).ToArray();
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

        // Send MSG and acknowledgement
        message = Encoding.UTF8.GetBytes($"MSG {TransactionID} D {messagePayload.Length}\r\n");
        await SendAsync(message.Concat(messagePayload).ToArray());

        byte[] dataResponse = Array.Empty<byte>();
        byte[] picture = Array.Empty<byte>();
        while (true)
        {   
            if (dataResponse.Length == 0)
            {
                // Receive MSG with data
                dataResponse = await ReceiveAsync();
            }

            string responseString = Encoding.UTF8.GetString(dataResponse);

            if (responseString.Contains("MSG"))
            {
                if (!responseString.StartsWith("MSG"))
                {
                    int msgIndex = IndexOf(dataResponse, Encoding.UTF8.GetBytes("MSG"));
                    picture = picture.Concat(dataResponse.Take(msgIndex).ToArray()).ToArray();
                    dataResponse = dataResponse.Skip(msgIndex).ToArray();
                    responseString = Encoding.UTF8.GetString(dataResponse);
                }

                string[] responses = responseString.Split("\r\n");
                string[] parameters = responses[0].Split(" ");

                int length = Convert.ToInt32(parameters[3]);

                if (length > dataResponse.Length)
                {
                    dataResponse = dataResponse.Concat(await ReceiveAsync()).ToArray();
                }

                byte[] payloadResponse = dataResponse.Skip(Encoding.UTF8.GetBytes(responses[0] + "\r\n").Length).ToArray();
                byte[] payload = new Span<byte>(payloadResponse, 0, length).ToArray();
                string payloadString = Encoding.UTF8.GetString(payload);
                string[] payloadParameters = payloadString.Split("\r\n");

                dataResponse = payloadResponse.Skip(length).ToArray();

                if (payloadParameters[1] == "Content-Type: application/x-msnmsgrp2p")
                {
                    string messageHeaders = payloadString.Split("\r\n\r\n")[0] + "\r\n\r\n";

                    if (messageHeaders.Split("\r\n")[2].Contains(Profile.Email))
                    {
                        byte[] binaryPayload = payload.Skip(Encoding.UTF8.GetBytes(messageHeaders).Length).ToArray();
                        byte[] header = binaryPayload.Take(48).ToArray();
                        binaryPayload = binaryPayload.Skip(48).ToArray();

                        binaryHeader = new BinaryHeader(header);
                        binaryPayload = binaryPayload[..^4];
                        picture = picture.Concat(binaryPayload).ToArray();

                        if (binaryHeader.DataOffset + binaryHeader.Length == binaryHeader.DataSize)
                        {
                            break;
                        }
                    }
                    else
                    {
                        await HandleIncoming(dataResponse);
                        dataResponse = Array.Empty<byte>();
                    }
                }
            }
            else
            {
                await HandleIncoming(dataResponse);
                dataResponse = Array.Empty<byte>();
            }
        }

        Contact.DisplayPicture = picture;
        DisplayPictureUpdated?.Invoke(this, new DisplayPictureEventArgs()
        {
            Email = Contact.Email,
            DisplayPicture = Contact.DisplayPicture
        });

        TransactionID++;
        messagePayload = displayPicture.ByePayload();

        // Send MSG and Bye
        message = Encoding.UTF8.GetBytes($"MSG {TransactionID} D {messagePayload.Length}\r\n");
        await SendAsync(message.Concat(messagePayload).ToArray());

        _ = ReceiveIncomingAsync();
    }

    /// <summary>
    /// Returns the first index of a subarray inside an array
    /// </summary>
    private static int IndexOf(byte[] array, byte[] subArray)
    {
        for (int i = 0; i < array.Length; i++)
        {
            if (array.Skip(i).Take(subArray.Length).SequenceEqual(subArray))
                return i;
        }

        return -1;
    }
}
