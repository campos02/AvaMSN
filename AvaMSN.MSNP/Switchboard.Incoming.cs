using AvaMSN.MSNP.Messages;
using AvaMSN.MSNP.Messages.MSNSLP;
using System.Security.Cryptography;
using System.Text;

namespace AvaMSN.MSNP;

public partial class Switchboard : Connection
{
    public event EventHandler<MessageEventArgs>? MessageReceived;
    public event EventHandler<DisplayPictureEventArgs>? DisplayPictureUpdated;

    /// <summary>
    /// Handles responses that aren't the result of a command.
    /// </summary>
    /// <param name="response">Incoming response.</param>
    /// <returns></returns>
    protected override async Task HandleIncoming(byte[] response)
    {
        string responseString = Encoding.UTF8.GetString(response);
        string command = responseString.Split(" ")[0];

        await (command switch
        {
            "MSG" => HandleMSG(response),
            "BYE" => DisconnectAsync(),
            _ => Task.CompletedTask
        });
    }

    /// <summary>
    /// Handles an MSG command.
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    private async Task HandleMSG(byte[] response)
    {
        ResetTimeout();
        string responseString = Encoding.UTF8.GetString(response);

        string[] responses = responseString.Split("\r\n");
        string[] parameters = responses[0].Split(" ");

        int length = Convert.ToInt32(parameters[3]);

        // Get payload
        byte[] payloadResponse = response.Skip(Encoding.UTF8.GetBytes(responses[0] + "\r\n").Length).ToArray();
        byte[] payload = new Span<byte>(payloadResponse, 0, length).ToArray();
        string payloadString = Encoding.UTF8.GetString(payload);

        string[] payloadParameters = payloadString.Split("\r\n");

        // Handle "is writing..." notification
        if (payloadParameters[1] == "Content-Type: text/x-msmsgscontrol")
        {
            if (payloadParameters[2].Contains("TypingUser"))
            {
                string user = payloadParameters[2].Split(" ")[1];

                MessageReceived?.Invoke(this, new MessageEventArgs()
                {
                    Email = user,
                    TypingUser = true
                });
            }
        }

        // Handle nudge
        if (payloadParameters[1] == "Content-Type: text/x-msnmsgr-datacast")
        {
            if (payloadParameters[3].Contains("ID:"))
            {
                string ID = payloadParameters[3].Split(" ")[1];

                if (ID == "1")
                {
                    MessageReceived?.Invoke(this, new MessageEventArgs()
                    {
                        Email = parameters[1],

                        Message = new Messages.TextPlain()
                        {
                            Content = $"{Uri.UnescapeDataString(parameters[2])} just sent you a nudge!"
                        },

                        IsNudge = true
                    });
                }
            }
        }

        // Handle plain text
        if (payloadParameters[1].Contains("Content-Type: text/plain"))
        {
            TextPlain message = new TextPlain()
            {
                Content = payloadParameters[4]
            };

            message.SetFormatting(payloadParameters[2]);

            MessageReceived?.Invoke(this, new MessageEventArgs()
            {
                Email = parameters[1],
                Message = message
            });
        }

        // Handle P2P message
        if (payloadParameters[1] == "Content-Type: application/x-msnmsgrp2p")
        {
            if (payloadParameters[4].Contains("INVITE"))
            {
                await HandleP2PInvite(payload);
            }

            if (payloadParameters.Length > 5)
            {
                if (payloadParameters[5].Contains("INVITE"))
                {
                    await HandleP2PInvite(payload);
                }
            }
        }
    }

    /// <summary>
    /// Handles accepting a P2P invite to send a display picture and sending it.
    /// </summary>
    /// <param name="payload">P2P invite payload.</param>
    /// <returns></returns>
    private async Task HandleP2PInvite(byte[] payload)
    {
        string payloadString = Encoding.UTF8.GetString(payload);
        string messageHeaders = payloadString.Split("\r\n\r\n")[0] + "\r\n\r\n";

        byte[] header = payload.Skip(Encoding.UTF8.GetBytes(messageHeaders).Length).Take(BinaryHeader.HeaderSize).ToArray();
        BinaryHeader binaryHeader = new BinaryHeader(header);

        string[] payloadHeaderParameters = payloadString.Split("\r\n\r\n")[1].Split("\r\n");
        string[] payloadBodyParameters = payloadString.Split("\r\n\r\n")[2].Split("\r\n");

        SendDisplayPicture displayPicture = new SendDisplayPicture()
        {
            Identifier = BitConverter.ToUInt32(RandomNumberGenerator.GetBytes(sizeof(uint))),
            From = Profile.Email,
            Data = Profile.DisplayPicture,
            Branch = payloadHeaderParameters[3].Replace("Via: MSNSLP/1.0/TLP ;branch=", ""),
            CSeq = Convert.ToInt32(payloadHeaderParameters[4].Replace("CSeq: ", "")),
            CallID = payloadHeaderParameters[5].Replace("Call-ID: ", "")
        };

        if (!messageHeaders.Split("\r\n")[2].Contains(Profile.Email))
            return;

        displayPicture.To = payloadHeaderParameters[2].Split(":")[2].Replace(">", "");

        // Transfer only display pictures
        if (!payloadBodyParameters[0].Contains("{A4268EEC-FEC5-49E5-95C3-F126696BDBF6}"))
            return;

        displayPicture.SessionID = Convert.ToUInt32(payloadBodyParameters[1].Split(" ")[1]);

        string msnObject = Encoding.UTF8.GetString(Convert.FromBase64String(payloadBodyParameters[4].Replace("Context: ", "")));
        if (!msnObject.Contains(Profile.DisplayPictureObject!))
            return;

        TransactionID++;
        byte[] messagePayload = displayPicture.AcknowledgePayload(binaryHeader);

        // Send MSG with acknowledgement
        byte[] message = Encoding.UTF8.GetBytes($"MSG {TransactionID} D {messagePayload.Length}\r\n");
        await SendAsync(message.Concat(messagePayload).ToArray());

        TransactionID++;
        messagePayload = displayPicture.OkPayload();

        // Send MSG with 200 OK
        message = Encoding.UTF8.GetBytes($"MSG {TransactionID} D {messagePayload.Length}\r\n");
        await SendAsync(message.Concat(messagePayload).ToArray());

        while (true)
        {
            // Receive MSG with acknowledgement
            byte[] response = await ReceiveAsync();
            string responseString = Encoding.UTF8.GetString(response);

            if (responseString.Contains("MSG"))
            {
                string[] responses = responseString.Split("\r\n");
                if (responses[0].Contains("ACK"))
                    responses = responses.Skip(1).ToArray();

                string[] parameters = responses[0].Split(" ");

                int length = Convert.ToInt32(parameters[3]);

                byte[] payloadResponse = response.Skip(Encoding.UTF8.GetBytes(responses[0] + "\r\n").Length).ToArray();
                payload = new Span<byte>(payloadResponse, 0, length).ToArray();
                payloadString = Encoding.UTF8.GetString(payload);

                string[] payloadParameters = payloadString.Split("\r\n");

                if (payloadParameters[1] == "Content-Type: application/x-msnmsgrp2p")
                {
                    messageHeaders = payloadString.Split("\r\n\r\n")[0] + "\r\n\r\n";

                    if (messageHeaders.Split("\r\n")[2].Contains(Profile.Email))
                    {
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
        messagePayload = displayPicture.DataPreparationPayload();

        // Send MSG with data preparation
        message = Encoding.UTF8.GetBytes($"MSG {TransactionID} D {messagePayload.Length}\r\n");
        await SendAsync(message.Concat(messagePayload).ToArray());

        while (true)
        {
            // Receive MSG with acknowledgement
            byte[] response = await ReceiveAsync();
            string responseString = Encoding.UTF8.GetString(response);

            if (responseString.Contains("MSG"))
            {
                string[] responses = responseString.Split("\r\n");
                if (responses[0].Contains("ACK"))
                    responses = responses.Skip(1).ToArray();

                string[] parameters = responses[0].Split(" ");

                int length = Convert.ToInt32(parameters[3]);

                byte[] payloadResponse = response.Skip(Encoding.UTF8.GetBytes(responses[0] + "\r\n").Length).ToArray();
                payload = new Span<byte>(payloadResponse, 0, length).ToArray();
                payloadString = Encoding.UTF8.GetString(payload);

                string[] payloadParameters = payloadString.Split("\r\n");

                if (payloadParameters[1] == "Content-Type: application/x-msnmsgrp2p")
                {
                    messageHeaders = payloadString.Split("\r\n\r\n")[0] + "\r\n\r\n";

                    if (messageHeaders.Split("\r\n")[2].Contains(Profile.Email))
                    {
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

        // Send display picture data in chunks
        while (displayPicture.Data!.Length - displayPicture.DataOffset > 0)
        {
            TransactionID++;
            messagePayload = displayPicture.DataPayload();

            // Send MSG with data chunk
            message = Encoding.UTF8.GetBytes($"MSG {TransactionID} D {messagePayload.Length}\r\n");
            await SendAsync(message.Concat(messagePayload).ToArray());
        }

        // Start receiving incoming commands again
        _ = ReceiveIncomingAsync();
    }

    /// <summary>
    /// Creates a P2P session to receive a contact's display picture data.
    /// </summary>
    /// <returns></returns>
    public async Task ReceiveDisplayPicture()
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
                        {
                            break;
                        }
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
            DisplayPicture = Contact.DisplayPicture
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
}
