using System.Xml;
using System.Xml.Serialization;
using AvaMSN.MSNP.Models;
using AvaMSN.MSNP.XML.SerializableClasses.InitialListPayload;

namespace AvaMSN.MSNP.NotificationServer.Contacts;

/// <summary>
/// Contains methods for building payloads to contact-related commands.
/// </summary>
internal static class ContactPayloads
{
    /// <summary>
    /// Returns the payload for the first ADL command ran by the client, which includes all contacts.
    /// </summary>
    /// <param name="contacts">Contact list.</param>
    /// <returns>ADL payload.</returns>
    public static string InitialListPayload(List<Contact> contacts)
    {
        // Initialize while setting the necessary attribute
        ml ml = new ml
        {
            l = 1
        };

        List<mlD> domains = new List<mlD>();

        // Add contacts with the necessary attributes
        foreach (Contact contact in contacts)
        {
            domains.Add(new mlD
            {
                n = contact.Email.Split("@")[1],
                c = new mlDC
                {
                    n = contact.Email.Split("@")[0],
                    l = (byte)contact.InLists.ListsNumber(),
                    t = (byte)(contact.Type == "Passport" ? 1 : 4)
                }
            });
        }

        ml.d = domains.ToArray();

        var settings = new XmlWriterSettings
        {
            OmitXmlDeclaration = true
        };

        // Remove namespaces
        var namespaces = new XmlSerializerNamespaces([XmlQualifiedName.Empty]);
        XmlSerializer mlSerializer = new(typeof(ml));
        
        // Serialize with options
        using StringWriter stream = new StringWriter();
        using XmlWriter writer = XmlWriter.Create(stream, settings);
        mlSerializer.Serialize(writer, ml, namespaces);
        return stream.ToString();
    }

    /// <summary>
    /// Returns the payload for an ADL (other than the initial one) or RML command.
    /// </summary>
    /// <param name="contact">Contact to add or remove.</param>
    /// <param name="lists">Lists to add or remove the contact from.</param>
    /// <returns>List command payload.</returns>
    public static string ListPayload(Contact contact, Lists lists)
    {
        XML.SerializableClasses.ListPayload.ml ml = new XML.SerializableClasses.ListPayload.ml();

        // Add contact with the necessary attributes
        List<XML.SerializableClasses.ListPayload.mlD> domains =
        [
            new XML.SerializableClasses.ListPayload.mlD
            {
                n = contact.Email.Split("@")[1],
                c = new XML.SerializableClasses.ListPayload.mlDC
                {
                    n = contact.Email.Split("@")[0],
                    l = (byte)lists.ListsNumber(),
                    t = (byte)(contact.Type == "Passport" ? 1 : 4)
                }
            }
        ];

        ml.d = domains.ToArray();
        
        var settings = new XmlWriterSettings
        {
            OmitXmlDeclaration = true
        };
        
        // Remove namespaces
        var namespaces = new XmlSerializerNamespaces([XmlQualifiedName.Empty]);
        
        XmlSerializer mlSerializer = new(typeof(XML.SerializableClasses.ListPayload.ml));
        using StringWriter stream = new StringWriter();
        using XmlWriter writer = XmlWriter.Create(stream, settings);
        
        // Serialize with options
        mlSerializer.Serialize(writer, ml, namespaces);
        return stream.ToString();
    }

    /// <summary>
    /// Returns the payload for an FQY command.
    /// </summary>
    /// <param name="contact"></param>
    /// <returns></returns>
    public static string ContactPayload(Contact contact)
    {
        XML.SerializableClasses.ContactPayload.ml ml = new XML.SerializableClasses.ContactPayload.ml();

        // Add contact with the necessary attributes
        List<XML.SerializableClasses.ContactPayload.mlD> domains =
        [
            new XML.SerializableClasses.ContactPayload.mlD
            {
                n = contact.Email.Split("@")[1],
                c = new XML.SerializableClasses.ContactPayload.mlDC
                {
                    n = contact.Email.Split("@")[0]
                }
            }
        ];

        ml.d = domains.ToArray();

        var settings = new XmlWriterSettings
        {
            OmitXmlDeclaration = true
        };

        // Remove namespaces
        var namespaces = new XmlSerializerNamespaces([XmlQualifiedName.Empty]);

        XmlSerializer mlSerializer = new(typeof(XML.SerializableClasses.ContactPayload.ml));
        using StringWriter stream = new StringWriter();
        using XmlWriter writer = XmlWriter.Create(stream, settings);
        
        // Serialize with options
        mlSerializer.Serialize(writer, ml, namespaces);
        return stream.ToString();
    }
}