namespace AvaMSN.MSNP.Models;

/// <summary>
/// List numbers of MSNP.
/// </summary>
public enum ListNumbers
{
    Forward = 1,
    Allow = 2,
    Block = 4,
    Reverse = 8,
    Pending = 16
}

/// <summary>
/// MSNSLP header flags.
/// </summary>
public enum Flags
{
    OutOfOrder = 0x1,
    Acknowledgement = 0x2,
    Data = 0x20
}