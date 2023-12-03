namespace AvaMSN.MSNP.Messages;

/// <summary>
/// Represents a plain text message.
/// </summary>
public class TextPlain
{
    // Headers
    public string MimeVersion { get; set; } = "1.0";
    public string ContentType { get; set; } = "text/plain";
    public string Charset { get; set; } = "UTF-8";

    // Message format
    public string FontName { get; set; } = string.Empty;
    public string EF { get; set; } = string.Empty;
    public int CO { get; set; }
    public int CS { get; set; }
    public int FontSize { get; set; } = 22;

    // Contains message text
    public string Content { get ; set; } = string.Empty;

    /// <summary>
    /// Returns a payload for use in the MSG command.
    /// </summary>
    /// <returns>Message payload.</returns>
    public string CreatePayload()
    {
        return $"MIME-Version: {MimeVersion}\r\n" +
               $"Content-Type: {ContentType}; charset={Charset}\r\n" +
               $"X-MMS-IM-Format: FN={Uri.EscapeDataString(FontName)}; EF={EF}; CO={CO}; CS={CS}; PF={FontSize}\r\n\r\n" +
               Content;
    }
}
