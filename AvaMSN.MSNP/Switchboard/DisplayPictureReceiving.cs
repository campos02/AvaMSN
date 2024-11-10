using System.Text;
using AvaMSN.MSNP.Models;
using AvaMSN.MSNP.Models.Messages.MSNSLP;

namespace AvaMSN.MSNP.Switchboard;

/// <summary>
/// Contains methods for receiving a display picture from a contact via a P2P transfer.
/// </summary>
public class DisplayPictureReceiving : ISwitchboardWrapper
{
    public Switchboard? Server { get; set; }
    public event EventHandler<DisplayPictureEventArgs>? DisplayPictureUpdated;
    
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
    /// Creates a P2P session to receive a contact's display picture data.
    /// </summary>
    /// <returns></returns>
    public async Task GetDisplayPicture()
    {
        if (Server == null)
            throw new NullReferenceException("Server is null");
        
        if (Server.Contact?.DisplayPictureObject == null)
            return;

        ReceiveDisplayPicture displayPicture = new ReceiveDisplayPicture
        {
            To = Server.Contact.Email,
            From = Server.User?.Email ?? string.Empty
        };

        // Send MSG with invitation
        Server.TransactionID++;
        byte[] messagePayload = displayPicture.InvitePayload(Server.Contact.DisplayPictureObject);
        byte[] message = Encoding.UTF8.GetBytes($"MSG {Server.TransactionID} D {messagePayload.Length}\r\n");
        await Server.SendAsync(message.Concat(messagePayload).ToArray());

        BinaryHeader binaryHeader;
        byte[] response;
        while (true)
        {
            // Receive MSG with acknowledgement
            response = await Server.ReceiveAsync();
            string responseString = Encoding.UTF8.GetString(response);
            string[] responses = responseString.Split("\r\n");

            // Skip ACKs
            while (responses[0].Contains("ACK"))
            {
                response = response.Skip(Encoding.UTF8.GetBytes(responses[0] + "\r\n").Length).ToArray();
                responseString = Encoding.UTF8.GetString(response);
                responses = responseString.Split("\r\n");
            }

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

                    if (messageHeaders.Split("\r\n")[2].Contains(Server.User!.Email) && payloadHeaderParameters[0].Contains("200 OK"))
                    {
                        byte[] header = payload.Skip(Encoding.UTF8.GetBytes(messageHeaders).Length).Take(BinaryHeader.HeaderSize).ToArray();
                        binaryHeader = new BinaryHeader(header);
                        break;
                    }
                }
                else
                    await Server.HandleIncoming(response);
            }
            else
                await Server.HandleIncoming(response);
        }

        // Send MSG with acknowledgement
        Server.TransactionID++;
        messagePayload = displayPicture.AcknowledgePayload(binaryHeader);
        message = Encoding.UTF8.GetBytes($"MSG {Server.TransactionID} D {messagePayload.Length}\r\n");
        await Server.SendAsync(message.Concat(messagePayload).ToArray());

        byte[] picture = [];
        while (true)
        {
            if (response.Length == 0)
            {
                // Receive MSG with data
                response = await Server.ReceiveAsync();
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
                    response = response.Concat(await Server.ReceiveAsync()).ToArray();
                    continue;
                }

                int length = Convert.ToInt32(parameters[3]);
                if (length > response.Length - Encoding.UTF8.GetBytes(responses[0] + "\r\n").Length)
                {
                    response = response.Concat(await Server.ReceiveAsync()).ToArray();
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
                    if (messageHeaders.Split("\r\n")[2].Contains(Server.User.Email))
                    {
                        byte[] binaryPayload = payload.Skip(Encoding.UTF8.GetBytes(messageHeaders).Length).ToArray();
                        byte[] header = binaryPayload.Take(BinaryHeader.HeaderSize).ToArray();

                        binaryPayload = binaryPayload.Skip(BinaryHeader.HeaderSize).ToArray();
                        binaryHeader = new BinaryHeader(header);
                        binaryPayload = binaryPayload[..^4];

                        // Handle data preparation
                        if (binaryPayload.Length == 4)
                        {
                            // Send MSG with acknowledgement
                            Server.TransactionID++;
                            messagePayload = displayPicture.AcknowledgePayload(binaryHeader);
                            message = Encoding.UTF8.GetBytes($"MSG {Server.TransactionID} D {messagePayload.Length}\r\n");
                            await Server.SendAsync(message.Concat(messagePayload).ToArray());

                            continue;
                        }

                        picture = picture.Concat(binaryPayload).ToArray();
                        if (binaryHeader.DataOffset + binaryHeader.Length == binaryHeader.DataSize)
                            break;
                    }
                    else
                    {
                        await Server.HandleIncoming(response);
                        response = [];
                    }
                }
            }
            else
            {
                await Server.HandleIncoming(response);
                response = [];
            }
        }

        // Send MSG and BYE
        Server.TransactionID++;
        messagePayload = displayPicture.ByePayload();
        message = Encoding.UTF8.GetBytes($"MSG {Server.TransactionID} D {messagePayload.Length}\r\n");
        await Server.SendAsync(message.Concat(messagePayload).ToArray());

        // Set display picture and invoke picture event
        Server.Contact.DisplayPicture = picture;
        DisplayPictureUpdated?.Invoke(this, new DisplayPictureEventArgs
        {
            Email = Server.Contact.Email,
            DisplayPicture = Server.Contact.DisplayPicture,
            DisplayPictureHash = Server.Contact.DisplayPictureHash
        });

        // Start receiving incoming commands again
        _ = Server.ReceiveIncomingAsync();
    }
}