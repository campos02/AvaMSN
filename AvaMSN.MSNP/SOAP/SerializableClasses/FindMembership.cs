namespace AvaMSN.MSNP.SOAP.SerializableClasses.FindMembership;

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

    private FindMembership findMembershipField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.msn.com/webservices/AddressBook")]
    public FindMembership FindMembership
    {
        get
        {
            return this.findMembershipField;
        }
        set
        {
            this.findMembershipField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.msn.com/webservices/AddressBook")]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.msn.com/webservices/AddressBook", IsNullable = false)]
public partial class FindMembership
{

    private FindMembershipServiceFilter serviceFilterField;

    /// <remarks/>
    public FindMembershipServiceFilter serviceFilter
    {
        get
        {
            return this.serviceFilterField;
        }
        set
        {
            this.serviceFilterField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.msn.com/webservices/AddressBook")]
public partial class FindMembershipServiceFilter
{

    private string[] typesField;

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute("ServiceType", IsNullable = false)]
    public string[] Types
    {
        get
        {
            return this.typesField;
        }
        set
        {
            this.typesField = value;
        }
    }
}

