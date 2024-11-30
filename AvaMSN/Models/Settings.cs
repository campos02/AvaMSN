namespace AvaMSN.Models;

/// <summary>
/// Represents client settings and their defaults.
/// </summary>
public class Settings
{
    public string MainServer { get; set; } = "master.ctsrv.hiden.cc";
    public string RstUrl { get; set; } = "https://login.ugnet.hiden.cc/RST.srf";
    public string SharingServiceUrl { get; set; } = "https://abch.livesvcs.ctsrv.hiden.cc/abservice/SharingService.asmx";
    public string AddressBookUrl { get; set; } = "https://abch.livesvcs.ctsrv.hiden.cc/abservice/abservice.asmx";
    public bool SaveMessagingHistory { get; set; } = true;
    public bool MinimizeToTray { get; set; } = true;
    public bool SaveConnectionLog { get; set; }
}
