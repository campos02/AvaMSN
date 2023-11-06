using System.Xml.Serialization;

namespace AvaMSN.MSNP.SOAP.SerializableClasses.DeleteMember;

// NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://schemas.xmlsoap.org/soap/envelope/", IsNullable = false)]
public partial class Envelope
{

    private EnvelopeHeader headerField;

    private EnvelopeBody bodyField;

    /// <remarks/>
    public EnvelopeHeader Header
    {
        get
        {
            return this.headerField;
        }
        set
        {
            this.headerField = value;
        }
    }

    /// <remarks/>
    public EnvelopeBody Body
    {
        get
        {
            return this.bodyField;
        }
        set
        {
            this.bodyField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
public partial class EnvelopeHeader
{

    private ABApplicationHeader aBApplicationHeaderField;

    private ABAuthHeader aBAuthHeaderField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.msn.com/webservices/AddressBook")]
    public ABApplicationHeader ABApplicationHeader
    {
        get
        {
            return this.aBApplicationHeaderField;
        }
        set
        {
            this.aBApplicationHeaderField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.msn.com/webservices/AddressBook")]
    public ABAuthHeader ABAuthHeader
    {
        get
        {
            return this.aBAuthHeaderField;
        }
        set
        {
            this.aBAuthHeaderField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.msn.com/webservices/AddressBook")]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.msn.com/webservices/AddressBook", IsNullable = false)]
public partial class ABApplicationHeader
{

    private string applicationIdField;

    private bool isMigrationField;

    private string partnerScenarioField;

    /// <remarks/>
    public string ApplicationId
    {
        get
        {
            return this.applicationIdField;
        }
        set
        {
            this.applicationIdField = value;
        }
    }

    /// <remarks/>
    public bool IsMigration
    {
        get
        {
            return this.isMigrationField;
        }
        set
        {
            this.isMigrationField = value;
        }
    }

    /// <remarks/>
    public string PartnerScenario
    {
        get
        {
            return this.partnerScenarioField;
        }
        set
        {
            this.partnerScenarioField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.msn.com/webservices/AddressBook")]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.msn.com/webservices/AddressBook", IsNullable = false)]
public partial class ABAuthHeader
{

    private bool managedGroupRequestField;

    private object ticketTokenField;

    /// <remarks/>
    public bool ManagedGroupRequest
    {
        get
        {
            return this.managedGroupRequestField;
        }
        set
        {
            this.managedGroupRequestField = value;
        }
    }

    /// <remarks/>
    public object TicketToken
    {
        get
        {
            return this.ticketTokenField;
        }
        set
        {
            this.ticketTokenField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
public partial class EnvelopeBody
{

    private DeleteMember deleteMemberField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.msn.com/webservices/AddressBook")]
    public DeleteMember DeleteMember
    {
        get
        {
            return this.deleteMemberField;
        }
        set
        {
            this.deleteMemberField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.msn.com/webservices/AddressBook")]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.msn.com/webservices/AddressBook", IsNullable = false)]
public partial class DeleteMember
{

    private DeleteMemberServiceHandle serviceHandleField;

    private DeleteMemberMemberships membershipsField;

    /// <remarks/>
    public DeleteMemberServiceHandle serviceHandle
    {
        get
        {
            return this.serviceHandleField;
        }
        set
        {
            this.serviceHandleField = value;
        }
    }

    /// <remarks/>
    public DeleteMemberMemberships memberships
    {
        get
        {
            return this.membershipsField;
        }
        set
        {
            this.membershipsField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.msn.com/webservices/AddressBook")]
public partial class DeleteMemberServiceHandle
{

    private byte idField;

    private string typeField;

    private object foreignIdField;

    /// <remarks/>
    public byte Id
    {
        get
        {
            return this.idField;
        }
        set
        {
            this.idField = value;
        }
    }

    /// <remarks/>
    public string Type
    {
        get
        {
            return this.typeField;
        }
        set
        {
            this.typeField = value;
        }
    }

    /// <remarks/>
    public object ForeignId
    {
        get
        {
            return this.foreignIdField;
        }
        set
        {
            this.foreignIdField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.msn.com/webservices/AddressBook")]
public partial class DeleteMemberMemberships
{

    private DeleteMemberMembershipsMembership membershipField;

    /// <remarks/>
    public DeleteMemberMembershipsMembership Membership
    {
        get
        {
            return this.membershipField;
        }
        set
        {
            this.membershipField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.msn.com/webservices/AddressBook")]
public partial class DeleteMemberMembershipsMembership
{

    private string memberRoleField;

    private DeleteMemberMembershipsMembershipMembers membersField;

    /// <remarks/>
    public string MemberRole
    {
        get
        {
            return this.memberRoleField;
        }
        set
        {
            this.memberRoleField = value;
        }
    }

    /// <remarks/>
    public DeleteMemberMembershipsMembershipMembers Members
    {
        get
        {
            return this.membersField;
        }
        set
        {
            this.membersField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.msn.com/webservices/AddressBook")]
public partial class DeleteMemberMembershipsMembershipMembers
{

    private DeleteMemberMembershipsMembershipMembersMember memberField;

    /// <remarks/>
    public DeleteMemberMembershipsMembershipMembersMember Member
    {
        get
        {
            return this.memberField;
        }
        set
        {
            this.memberField = value;
        }
    }
}

/// <remarks/>
[XmlInclude(typeof(PassportMember))]
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(TypeName = "PassportMember", Namespace = "http://www.msn.com/webservices/AddressBook")]
public partial class DeleteMemberMembershipsMembershipMembersMember
{

    private string typeField;

    private string stateField;

    private string passportNameField;

    /// <remarks/>
    public string Type
    {
        get
        {
            return this.typeField;
        }
        set
        {
            this.typeField = value;
        }
    }

    /// <remarks/>
    public string State
    {
        get
        {
            return this.stateField;
        }
        set
        {
            this.stateField = value;
        }
    }

    /// <remarks/>
    public string PassportName
    {
        get
        {
            return this.passportNameField;
        }
        set
        {
            this.passportNameField = value;
        }
    }
}

public class PassportMember : DeleteMemberMembershipsMembershipMembersMember { }