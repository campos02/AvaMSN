using System.Security.Cryptography;
using System.Text;

namespace AvaMSN.MSNP.Models.Messages.MSNSLP;

/// <summary>
/// Stores session parameters and contains functions that return message payloads for receiving a display picture.
/// </summary>
internal class ReceiveDisplayPicture : DisplayPictureSession
{
    /// <summary>
    /// Returns an INVITE payload, used to start a session.
    /// </summary>
    /// <param name="MSNObject">Display picture object used as context.</param>
    /// <returns>Binary payload.</returns>
    public byte[] InvitePayload(string MSNObject)
    {
        Identifier = BitConverter.ToUInt32(RandomNumberGenerator.GetBytes(sizeof(uint)));
        SessionID = (uint)RandomNumberGenerator.GetInt32(10000000);
        Branch = $"{{{Guid.NewGuid().ToString().ToUpper()}}}";
        CallID = $"{{{Guid.NewGuid().ToString().ToUpper()}}}";
        string bodyText = "EUF-GUID: {A4268EEC-FEC5-49E5-95C3-F126696BDBF6}\r\n" +
                          $"SessionID: {SessionID}\r\n" +
                          "SChannelState: 0\r\n" +
                          "AppID: 1\r\n" +
                          $"Context: {Convert.ToBase64String(Encoding.UTF8.GetBytes(MSNObject))}\r\n\r\n";

        byte[] body = Encoding.UTF8.GetBytes(bodyText);
        string headersText = $"INVITE MSNMSGR:{To} MSNSLP/1.0\r\n" +
                             $"To: <msnmsgr:{To}>\r\n" +
                             $"From: <msnmsgr:{From}>\r\n" +
                             $"Via: MSNSLP/1.0/TLP ;branch={Branch}\r\n" +
                             $"CSeq: {CSeq}\r\n" +
                             $"Call-ID: {CallID}\r\n" +
                             "Max-Forwards: 0\r\n" +
                             "Content-Type: application/x-msnmsgr-sessionreqbody\r\n" +
                             $"Content-Length: {body.Length + 1}\r\n\r\n";

        byte[] headers = Encoding.UTF8.GetBytes(headersText);
        byte[] message = headers.Concat(body).ToArray();

        BinaryHeader binaryHeader = new BinaryHeader
        {
            Identifier = Identifier - 3,
            DataSize = (ulong)message.Length + 1,
            Length = (uint)message.Length + 1,
            AckID = BitConverter.ToUInt32(RandomNumberGenerator.GetBytes(sizeof(uint)))
        };

        byte[] footer = [00, 00, 00, 00, 00];

        // Combine to produce full MSNSLP content
        byte[] content = binaryHeader.GetBytes().Concat(message).Concat(footer).ToArray();

        // Combine with headers to get full MSG payload
        return Encoding.UTF8.GetBytes(PayloadHeaders()).Concat(content).ToArray();
    }

    /// <summary>
    /// Returns a BYE payload, used to end a session.
    /// </summary>
    /// <returns>Binary payload.</returns>
    public byte[] ByePayload()
    {
        string headersText = $"BYE MSNMSGR:{To} MSNSLP/1.0\r\n" +
                             $"To: <msnmsgr:{To}>\r\n" +
                             $"From: <msnmsgr:{From}>\r\n" +
                             $"Via: MSNSLP/1.0/TLP ;branch={Branch}\r\n" +
                             $"CSeq: {CSeq}\r\n" +
                             $"Call-ID: {CallID}\r\n" +
                             "Max-Forwards: 0\r\n" +
                             "Content-Type: application/x-msnmsgr-sessionreqbody\r\n" +
                             "Content-Length: 3\r\n\r\n";

        byte[] headers = Encoding.UTF8.GetBytes(headersText);
        BinaryHeader binaryHeader = new BinaryHeader
        {
            Identifier = Identifier - 2,
            DataSize = (ulong)headers.Length + 1,
            Length = (uint)headers.Length + 1,
            AckID = BitConverter.ToUInt32(RandomNumberGenerator.GetBytes(sizeof(uint)))
        };

        byte[] footer = [00, 00, 00, 00, 00];

        // Combine to produce full MSNSLP content
        byte[] content = binaryHeader.GetBytes().Concat(headers).Concat(footer).ToArray();

        // Combine with headers to get full MSG payload
        return Encoding.UTF8.GetBytes(PayloadHeaders()).Concat(content).ToArray();
    }
}
