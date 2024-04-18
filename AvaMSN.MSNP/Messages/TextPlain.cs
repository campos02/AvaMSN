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
    public int PitchFamily { get; set; } = 22;

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
        string effect = parameters[1];

        if (effect.Contains("B"))
            Bold = true;
        if (effect.Contains("I"))
            Italic = true;
        if (effect.Contains("S"))
            Strikethrough = true;
        if (effect.Contains("U"))
            Underline = true;

        string color = parameters[2].Replace(" CO=", "");
        // Add trailing zeroes back
        while (color.Length < 6)
            color += "0";

        // Reverse from BGR to RGB and add hex sign
        Color = $"#{color.Substring(4,2)}{color.Substring(2,2)}{color.Substring(0,2)}";
    }

    /// <summary>
    /// Returns a payload for use in the MSG command.
    /// </summary>
    /// <returns>Message payload.</returns>
    public string CreatePayload()
    {
        string effect = string.Empty;

        if (Bold)
            effect += "B";
        if (Italic)
            effect += "I";
        if (Strikethrough)
            effect += "S";
        if (Underline)
            effect += "U";

        string color = Color.Replace("#", "");
        if (color.Length >= 6)
        {
            // Reverse from RGB to BGR
            color = $"{color.Substring(4,2)}{color.Substring(2,2)}{color.Substring(0,2)}";
        }

        return $"MIME-Version: {MimeVersion}\r\n" +
               $"Content-Type: {ContentType}; charset={Charset}\r\n" +
               $"X-MMS-IM-Format: FN={Uri.EscapeDataString(FontName)}; EF={effect}; CO={color}; CS=1; PF={PitchFamily}\r\n\r\n" +
               Content;
    }
}
