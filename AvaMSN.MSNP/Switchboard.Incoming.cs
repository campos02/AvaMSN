using AvaMSN.MSNP.Messages;
using System.Security.Cryptography;
using System.Text;

namespace AvaMSN.MSNP;

public partial class Switchboard : Connection
{
    public event EventHandler<MessageEventArgs>? MessageReceived;

    protected override async Task HandleIncoming(byte[] response)
    {
        string responseString = Encoding.UTF8.GetString(response);
        string command = responseString.Split(" ")[0];

        await (command switch
        {
            "JOI" => HandleJOI(),
            "MSG" => HandleMSG(response),
            "BYE" => HandleBYE(),
            _ => Task.CompletedTask
        });
    }

    private async Task HandleJOI()
    {
        await SendDisplayPictureInvite();
    }

    private async Task HandleMSG(byte[] response)
    {
        string responseString = Encoding.UTF8.GetString(response);

        string[] responses = responseString.Split("\r\n");
        string[] parameters = responses[0].Split(" ");

        int length = Convert.ToInt32(parameters[3]);

        byte[] payloadResponse = response.Skip(Encoding.UTF8.GetBytes(responses[0] + "\r\n").Length).ToArray();
        byte[] payload = new Span<byte>(payloadResponse, 0, length).ToArray();
        string payloadString = Encoding.UTF8.GetString(payload);

        string[] payloadParameters = payloadString.Split("\r\n");

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
                        Message = $"{Uri.UnescapeDataString(parameters[2])} just sent you a nudge!",
                        IsNudge = true
                    });
                }
            }
        }

        if (payloadParameters[1].Contains("Content-Type: text/plain"))
        {
            MessageReceived?.Invoke(this, new MessageEventArgs()
            {
                Email = parameters[1],
                Message = payloadParameters[4]
            });
        }

        if (payloadParameters[1] == "Content-Type: application/x-msnmsgrp2p")
        {
            if (payloadParameters[3].Contains("INVITE") || payloadParameters[5].Contains("INVITE"))
            {
                await HandleP2PInvite(payload);
            }
        }

        if (!displayPictureInviteSent)
            await SendDisplayPictureInvite();
    }

    private async Task HandleP2PInvite(byte[] payload)
    {
        string payloadString = Encoding.UTF8.GetString(payload);
        string messageHeaders = payloadString.Split("\r\n\r\n")[0] + "\r\n\r\n";

        byte[] binaryHeader = payload.Skip(Encoding.UTF8.GetBytes(messageHeaders).Length).Take(48).ToArray();
        BinaryHeader ackHeader = new BinaryHeader(binaryHeader);

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

        if (!payloadBodyParameters[0].Contains("{A4268EEC-FEC5-49E5-95C3-F126696BDBF6}"))
            return;

        displayPicture.SessionID = Convert.ToUInt32(payloadBodyParameters[1].Split(" ")[1]);

        string base64Object = Convert.ToBase64String(Encoding.UTF8.GetBytes(Profile.DisplayPictureObject + char.MinValue));
        if (!payloadBodyParameters[4].Contains(base64Object))
            return;

        TransactionID++;
        byte[] messagePayload = displayPicture.AcknowledgePayload(ackHeader);

        // Send MSG and acknowledgement
        byte[] message = Encoding.UTF8.GetBytes($"MSG {TransactionID} D {messagePayload.Length}\r\n");
        await SendAsync(message.Concat(messagePayload).ToArray());

        TransactionID++;
        messagePayload = displayPicture.OkPayload();

        // Send MSG and 200 OK
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
            }

            await HandleIncoming(response);
        }

        TransactionID++;
        messagePayload = displayPicture.DataPreparationPayload();

        // Send MSG and data preparation
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
            }

            await HandleIncoming(response);
        }

        while (displayPicture.Data!.Length - displayPicture.DataOffset > 0)
        {
            TransactionID++;
            messagePayload = displayPicture.DataPayload();

            // Send MSG and data chunk
            message = Encoding.UTF8.GetBytes($"MSG {TransactionID} D {messagePayload.Length}\r\n");
            await SendAsync(message.Concat(messagePayload).ToArray());
        }

        _ = ReceiveIncomingAsync();
    }

    private async Task HandleBYE()
    {
        await DisconnectAsync();
    }
}
