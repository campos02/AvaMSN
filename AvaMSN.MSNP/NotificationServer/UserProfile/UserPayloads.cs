using System.Xml;
using System.Xml.Serialization;
using AvaMSN.MSNP.XML.SerializableClasses;

namespace AvaMSN.MSNP.NotificationServer.UserProfile;

/// <summary>
/// Contains methods for building payloads to user-related commands.
/// </summary>
internal static class UserPayloads
{
    /// <summary>
    /// Returns the payload used by the UUX command, which sets a user's personal message.
    /// </summary>
    /// <param name="personalMessage">User personal message.</param>
    /// <returns>UUX command payload.</returns>
    public static string UUXPayload(string personalMessage)
    {
        Data data = new Data
        {
            PSM = personalMessage
        };

        var settings = new XmlWriterSettings
        {
            OmitXmlDeclaration = true
        };

        // Remove namespaces
        var namespaces = new XmlSerializerNamespaces([XmlQualifiedName.Empty]);

        XmlSerializer mlSerializer = new(typeof(Data));
        using StringWriter stream = new StringWriter();
        using XmlWriter writer = XmlWriter.Create(stream, settings);
        
        // Serialize with options
        mlSerializer.Serialize(writer, data, namespaces);
        return stream.ToString();
    }
}