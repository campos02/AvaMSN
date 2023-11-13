using System.Security.Cryptography;
using System.Text;

namespace AvaMSN.MSNP.Messages;

public class SendDisplayPicture
{
    public string MimeVersion { get; set; } = "1.0";
    public string ContentType { get; set; } = "application/x-msnmsgrp2p";

    public string To { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public string Branch { get; set; } = string.Empty;
    public int CSeq { get; set; }
    public string CallID { get; set; } = string.Empty;

    public uint Identifier { get; set; }
    public uint SessionID { get; set; }

    public byte[]? Data { get; set; }
    public int DataOffset { get; set; }

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
        byte[] inviteMessage = headers.Concat(body).ToArray();

        Identifier++;
        LastHeader = new BinaryHeader()
        {
            Identifier = Identifier,
            DataSize = (ulong)inviteMessage.Length + 1,
            Length = (uint)inviteMessage.Length + 1,
            AckID = BitConverter.ToUInt32(RandomNumberGenerator.GetBytes(sizeof(uint)))
        };

        byte[] footer = { 00, 00, 00, 00, 00 };
        byte[] content = LastHeader.GetBytes().Concat(inviteMessage).Concat(footer).ToArray();

        return Encoding.UTF8.GetBytes(PayloadHeaders()).Concat(content).ToArray();
    }

    public byte[] DataPreparationPayload()
    {
        byte[] message = { 00, 00, 00, 00 };

        Identifier++;
        LastHeader = new BinaryHeader()
        {
            SessionID = SessionID,
            Identifier = Identifier,
            DataSize = (ulong)message.Length,
            Length = (uint)message.Length,
            AckID = BitConverter.ToUInt32(RandomNumberGenerator.GetBytes(sizeof(uint)))
        };

        byte[] footer = { 00, 00, 00, 01 };
        byte[] content = LastHeader.GetBytes().Concat(message).Concat(footer).ToArray();

        return Encoding.UTF8.GetBytes(PayloadHeaders()).Concat(content).ToArray();
    }

    public byte[] DataPayload()
    {
        byte[] message;

        if (Data!.Length - DataOffset < 1200)
        {
            message = new byte[Data.Length - DataOffset];
        }
        else
        {
            message = new byte[1200];
        }

        Buffer.BlockCopy(Data!, DataOffset, message, 0, message.Length);

        LastHeader = new BinaryHeader()
        {
            SessionID = SessionID,
            Identifier = Identifier + 1,
            DataOffset = (ulong)DataOffset,
            DataSize = (ulong)Data!.Length,
            Length = (uint)message.Length,
            Flag = (uint)Flags.DisplayPictureData,
            AckID = BitConverter.ToUInt32(RandomNumberGenerator.GetBytes(sizeof(uint)))
        };

        byte[] footer = { 00, 00, 00, 01 };
        byte[] content = LastHeader.GetBytes().Concat(message).Concat(footer).ToArray();
        DataOffset += message.Length;

        return Encoding.UTF8.GetBytes(PayloadHeaders()).Concat(content).ToArray();
    }

    private string PayloadHeaders()
    {
        return $"MIME-Version: {MimeVersion}\r\n" +
               $"Content-Type: {ContentType}\r\n" +
               $"P2P-Dest: {To}\r\n\r\n";
    }
}
