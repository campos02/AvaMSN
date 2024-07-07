using System.Security.Cryptography;
using System.Text;

namespace AvaMSN.MSNP.Models.Messages.MSNSLP;

/// <summary>
/// Stores session parameters and contains functions that return message payloads for sending a display picture.
/// </summary>
internal class SendDisplayPicture : DisplayPictureSession
{
    public byte[]? Data { get; set; }
    public int DataOffset { get; set; }

    /// <summary>
    /// Returns a 200 OK payload, used to accept a session.
    /// </summary>
    /// <returns>Binary payload.</returns>
    public byte[] OkPayload()
    {
        string bodyText = $"SessionID: {SessionID}\r\n\r\n";
        byte[] body = Encoding.UTF8.GetBytes(bodyText);

        CSeq++;
        string headersText = "MSNSLP/1.0 200 OK\r\n" +
                             $"To: <msnmsgr:{To}>\r\n" +
                             $"From: <msnmsgr:{From}>\r\n" +
                             $"Via: MSNSLP/1.0/TLP ;branch={Branch}\r\n" +
                             $"CSeq: {CSeq + 1}\r\n" +
                             $"Call-ID: {CallID}\r\n" +
                             "Max-Forwards: 0\r\n" +
                             "Content-Type: application/x-msnmsgr-sessionreqbody\r\n" +
                             $"Content-Length: {body.Length + 1}\r\n\r\n";

        byte[] headers = Encoding.UTF8.GetBytes(headersText);
        byte[] message = headers.Concat(body).ToArray();

        Identifier++;
        LastHeader = new BinaryHeader
        {
            Identifier = Identifier,
            DataSize = (ulong)message.Length + 1,
            Length = (uint)message.Length + 1,
            AckID = BitConverter.ToUInt32(RandomNumberGenerator.GetBytes(sizeof(uint)))
        };

        byte[] footer = [00, 00, 00, 00, 00];

        // Combine to produce full MSNSLP content
        byte[] content = LastHeader.GetBytes().Concat(message).Concat(footer).ToArray();

        // Combine with headers to get full MSG payload
        return Encoding.UTF8.GetBytes(PayloadHeaders()).Concat(content).ToArray();
    }

    /// <summary>
    /// Returns a data preparation payload, which tells the receiving client to prepare to receive picture data.
    /// </summary>
    /// <returns>Binary payload.</returns>
    public byte[] DataPreparationPayload()
    {
        byte[] message = [00, 00, 00, 00];

        Identifier++;
        LastHeader = new BinaryHeader
        {
            SessionID = SessionID,
            Identifier = Identifier,
            DataSize = (ulong)message.Length,
            Length = (uint)message.Length,
            AckID = BitConverter.ToUInt32(RandomNumberGenerator.GetBytes(sizeof(uint)))
        };

        byte[] footer = [00, 00, 00, 01];

        // Combine to produce full MSNSLP content
        byte[] content = LastHeader.GetBytes().Concat(message).Concat(footer).ToArray();

        // Combine with headers to get full MSG payload
        return Encoding.UTF8.GetBytes(PayloadHeaders()).Concat(content).ToArray();
    }

    /// <summary>
    /// Returns a data payload, which contains a chunk of picture data.
    /// </summary>
    /// <returns>Binary payload.</returns>
    public byte[] DataPayload()
    {
        byte[] message;

        // Set chunk buffer
        if (Data!.Length - DataOffset < 1200)
        {
            message = new byte[Data.Length - DataOffset];
        }
        else
        {
            message = new byte[1200];
        }

        // Copy chunk to buffer
        Buffer.BlockCopy(Data!, DataOffset, message, 0, message.Length);

        LastHeader = new BinaryHeader
        {
            SessionID = SessionID,
            Identifier = Identifier + 1,
            DataOffset = (ulong)DataOffset,
            DataSize = (ulong)Data!.Length,
            Length = (uint)message.Length,
            Flag = (uint)Flags.Data,
            AckID = BitConverter.ToUInt32(RandomNumberGenerator.GetBytes(sizeof(uint)))
        };

        byte[] footer = [00, 00, 00, 01];

        // Combine to produce full MSNSLP content
        byte[] content = LastHeader.GetBytes().Concat(message).Concat(footer).ToArray();
        DataOffset += message.Length;

        // Combine with headers to get full MSG payload
        return Encoding.UTF8.GetBytes(PayloadHeaders()).Concat(content).ToArray();
    }
}
