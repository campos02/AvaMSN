using System.Security.Cryptography;
using System.Text;

namespace AvaMSN.MSNP.Messages;

public class ReceiveDisplayPicture
{
    public string MimeVersion { get; set; } = "1.0";
    public string ContentType { get; set; } = "application/x-msnmsgrp2p";

    public string To { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public string Branch { get; set; } = string.Empty;
    public int CSeq { get; set; }
    public string CallID { get; set; } = string.Empty;

    public uint Identifier { get; set; }
    public int SessionID { get; set; }

    public BinaryHeader? LastHeader { get; set; }

    public byte[] AcknowledgePayload(BinaryHeader binaryHeader)
    {
        LastHeader = new BinaryHeader()
        {
            Identifier = Identifier,
            DataSize = binaryHeader.DataSize,
            Flag = (uint)Flags.Acknowledgement,
            AckID = binaryHeader.Identifier,
            AckUniqueID = binaryHeader.AckUniqueID,
            AckDataSize = binaryHeader.DataSize
        };

        byte[] footer = { 00, 00, 00, 00 };
        byte[] content = LastHeader.GetBytes().Concat(footer).ToArray();

        return Encoding.UTF8.GetBytes(PayloadHeaders()).Concat(content).ToArray();
    }

    public byte[] InvitePayload(string MSNObject)
    {
        Identifier = BitConverter.ToUInt32(RandomNumberGenerator.GetBytes(sizeof(uint)));
        SessionID = RandomNumberGenerator.GetInt32(10000000);
        Branch = $"{{{Guid.NewGuid().ToString().ToUpper()}}}";
        CallID = $"{{{Guid.NewGuid().ToString().ToUpper()}}}";

        string bodyText = "EUF-GUID: {A4268EEC-FEC5-49E5-95C3-F126696BDBF6}\r\n" +
                          $"SessionID: {SessionID}\r\n" +
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
        byte[] inviteMessage = headers.Concat(body).ToArray();

        BinaryHeader binaryHeader = new BinaryHeader()
        {
            Identifier = Identifier - 3,
            DataSize = (ulong)inviteMessage.Length + 1,
            Length = (uint)inviteMessage.Length + 1,
            AckID = BitConverter.ToUInt32(RandomNumberGenerator.GetBytes(sizeof(uint)))
        };

        byte[] footer = { 00, 00, 00, 00, 00 };
        byte[] content = binaryHeader.GetBytes().Concat(inviteMessage).Concat(footer).ToArray();

        return Encoding.UTF8.GetBytes(PayloadHeaders()).Concat(content).ToArray();
    }

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

        BinaryHeader binaryHeader = new BinaryHeader()
        {
            Identifier = Identifier - 2,
            DataSize = (ulong)headers.Length + 1,
            Length = (uint)headers.Length + 1,
            AckID = BitConverter.ToUInt32(RandomNumberGenerator.GetBytes(sizeof(uint)))
        };

        byte[] footer = { 00, 00, 00, 00, 00 };
        byte[] content = binaryHeader.GetBytes().Concat(headers).Concat(footer).ToArray();

        return Encoding.UTF8.GetBytes(PayloadHeaders()).Concat(content).ToArray();
    }

    private string PayloadHeaders()
    {
        return $"MIME-Version: {MimeVersion}\r\n" +
               $"Content-Type: {ContentType}\r\n" +
               $"P2P-Dest: {To}\r\n\r\n";
    }
}
