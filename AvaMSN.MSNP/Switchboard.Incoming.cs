using AvaMSN.MSNP.Messages;
using AvaMSN.MSNP.Messages.MSNSLP;
using System.Security.Cryptography;
using System.Text;
using AvaMSN.MSNP.Utils;

namespace AvaMSN.MSNP;

public partial class Switchboard : Connection
{
    public event EventHandler<MessageEventArgs>? MessageReceived;
    public event EventHandler<DisplayPictureEventArgs>? DisplayPictureUpdated;

    /// <summary>
    /// Overload that takes a string response and converts to a byte array.
    /// </summary>
    /// <param name="response">Incoming response.</param>
    private async Task HandleIncoming(string response)
    {
        await HandleIncoming(Encoding.UTF8.GetBytes(response));
    }
    
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
        byte[] payloadResponse = response.Skip(Encoding.UTF8.GetByteCount(responses[0] + "\r\n")).ToArray();
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
                        Message = new TextPlain()
                        {
                            Text = $"{Uri.UnescapeDataString(parameters[2])} just sent you a nudge!"
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
                Text = payloadString[payloadString.IndexOf(payloadParameters[4])..].Replace("\r\n", "\n")
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
                await HandleP2PInvite(payload);

            if (payloadParameters.Length > 5)
                if (payloadParameters[5].Contains("INVITE"))
                    await HandleP2PInvite(payload);
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
}
