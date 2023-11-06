using System.Text;
using System.Xml;
using System.Xml.Serialization;
using AvaMSN.MSNP.Exceptions;
using AvaMSN.MSNP.SOAP;
using AvaMSN.MSNP.SOAP.RequestObjects;
using AvaMSN.MSNP.XML.SerializableClasses;

namespace AvaMSN.MSNP;

public class ContactList
{
    public string TicketToken { get; set; } = string.Empty;
    public string SharingService { get; private set; } = string.Empty;
    public string ABService { get; private set; } = string.Empty;

    public List<Contact> Contacts { get; set; } = new List<Contact>();
    public Profile Profile { get; set; } = new Profile();

    public ContactList(string host)
    {
        SharingService = $"https://{host}/abservice/SharingService.asmx";
        ABService = $"https://{host}/abservice/abservice.asmx";
    }

    public async Task FindMembership()
    {
        XmlSerializer requestSerializer = new(typeof(SOAP.SerializableClasses.FindMembership.Envelope));

        var envelope = RequestObjects.FindMembership();
        envelope.Header.ABAuthHeader.TicketToken = TicketToken;

        using MemoryStream memory = new();
        requestSerializer.Serialize(memory, envelope);

        string soapXML = Encoding.UTF8.GetString(memory.ToArray());
        string response = await Requests.SoapRequest(soapXML, SharingService, "http://www.msn.com/webservices/AddressBook/FindMembership");

        XmlSerializer responseSerializer = new(typeof(SOAP.SerializableClasses.FindMembershipResponse.Envelope));

        using (StringReader reader = new StringReader(response))
        {
            var responseEnvelope = (SOAP.SerializableClasses.FindMembershipResponse.Envelope?)responseSerializer.Deserialize(reader);

            foreach (var membership in responseEnvelope!.Body.FindMembershipResponse.FindMembershipResult.Services.Service.Memberships)
            {
                foreach(var member in membership.Members)
                {
                    if (member.Type != "Passport")
                        continue;

                    string contactID = member.MembershipId.Replace($"{membership.MemberRole}/", "");

                    Contact contact = Contacts.FirstOrDefault(c => c.ContactID == contactID) ?? new Contact();

                    contact.InLists.SetMembershipLists(membership.MemberRole);

                    contact.Email = member.PassportName;
                    contact.ContactID = contactID;

                    contact.Type = member.Type;
                    contact.State = member.State;

                    contact.MembershipLastChanged = member.LastChanged;
                    contact.JoinedDate = member.JoinedDate;
                    contact.ExpirationDate = member.ExpirationDate;

                    contact.IsEmailHidden = member.IsPassportNameHidden;

                    if (!Contacts.Contains(contact))
                        Contacts.Add(contact);
                }
            }
        }
    }

    public async Task ABFindAll()
    {
        XmlSerializer requestSerializer = new(typeof(SOAP.SerializableClasses.ABFindAll.Envelope));

        var envelope = RequestObjects.ABFindAll();
        envelope.Header.ABAuthHeader.TicketToken = TicketToken;

        using MemoryStream memory = new();
        requestSerializer.Serialize(memory, envelope);

        string soapXML = Encoding.UTF8.GetString(memory.ToArray());
        string response = await Requests.SoapRequest(soapXML, ABService, "http://www.msn.com/webservices/AddressBook/ABFindAll");

        XmlSerializer responseSerializer = new(typeof(SOAP.SerializableClasses.ABFindAllResponse.Envelope));

        using (StringReader reader = new StringReader(response))
        {
            var responseEnvelope = (SOAP.SerializableClasses.ABFindAllResponse.Envelope?)responseSerializer.Deserialize(reader);

            foreach (var addressBookContact in responseEnvelope!.Body.ABFindAllResponse.ABFindAllResult.contacts)
            {
                var contactInfo = addressBookContact.contactInfo;

                if (contactInfo.contactType == "Me")
                {
                    UpdateProfile(addressBookContact);
                    continue;
                }

                Contact contact = Contacts.FirstOrDefault(c => c.ContactID == addressBookContact.contactId) ?? new Contact();

                contact.InLists.Forward = true;

                contact.DisplayName = contactInfo.displayName;
                contact.Email = contactInfo.passportName;

                contact.ContactID = addressBookContact.contactId;

                contact.IsFavorite = contactInfo.isFavorite;
                contact.HasSpace = contactInfo.hasSpace;
                contact.IsPrivate = contactInfo.IsPrivate;
                contact.IsEmailHidden = contactInfo.IsPassportNameHidden;

                contact.Gender = contactInfo.Gender;
                contact.TimeZone = contactInfo.TimeZone;

                contact.Birthdate = contactInfo.birthdate;
                contact.ABLastChanged = addressBookContact.lastChange;

                if (!Contacts.Contains(contact))
                    Contacts.Add(contact);
            }
        }
    }

    public async Task ChangeDisplayName()
    {
        XmlSerializer requestSerializer = new(typeof(SOAP.SerializableClasses.ABContactUpdate.Envelope));

        var envelope = RequestObjects.AbContactUpdate();
        
        envelope.Header.ABAuthHeader.TicketToken = TicketToken;
        envelope.Body.ABContactUpdate.contacts.Contact.contactInfo.contactType = "Me";
        envelope.Body.ABContactUpdate.contacts.Contact.contactInfo.displayName = Profile.DisplayName;

        using MemoryStream memory = new();
        requestSerializer.Serialize(memory, envelope);

        string soapXML = Encoding.UTF8.GetString(memory.ToArray());
        string response = await Requests.SoapRequest(soapXML, ABService, "http://www.msn.com/webservices/AddressBook/ABContactUpdate");

        XmlSerializer responseSerializer = new(typeof(SOAP.SerializableClasses.ABContactUpdateResponse.Envelope));

        using (StringReader reader = new StringReader(response))
        {
            var responseEnvelope = (SOAP.SerializableClasses.ABContactUpdateResponse.Envelope?)responseSerializer.Deserialize(reader)
                ?? throw new ContactException("Contact update failure");
        }
    }

    public async Task AddMember(string memberRole, string email, string scenario = "BlockUnblock")
    {
        XmlSerializer requestSerializer = new(typeof(SOAP.SerializableClasses.AddMember.Envelope));

        var envelope = RequestObjects.AddMember();

        envelope.Header.ABAuthHeader.TicketToken = TicketToken;
        envelope.Header.ABApplicationHeader.PartnerScenario = scenario;

        envelope.Body.AddMember.memberships.Membership.MemberRole = memberRole;
        envelope.Body.AddMember.memberships.Membership.Members.Member.PassportName = email;

        using MemoryStream memory = new();
        requestSerializer.Serialize(memory, envelope);

        string soapXML = Encoding.UTF8.GetString(memory.ToArray());
        string response = await Requests.SoapRequest(soapXML, SharingService, "http://www.msn.com/webservices/AddressBook/AddMember");

        XmlSerializer responseSerializer = new(typeof(SOAP.SerializableClasses.AddMemberResponse.Envelope));

        using (StringReader reader = new StringReader(response))
        {
            var responseEnvelope = (SOAP.SerializableClasses.AddMemberResponse.Envelope?)responseSerializer.Deserialize(reader)
                ?? throw new ContactException("Member add failure");
        }
    }

    public async Task DeleteMember(string memberRole, string email, string scenario = "BlockUnblock")
    {
        XmlSerializer requestSerializer = new(typeof(SOAP.SerializableClasses.DeleteMember.Envelope));

        var envelope = RequestObjects.DeleteMember();

        envelope.Header.ABAuthHeader.TicketToken = TicketToken;
        envelope.Header.ABApplicationHeader.PartnerScenario = scenario;

        envelope.Body.DeleteMember.memberships.Membership.MemberRole = memberRole;
        envelope.Body.DeleteMember.memberships.Membership.Members.Member.PassportName = email;

        using MemoryStream memory = new();
        requestSerializer.Serialize(memory, envelope);

        string soapXML = Encoding.UTF8.GetString(memory.ToArray());
        string response = await Requests.SoapRequest(soapXML, SharingService, "http://www.msn.com/webservices/AddressBook/DeleteMember");

        XmlSerializer responseSerializer = new(typeof(SOAP.SerializableClasses.DeleteMemberResponse.Envelope));

        using (StringReader reader = new StringReader(response))
        {
            var responseEnvelope = (SOAP.SerializableClasses.DeleteMemberResponse.Envelope?)responseSerializer.Deserialize(reader)
                ?? throw new ContactException("Member delete failure");
        }
    }

    public async Task ABContactAdd(string email)
    {
        XmlSerializer requestSerializer = new(typeof(SOAP.SerializableClasses.ABContactAdd.Envelope));

        var envelope = RequestObjects.ABContactAdd();

        envelope.Header.ABAuthHeader.TicketToken = TicketToken;
        envelope.Body.ABContactAdd.contacts.Contact.contactInfo.passportName = email;

        using MemoryStream memory = new();
        requestSerializer.Serialize(memory, envelope);

        string soapXML = Encoding.UTF8.GetString(memory.ToArray());
        string response = await Requests.SoapRequest(soapXML, ABService, "http://www.msn.com/webservices/AddressBook/ABContactAdd");

        XmlSerializer responseSerializer = new(typeof(SOAP.SerializableClasses.ABContactAddResponse.Envelope));

        using (StringReader reader = new StringReader(response))
        {
            var responseEnvelope = (SOAP.SerializableClasses.ABContactAddResponse.Envelope?)responseSerializer.Deserialize(reader)
                ?? throw new ContactException("Contact add failure");
        }
    }

    public async Task ABContactDelete(string contactID)
    {
        XmlSerializer requestSerializer = new(typeof(SOAP.SerializableClasses.ABContactDelete.Envelope));

        var envelope = RequestObjects.ABContactDelete();

        envelope.Header.ABAuthHeader.TicketToken = TicketToken;
        envelope.Body.ABContactDelete.contacts.Contact.contactId = contactID;

        using MemoryStream memory = new();
        requestSerializer.Serialize(memory, envelope);

        string soapXML = Encoding.UTF8.GetString(memory.ToArray());
        string response = await Requests.SoapRequest(soapXML, ABService, "http://www.msn.com/webservices/AddressBook/ABContactDelete");

        XmlSerializer responseSerializer = new(typeof(SOAP.SerializableClasses.ABContactDeleteResponse.Envelope));

        using (StringReader reader = new StringReader(response))
        {
            var responseEnvelope = (SOAP.SerializableClasses.ABContactDeleteResponse.Envelope?)responseSerializer.Deserialize(reader)
                ?? throw new ContactException("Contact delete failure");
        }
    }

    public void UpdateProfile(SOAP.SerializableClasses.ABFindAllResponse.ABFindAllResponseABFindAllResultContact profile)
    {
        var profileInfo = profile.contactInfo;

        Profile.MBEA = profileInfo.annotations[0].Value;
        Profile.GTC = profileInfo.annotations[1].Value;
        Profile.BLP = profileInfo.annotations[2].Value;

        Profile.DisplayName = profileInfo.displayName;
        Profile.Email = profileInfo.passportName;

        Profile.ContactID = profile.contactId;

        Profile.IsFavorite = profileInfo.isFavorite;
        Profile.HasSpace = profileInfo.hasSpace;
        Profile.IsPrivate = profileInfo.IsPrivate;
        Profile.IsEmailHidden = profileInfo.IsPassportNameHidden;

        Profile.Gender = profileInfo.Gender;
        Profile.TimeZone = profileInfo.TimeZone;

        Profile.Birthdate = profileInfo.birthdate;
        Profile.ABLastChanged = profile.lastChange;
    }

    public string InitialListPayload()
    {
        XML.SerializableClasses.InitialListPayload.ml ml = new()
        {
            l = 1
        };

        List<XML.SerializableClasses.InitialListPayload.mlD> domains = new List<XML.SerializableClasses.InitialListPayload.mlD>();

        foreach (Contact contact in Contacts)
        {
            domains.Add(new XML.SerializableClasses.InitialListPayload.mlD()
            {
                n = contact.Email.Split("@")[1],
                c = new XML.SerializableClasses.InitialListPayload.mlDC()
                {
                    n = contact.Email.Split("@")[0],
                    l = (byte)contact.InLists.ListsNumber(),
                    t = (byte)(contact.Type == "Passport" ? 1 : 4),
                }
            });
        }

        ml.d = domains.ToArray();

        var settings = new XmlWriterSettings()
        {
            OmitXmlDeclaration = true,
        };

        var namespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });

        XmlSerializer mlSerializer = new(typeof(XML.SerializableClasses.InitialListPayload.ml));

        using (StringWriter stream = new StringWriter())
        using (XmlWriter writer = XmlWriter.Create(stream, settings))
        {
            mlSerializer.Serialize(writer, ml, namespaces);
            return stream.ToString();
        }
    }

    public string ListPayload(Contact contact, Lists lists)
    {
        XML.SerializableClasses.ListPayload.ml ml = new();

        List<XML.SerializableClasses.ListPayload.mlD> domains = new List<XML.SerializableClasses.ListPayload.mlD>
        {
            new XML.SerializableClasses.ListPayload.mlD()
            {
                n = contact.Email.Split("@")[1],
                c = new XML.SerializableClasses.ListPayload.mlDC()
                {
                    n = contact.Email.Split("@")[0],
                    l = (byte)lists.ListsNumber(),
                    t = (byte)(contact.Type == "Passport" ? 1 : 4),
                }
            }
        };

        ml.d = domains.ToArray();

        var settings = new XmlWriterSettings()
        {
            OmitXmlDeclaration = true,
        };

        var namespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });

        XmlSerializer mlSerializer = new(typeof(XML.SerializableClasses.ListPayload.ml));

        using (StringWriter stream = new StringWriter())
        using (XmlWriter writer = XmlWriter.Create(stream, settings))
        {
            mlSerializer.Serialize(writer, ml, namespaces);
            return stream.ToString();
        }
    }

    public string ContactPayload(Contact contact)
    {
        XML.SerializableClasses.ContactPayload.ml ml = new();

        List<XML.SerializableClasses.ContactPayload.mlD> domains = new List<XML.SerializableClasses.ContactPayload.mlD>
        {
            new XML.SerializableClasses.ContactPayload.mlD()
            {
                n = contact.Email.Split("@")[1],
                c = new XML.SerializableClasses.ContactPayload.mlDC()
                {
                    n = contact.Email.Split("@")[0]
                }
            }
        };

        ml.d = domains.ToArray();

        var settings = new XmlWriterSettings()
        {
            OmitXmlDeclaration = true,
        };

        var namespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });

        XmlSerializer mlSerializer = new(typeof(XML.SerializableClasses.ContactPayload.ml));

        using (StringWriter stream = new StringWriter())
        using (XmlWriter writer = XmlWriter.Create(stream, settings))
        {
            mlSerializer.Serialize(writer, ml, namespaces);
            return stream.ToString();
        }
    }

    public string UUXPayload()
    {
        Data data = new()
        {
            PSM = Profile.PersonalMessage
        };

        var settings = new XmlWriterSettings()
        {
            OmitXmlDeclaration = true
        };

        var namespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });

        XmlSerializer mlSerializer = new(typeof(Data));

        using (StringWriter stream = new StringWriter())
        using (XmlWriter writer = XmlWriter.Create(stream, settings))
        {
            mlSerializer.Serialize(writer, data, namespaces);
            return stream.ToString();
        }
    }
}