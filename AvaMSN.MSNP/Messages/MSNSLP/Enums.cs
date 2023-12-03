namespace AvaMSN.MSNP.Messages.MSNSLP;

/// <summary>
/// MSNSLP header flags.
/// </summary>
public enum Flags
{
    OutOfOrder = 0x1,
    Acknowledgement = 0x2,
    Data = 0x20
}
