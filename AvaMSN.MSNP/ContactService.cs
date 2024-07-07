using System.Text;
using System.Xml.Serialization;
using AvaMSN.MSNP.Exceptions;
using AvaMSN.MSNP.Models;
using AvaMSN.MSNP.SOAP;
using AvaMSN.MSNP.SOAP.RequestObjects;
using Serilog;

namespace AvaMSN.MSNP;

/// <summary>
/// Makes requests to the ABCH contact service.
/// </summary>
internal static class ContactService
{
    /// <summary>
    /// Contact API token.
    /// </summary>
    public static string TicketToken { get; set; } = string.Empty;
    public static string SharingServiceUrl { get; set; } = string.Empty;
    public static string ABServiceUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets all contacts in the ABCH membership lists (allow, block, reverse or pending)
    /// and adds all of them to their respective lists in memory.
    /// </summary>
    /// <param name="contacts">Contact list to be modified.</param>
    /// <returns></returns>
    public static async Task FindMembership(List<Contact> contacts)
    {
        XmlSerializer requestSerializer = new(typeof(SOAP.SerializableClasses.FindMembership.Envelope));

        var envelope = RequestObjects.FindMembership();
        envelope.Header.ABAuthHeader.TicketToken = TicketToken;
        using MemoryStream memory = new();
        requestSerializer.Serialize(memory, envelope);

        Log.Information("Making SOAP request {Request} to {Url}", nameof(FindMembership), SharingServiceUrl);
        string soapXML = Encoding.UTF8.GetString(memory.ToArray());
        string response = await Requests.MakeRequest(soapXML, SharingServiceUrl, "http://www.msn.com/webservices/AddressBook/FindMembership");

        XmlSerializer responseSerializer = new(typeof(SOAP.SerializableClasses.FindMembershipResponse.Envelope));

        // Deserialize response and add contacts
        using StringReader reader = new StringReader(response);
        var responseEnvelope = (SOAP.SerializableClasses.FindMembershipResponse.Envelope?)responseSerializer.Deserialize(reader);
        foreach (var membership in responseEnvelope!.Body.FindMembershipResponse.FindMembershipResult.Services.Service.Memberships)
        {
            foreach(var member in membership.Members)
            {
                if (member.Type != "Passport")
                    continue;

                // Remove list from ID
                string contactID = member.MembershipId.Replace($"{membership.MemberRole}/", "");

                Contact contact = contacts.FirstOrDefault(c => c.ContactID == contactID) ?? new Contact();
                contact.InLists.SetMembershipList(membership.MemberRole);
                contact.Email = member.PassportName;
                contact.ContactID = contactID;
                contact.Type = member.Type;
                contact.State = member.State;
                contact.MembershipLastChanged = member.LastChanged;
                contact.JoinedDate = member.JoinedDate;
                contact.ExpirationDate = member.ExpirationDate;
                contact.IsEmailHidden = member.IsPassportNameHidden;

                // Don't add contacts twice
                if (!contacts.Contains(contact))
                    contacts.Add(contact);
            }
        }
    }

    /// <summary>
    /// Gets all contacts in the ABCH forward list and adds them all to the respective list in memory.
    /// </summary>
    /// <returns></returns>
    public static async Task ABFindAll(List<Contact> contacts, User? user)
    {
        XmlSerializer requestSerializer = new(typeof(SOAP.SerializableClasses.ABFindAll.Envelope));

        var envelope = RequestObjects.ABFindAll();
        envelope.Header.ABAuthHeader.TicketToken = TicketToken;

        using MemoryStream memory = new();
        requestSerializer.Serialize(memory, envelope);

        Log.Information("Making SOAP request {Request} to {Url}", nameof(ABFindAll), ABServiceUrl);
        string soapXML = Encoding.UTF8.GetString(memory.ToArray());
        string response = await Requests.MakeRequest(soapXML, ABServiceUrl, "http://www.msn.com/webservices/AddressBook/ABFindAll");

        XmlSerializer responseSerializer = new(typeof(SOAP.SerializableClasses.ABFindAllResponse.Envelope));

        // Deserialize response and add contacts
        using StringReader reader = new StringReader(response);
        var responseEnvelope = (SOAP.SerializableClasses.ABFindAllResponse.Envelope?)responseSerializer.Deserialize(reader);
        foreach (var addressBookContact in responseEnvelope!.Body.ABFindAllResponse.ABFindAllResult.contacts)
        {
            var contactInfo = addressBookContact.contactInfo;

            // Handle user info
            if (contactInfo.contactType == "Me")
            {
                UpdateUser(addressBookContact, user);
                continue;
            }

            Contact contact = contacts.FirstOrDefault(c => c.ContactID == addressBookContact.contactId) ?? new Contact();
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
            contact.Type = "Passport";

            // Don't add contacts twice
            if (!contacts.Contains(contact))
                contacts.Add(contact);
        }
    }

    /// <summary>
    /// Change a user's display name in the ABCH, which must be set in the Profile property before running the command.
    /// </summary>
    /// <param name="displayName">User display name.</param>
    /// <returns></returns>
    /// <exception cref="ContactException">Thrown if response can't be deserialized</exception>
    public static async Task ChangeDisplayName(string displayName)
    {
        XmlSerializer requestSerializer = new(typeof(SOAP.SerializableClasses.ABContactUpdate.Envelope));
        
        var envelope = RequestObjects.AbContactUpdate();
        envelope.Header.ABAuthHeader.TicketToken = TicketToken;

        // Update user display name
        envelope.Body.ABContactUpdate.contacts.Contact.contactInfo.contactType = "Me";
        envelope.Body.ABContactUpdate.contacts.Contact.contactInfo.displayName = displayName;

        using MemoryStream memory = new();
        requestSerializer.Serialize(memory, envelope);

        Log.Information("Making SOAP request {Request} to {Url}", nameof(ChangeDisplayName), ABServiceUrl);
        string soapXML = Encoding.UTF8.GetString(memory.ToArray());
        string response = await Requests.MakeRequest(soapXML, ABServiceUrl, "http://www.msn.com/webservices/AddressBook/ABContactUpdate");

        XmlSerializer responseSerializer = new(typeof(SOAP.SerializableClasses.ABContactUpdateResponse.Envelope));
        using StringReader reader = new StringReader(response);
        
        // Throw exception if deserialization is not possible
        var responseEnvelope = (SOAP.SerializableClasses.ABContactUpdateResponse.Envelope?)responseSerializer.Deserialize(reader)
                               ?? throw new ContactException("Contact update failure");
    }

    /// <summary>
    /// Adds a contact to an ABCH membership list (allow, block, reverse or pending).
    /// </summary>
    /// <param name="memberRole">List to add to.</param>
    /// <param name="email">Contact email.</param>
    /// <param name="scenario">Request scenario.</param>
    /// <returns></returns>
    /// <exception cref="ContactException">Thrown if response can't be deserialized.</exception>
    public static async Task AddMember(string memberRole, string email, string scenario = "BlockUnblock")
    {
        XmlSerializer requestSerializer = new(typeof(SOAP.SerializableClasses.AddMember.Envelope));
        
        var envelope = RequestObjects.AddMember();
        envelope.Header.ABAuthHeader.TicketToken = TicketToken;
        envelope.Header.ABApplicationHeader.PartnerScenario = scenario;
        envelope.Body.AddMember.memberships.Membership.MemberRole = memberRole;
        envelope.Body.AddMember.memberships.Membership.Members.Member.PassportName = email;

        using MemoryStream memory = new();
        requestSerializer.Serialize(memory, envelope);

        Log.Information("Making SOAP request {Request} to {Url}", nameof(AddMember), SharingServiceUrl);
        string soapXML = Encoding.UTF8.GetString(memory.ToArray());
        string response = await Requests.MakeRequest(soapXML, SharingServiceUrl, "http://www.msn.com/webservices/AddressBook/AddMember");

        XmlSerializer responseSerializer = new(typeof(SOAP.SerializableClasses.AddMemberResponse.Envelope));
        using StringReader reader = new StringReader(response);
        
        // Throw exception if deserialization is not possible
        var responseEnvelope = (SOAP.SerializableClasses.AddMemberResponse.Envelope?)responseSerializer.Deserialize(reader)
                               ?? throw new ContactException("Member add failure");
    }

    /// <summary>
    /// Deletes a contact from an ABCH membership list (allow, block, reverse or pending).
    /// </summary>
    /// <param name="memberRole">List to delete from.</param>
    /// <param name="email">Contact email.</param>
    /// <param name="scenario">Request scenario.</param>
    /// <returns></returns>
    /// <exception cref="ContactException">Thrown if response can't be deserialized.</exception>
    public static async Task DeleteMember(string memberRole, string email, string scenario = "BlockUnblock")
    {
        XmlSerializer requestSerializer = new(typeof(SOAP.SerializableClasses.DeleteMember.Envelope));
        
        var envelope = RequestObjects.DeleteMember();
        envelope.Header.ABAuthHeader.TicketToken = TicketToken;
        envelope.Header.ABApplicationHeader.PartnerScenario = scenario;
        envelope.Body.DeleteMember.memberships.Membership.MemberRole = memberRole;
        envelope.Body.DeleteMember.memberships.Membership.Members.Member.PassportName = email;

        using MemoryStream memory = new();
        requestSerializer.Serialize(memory, envelope);

        Log.Information("Making SOAP request {Request} to {Url}", nameof(DeleteMember), SharingServiceUrl);
        string soapXML = Encoding.UTF8.GetString(memory.ToArray());
        string response = await Requests.MakeRequest(soapXML, SharingServiceUrl, "http://www.msn.com/webservices/AddressBook/DeleteMember");

        XmlSerializer responseSerializer = new(typeof(SOAP.SerializableClasses.DeleteMemberResponse.Envelope));
        using StringReader reader = new StringReader(response);
        
        // Throw exception if deserialization is not possible
        var responseEnvelope = (SOAP.SerializableClasses.DeleteMemberResponse.Envelope?)responseSerializer.Deserialize(reader)
                               ?? throw new ContactException("Member delete failure");
    }

    /// <summary>
    /// Adds a contact to the ABCH forward list.
    /// </summary>
    /// <param name="email">Contact email.</param>
    /// <returns></returns>
    /// <exception cref="ContactException">Thrown if response can't be deserialized.</exception>
    public static async Task ABContactAdd(string email)
    {
        XmlSerializer requestSerializer = new(typeof(SOAP.SerializableClasses.ABContactAdd.Envelope));
        
        var envelope = RequestObjects.ABContactAdd();
        envelope.Header.ABAuthHeader.TicketToken = TicketToken;
        envelope.Body.ABContactAdd.contacts.Contact.contactInfo.passportName = email;

        using MemoryStream memory = new();
        requestSerializer.Serialize(memory, envelope);

        Log.Information("Making SOAP request {Request} to {Url}", nameof(ABContactAdd), ABServiceUrl);
        string soapXML = Encoding.UTF8.GetString(memory.ToArray());
        string response = await Requests.MakeRequest(soapXML, ABServiceUrl, "http://www.msn.com/webservices/AddressBook/ABContactAdd");

        XmlSerializer responseSerializer = new(typeof(SOAP.SerializableClasses.ABContactAddResponse.Envelope));
        using StringReader reader = new StringReader(response);
        
        // Throw exception if deserialization is not possible
        var responseEnvelope = (SOAP.SerializableClasses.ABContactAddResponse.Envelope?)responseSerializer.Deserialize(reader)
                               ?? throw new ContactException("Contact add failure");
    }

    /// <summary>
    /// Removes a contact from the ABCH forward list.
    /// </summary>
    /// <param name="contactID">Contact ID as returned from ABFindAll.</param>
    /// <returns></returns>
    /// <exception cref="ContactException">Thrown if response can't be deserialized.</exception>
    public static async Task ABContactDelete(string contactID)
    {
        XmlSerializer requestSerializer = new(typeof(SOAP.SerializableClasses.ABContactDelete.Envelope));

        var envelope = RequestObjects.ABContactDelete();
        envelope.Header.ABAuthHeader.TicketToken = TicketToken;
        envelope.Body.ABContactDelete.contacts.Contact.contactId = contactID;

        using MemoryStream memory = new();
        requestSerializer.Serialize(memory, envelope);

        Log.Information("Making SOAP request {Request} to {Url}", nameof(ABContactDelete), ABServiceUrl);
        string soapXML = Encoding.UTF8.GetString(memory.ToArray());
        string response = await Requests.MakeRequest(soapXML, ABServiceUrl, "http://www.msn.com/webservices/AddressBook/ABContactDelete");

        XmlSerializer responseSerializer = new(typeof(SOAP.SerializableClasses.ABContactDeleteResponse.Envelope));
        using StringReader reader = new StringReader(response);
        
        // Throw exception if deserialization is not possible
        var responseEnvelope = (SOAP.SerializableClasses.ABContactDeleteResponse.Envelope?)responseSerializer.Deserialize(reader)
                               ?? throw new ContactException("Contact delete failure");
    }

    /// <summary>
    /// Sets profile information from ABFindAll response.
    /// </summary>
    /// <param name="xmlUser">Deserialized user info.</param>
    /// <param name="user">User info object.</param>
    private static void UpdateUser(SOAP.SerializableClasses.ABFindAllResponse.ABFindAllResponseABFindAllResultContact xmlUser, User? user)
    {
        var profileInfo = xmlUser.contactInfo;
        if (user != null)
        {
            user.MBEA = Convert.ToInt32(profileInfo.annotations[0].Value);
            user.GTC = Convert.ToInt32(profileInfo.annotations[1].Value);
            user.BLP = Convert.ToInt32(profileInfo.annotations[2].Value);
            user.DisplayName = profileInfo.displayName;
            user.Email = profileInfo.passportName;
            user.ContactID = xmlUser.contactId;
            user.IsFavorite = profileInfo.isFavorite;
            user.HasSpace = profileInfo.hasSpace;
            user.IsPrivate = profileInfo.IsPrivate;
            user.IsEmailHidden = profileInfo.IsPassportNameHidden;
            user.Gender = profileInfo.Gender;
            user.TimeZone = profileInfo.TimeZone;
            user.Birthdate = profileInfo.birthdate;
            user.ABLastChanged = xmlUser.lastChange;
        }
    }
}