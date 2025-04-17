namespace AvaMSN.Models;

/// <summary>
/// Represents client settings and their defaults.
/// </summary>
public class Settings
{
    public string MainServer { get; set; } = "ms.msgrsvcs.ctsrv.xyz";
    public string RstUrl { get; set; } = "https://ctas.login.ugnet.xyz/RST.srf";
    public string SharingServiceUrl { get; set; } = "https://ctsvcs.addressbook.ugnet.xyz/abservice/SharingService.asmx";
    public string AddressBookUrl { get; set; } = "https://ctsvcs.addressbook.ugnet.xyz/abservice/abservice.asmx";
    public bool SaveMessagingHistory { get; set; } = true;
    public bool MinimizeToTray { get; set; } = true;
    public bool SaveConnectionLog { get; set; }
}
