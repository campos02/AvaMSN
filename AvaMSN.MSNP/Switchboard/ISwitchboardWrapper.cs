namespace AvaMSN.MSNP.Switchboard;

/// <summary>
/// Interface for classes that work around a Switchboard object.
/// </summary>
public interface ISwitchboardWrapper
{
    public Switchboard? Server { get; set; }
}