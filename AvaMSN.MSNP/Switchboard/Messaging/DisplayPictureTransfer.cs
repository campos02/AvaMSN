using AvaMSN.MSNP.Models;
using AvaMSN.MSNP.Models.Messages.MSNSLP;
using System.Security.Cryptography;
using System.Text;

namespace AvaMSN.MSNP.Switchboard.Messaging
{
    public class DisplayPictureTransfer : ISwitchboardWrapper
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

        /// <summary>
        /// Handles accepting a P2P invite to send a display picture and sending it.
        /// </summary>
        /// <param name="payload">P2P invite payload.</param>
        /// <returns></returns>
        public async Task SendDisplayPicture(byte[] payload)
        {
            if (Server == null)
                throw new NullReferenceException("Server is null");

            string payloadString = Encoding.UTF8.GetString(payload);
            string messageHeaders = payloadString.Split("\r\n\r\n")[0] + "\r\n\r\n";

            byte[] header = payload.Skip(Encoding.UTF8.GetBytes(messageHeaders).Length).Take(BinaryHeader.HeaderSize).ToArray();
            BinaryHeader binaryHeader = new BinaryHeader(header);

            string[] payloadHeaderParameters = payloadString.Split("\r\n\r\n")[1].Split("\r\n");
            string[] payloadBodyParameters = payloadString.Split("\r\n\r\n")[2].Split("\r\n");

            SendDisplayPicture displayPicture = new SendDisplayPicture
            {
                Identifier = BitConverter.ToUInt32(RandomNumberGenerator.GetBytes(sizeof(uint))),
                From = Server.User!.Email,
                Data = Server.User.DisplayPicture,
                Branch = payloadHeaderParameters[3].Replace("Via: MSNSLP/1.0/TLP ;branch=", ""),
                CSeq = Convert.ToInt32(payloadHeaderParameters[4].Replace("CSeq: ", "")),
                CallID = payloadHeaderParameters[5].Replace("Call-ID: ", "")
            };

            if (!messageHeaders.Split("\r\n")[2].Contains(Server.User.Email))
                return;

            displayPicture.To = payloadHeaderParameters[2].Split(":")[2].Replace(">", "");

            // Transfer only display pictures
            if (!payloadBodyParameters[0].Contains("{A4268EEC-FEC5-49E5-95C3-F126696BDBF6}"))
                return;

            displayPicture.SessionID = Convert.ToUInt32(payloadBodyParameters[1].Split(" ")[1]);
            string msnObject = Encoding.UTF8.GetString(Convert.FromBase64String(payloadBodyParameters.Last().Replace("Context: ", "")));
            if (!msnObject.Contains(Server.User.DisplayPictureObject!))
                return;

            // Send MSG with acknowledgement
            Server.TransactionID++;
            byte[] messagePayload = displayPicture.AcknowledgePayload(binaryHeader);
            byte[] message = Encoding.UTF8.GetBytes($"MSG {Server.TransactionID} D {messagePayload.Length}\r\n");
            await Server.SendAsync(message.Concat(messagePayload).ToArray());

            // Send MSG with 200 OK
            Server.TransactionID++;
            messagePayload = displayPicture.OkPayload();
            message = Encoding.UTF8.GetBytes($"MSG {Server.TransactionID} D {messagePayload.Length}\r\n");
            await Server.SendAsync(message.Concat(messagePayload).ToArray());

            while (true)
            {
                // Receive MSG with acknowledgement
                byte[] response = await Server.ReceiveAsync();
                string responseString = Encoding.UTF8.GetString(response);

                if (responseString.Contains("MSG"))
                {
                    string[] responses = responseString.Split("\r\n");

                    // Skip ACKs
                    while (responses[0].Contains("ACK"))
                    {
                        response = response.Skip(Encoding.UTF8.GetBytes(responses[0] + "\r\n").Length).ToArray();
                        responseString = Encoding.UTF8.GetString(response);
                        responses = responseString.Split("\r\n");
                    }

                    string[] parameters = responses[0].Split(" ");
                    int length = Convert.ToInt32(parameters[3]);
                    byte[] payloadResponse = response.Skip(Encoding.UTF8.GetBytes(responses[0] + "\r\n").Length).ToArray();
                    payload = new Span<byte>(payloadResponse, 0, length).ToArray();
                    payloadString = Encoding.UTF8.GetString(payload);

                    string[] payloadParameters = payloadString.Split("\r\n");
                    if (payloadParameters[1] == "Content-Type: application/x-msnmsgrp2p")
                    {
                        messageHeaders = payloadString.Split("\r\n\r\n")[0] + "\r\n\r\n";
                        if (messageHeaders.Split("\r\n")[2].Contains(Server.User.Email))
                            break;
                    }
                    else
                        await Server.HandleIncoming(response);
                }
                else
                    await Server.HandleIncoming(response);
            }

            // Send MSG with data preparation
            Server.TransactionID++;
            messagePayload = displayPicture.DataPreparationPayload();
            message = Encoding.UTF8.GetBytes($"MSG {Server.TransactionID} D {messagePayload.Length}\r\n");
            await Server.SendAsync(message.Concat(messagePayload).ToArray());

            while (true)
            {
                // Receive MSG with acknowledgement
                byte[] response = await Server.ReceiveAsync();
                string responseString = Encoding.UTF8.GetString(response);

                if (responseString.Contains("MSG"))
                {
                    string[] responses = responseString.Split("\r\n");

                    // Skip ACKs
                    while (responses[0].Contains("ACK"))
                    {
                        response = response.Skip(Encoding.UTF8.GetBytes(responses[0] + "\r\n").Length).ToArray();
                        responseString = Encoding.UTF8.GetString(response);
                        responses = responseString.Split("\r\n");
                    }

                    string[] parameters = responses[0].Split(" ");
                    int length = Convert.ToInt32(parameters[3]);
                    byte[] payloadResponse = response.Skip(Encoding.UTF8.GetBytes(responses[0] + "\r\n").Length).ToArray();
                    payload = new Span<byte>(payloadResponse, 0, length).ToArray();
                    payloadString = Encoding.UTF8.GetString(payload);

                    string[] payloadParameters = payloadString.Split("\r\n");
                    if (payloadParameters[1] == "Content-Type: application/x-msnmsgrp2p")
                    {
                        messageHeaders = payloadString.Split("\r\n\r\n")[0] + "\r\n\r\n";
                        if (messageHeaders.Split("\r\n")[2].Contains(Server.User.Email))
                            break;
                    }
                    else
                        await Server.HandleIncoming(response);
                }
                else
                    await Server.HandleIncoming(response);
            }

            // Send display picture data in chunks
            while (displayPicture.Data!.Length - displayPicture.DataOffset > 0)
            {
                Server.TransactionID++;
                messagePayload = displayPicture.DataPayload();

                // Send MSG with data chunk
                message = Encoding.UTF8.GetBytes($"MSG {Server.TransactionID} D {messagePayload.Length}\r\n");
                await Server.SendAsync(message.Concat(messagePayload).ToArray());
            }

            // Start receiving incoming commands again
            _ = Server.ReceiveIncomingAsync();
        }
    }
}
