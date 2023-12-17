namespace AvaMSN.MSNP.PresenceStatus;

/// <summary>
/// Contains all presence statuses used in MSNP.
/// </summary>
public struct PresenceStatus
{
    public const string Online = "NLN";
    public const string Busy = "BSY";
    public const string Idle = "IDL";
    public const string BeRightBack = "BRB";
    public const string Away = "AWY";
    public const string OnThePhone = "PHN";
    public const string OutToLunch = "LUN";
    public const string Invisible = "HDN";
    public const string Offline = "";

    public static string GetFullName(string status) => status switch
    {
        Online => "Online",
        Busy => "Busy",
        Idle => "Idle",
        BeRightBack => "Be right back",
        Away => "Away",
        OnThePhone => "On the phone",
        OutToLunch => "Out to lunch",
        Invisible => "Invisible",
        _ => "Offline"
    };

    public static string GetShortName(string status) => status switch
    {
        "Online" => Online,
        "Busy" => Busy,
        "Idle" => Idle,
        "Be right back" => BeRightBack,
        "Away" => Away,
        "On the phone" => OnThePhone,
        "Out to lunch" => OutToLunch,
        "Invisible" => Invisible,
        _ => Offline
    };
}
