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

    // Message formatting
    public string FontName { get; set; } = string.Empty;
    
    public bool Bold { get; set; }
    public bool Italic { get; set; }
    public bool Strikethrough { get; set; }
    public bool Underline { get; set; }
    public string Decorations { get; set; } = string.Empty;

    public string Color { get; set; } = "0";
    public int FontSize { get; set; } = 22;

    // Contains message text
    public string Content { get ; set; } = string.Empty;

    /// <summary>
    /// Parses X-MMS-IM-Format header and sets bold, italic, strikethrough and underline options.
    /// </summary>
    /// <param name="formatParameter">X-MMS-IM-Format parameter.</param>
    public void SetFormatting(string formatParameter)
    {
        formatParameter = formatParameter.Replace("X-MMS-IM-Format: ", "");
        string[] parameters = formatParameter.Split(";");
        string ef = parameters[1];

        if (ef.Contains("B"))
            Bold = true;
        if (ef.Contains("I"))
            Italic = true;
        if (ef.Contains("S"))
            Strikethrough = true;
        if (ef.Contains("U"))
            Underline = true;
    }

    /// <summary>
    /// Returns a payload for use in the MSG command.
    /// </summary>
    /// <returns>Message payload.</returns>
    public string CreatePayload()
    {
        string ef = string.Empty;

        if (Bold)
            ef += "B";
        if (Italic)
            ef += "I";
        if (Strikethrough)
            ef += "S";
        if (Underline)
            ef += "U";

        return $"MIME-Version: {MimeVersion}\r\n" +
               $"Content-Type: {ContentType}; charset={Charset}\r\n" +
               $"X-MMS-IM-Format: FN={Uri.EscapeDataString(FontName)}; EF={ef}; CO={Color}; CS=0; PF={FontSize}\r\n\r\n" +
               Content;
    }
}
