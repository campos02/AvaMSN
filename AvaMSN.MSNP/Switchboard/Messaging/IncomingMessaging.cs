using System.Text;
using AvaMSN.MSNP.Messages;
using AvaMSN.MSNP.Models;

namespace AvaMSN.MSNP.Switchboard.Messaging;

/// <summary>
/// Handles receiving messages.
/// </summary>
public class IncomingMessaging
{
    public Switchboard? Server { get; init; }
    public DisplayPictureTransfer? DisplayPictureTransfer { get; set; }
    public event EventHandler? ContactJoined;
    public event EventHandler<MessageEventArgs>? MessageReceived;

    /// <summary>
    /// Handles responses that aren't the result of a command.
    /// </summary>
    /// <param name="response">Incoming response.</param>
    /// <returns></returns>
    internal async Task HandleIncoming(byte[] response)
    {
        string responseString = Encoding.UTF8.GetString(response);
        string command = responseString.Split(" ")[0];

        await (command switch
        {
            "JOI" => Task.Run(HandleJOI),
            "MSG" => HandleMSG(response),
            _ => Task.CompletedTask
        });
    }

    /// <summary>
    /// Handles a JOI command.
    /// </summary>
    private void HandleJOI()
    {
        Server!.ContactInSession = true;
        ContactJoined?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Handles an MSG command.
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    private async Task HandleMSG(byte[] response)
    {
        if (Server == null)
            throw new NullReferenceException("Server is null");
        
        Server.ResetTimeout();
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
                MessageReceived?.Invoke(this, new MessageEventArgs
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
                    MessageReceived?.Invoke(this, new MessageEventArgs
                    {
                        Email = parameters[1],
                        Message = new TextPlain
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
            TextPlain message = new TextPlain
            {
                Text = payloadString[payloadString.IndexOf(payloadParameters[2])..]
                    .Replace(payloadParameters[2] + "\r\n\r\n", "")
                    .Replace("\r\n", "\n")
            };

            message.SetFormatting(payloadParameters[2]);
            MessageReceived?.Invoke(this, new MessageEventArgs
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
    /// Creates a new transfer class, if none has been created, to handle sending display pictures.
    /// </summary>
    /// <param name="payload">P2P invite payload.</param>
    /// <returns></returns>
    private async Task HandleP2PInvite(byte[] payload)
    {
        DisplayPictureTransfer ??= new DisplayPictureTransfer()
        {
            Server = Server
        };
        await DisplayPictureTransfer.SendDisplayPicture(payload);
    }
}