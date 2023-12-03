namespace AvaMSN.MSNP.Messages.MSNSLP;

/// <summary>
/// Represents a P2Pv1 binary header.
/// </summary>
public class BinaryHeader
{
    /// <summary>
    /// Binary header length.
    /// </summary>
    public const int HeaderSize = 48;

    // Header parameters
    public uint SessionID { get; set; }
    public uint Identifier { get; set; }
    public ulong DataOffset { get; set; }
    public ulong DataSize { get; set; }
    public uint Length { get; set; }
    public uint Flag { get; set; }
    public uint AckID { get; set; }
    public uint AckUniqueID { get; set; }
    public ulong AckDataSize { get; set; }

    public BinaryHeader() { }

    /// <summary>
    /// Initialize using data from a byte header.
    /// </summary>
    /// <param name="binaryHeader">Header in byte array format.</param>
    public BinaryHeader(byte[] binaryHeader)
    {
        using (MemoryStream headerStream = new MemoryStream(binaryHeader))
        {
            using (BinaryReader reader = new BinaryReader(headerStream))
            {
                SessionID = reader.ReadUInt32();
                Identifier = reader.ReadUInt32();
                DataOffset = reader.ReadUInt64();
                DataSize = reader.ReadUInt64();
                Length = reader.ReadUInt32();
                Flag = reader.ReadUInt32();
                AckID = reader.ReadUInt32();
                AckUniqueID = reader.ReadUInt32();
                AckDataSize = reader.ReadUInt64();
            }
        }
    }

    /// <summary>
    /// Convert all parameters into a byte array.
    /// </summary>
    /// <returns>Converted byte array.</returns>
    public byte[] GetBytes()
    {
        using (MemoryStream headerStream = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(headerStream))
            {
                writer.Write(SessionID);
                writer.Write(Identifier);
                writer.Write(DataOffset);
                writer.Write(DataSize);
                writer.Write(Length);
                writer.Write(Flag);
                writer.Write(AckID);
                writer.Write(AckUniqueID);
                writer.Write(AckDataSize);
            }

            return headerStream.ToArray();
        }
    }
}
