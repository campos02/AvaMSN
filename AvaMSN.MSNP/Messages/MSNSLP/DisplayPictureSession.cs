using System.Text;

namespace AvaMSN.MSNP.Messages.MSNSLP;

/// <summary>
/// Base class containing session parameters and other common payload functions for a display picture session.
/// </summary>
public class DisplayPictureSession
{
    // Session parameters
    public string MimeVersion { get; set; } = "1.0";
    public string ContentType { get; set; } = "application/x-msnmsgrp2p";

    public string To { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public string Branch { get; set; } = string.Empty;
    public int CSeq { get; set; }
    public string CallID { get; set; } = string.Empty;

    public uint Identifier { get; set; }
    public uint SessionID { get; set; }

    /// <summary>
    /// Last binary header used in a message.
    /// </summary>
    public BinaryHeader? LastHeader { get; set; }

    /// <summary>
    /// Returns the payload for an acknowledgement to the specified binary header.
    /// </summary>
    /// <param name="binaryHeader">Binary header of the message being acknowledged.</param>
    /// <returns>Binary payload.</returns>
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

        // Combine to produce full MSNSLP content
        byte[] content = LastHeader.GetBytes().Concat(footer).ToArray();

        // Combine with headers to get full MSG payload
        return Encoding.UTF8.GetBytes(PayloadHeaders()).Concat(content).ToArray();
    }

    /// <summary>
    /// Returns MSG payload headers used in the session.
    /// </summary>
    /// <returns>Message payload headers.</returns>
    protected string PayloadHeaders()
    {
        return $"MIME-Version: {MimeVersion}\r\n" +
               $"Content-Type: {ContentType}\r\n" +
               $"P2P-Dest: {To}\r\n\r\n";
    }
}
