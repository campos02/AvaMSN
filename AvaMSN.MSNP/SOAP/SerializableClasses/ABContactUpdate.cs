namespace AvaMSN.MSNP.SOAP.SerializableClasses.ABContactUpdate;

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

    private ABContactUpdate aBContactUpdateField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.msn.com/webservices/AddressBook")]
    public ABContactUpdate ABContactUpdate
    {
        get
        {
            return this.aBContactUpdateField;
        }
        set
        {
            this.aBContactUpdateField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.msn.com/webservices/AddressBook")]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.msn.com/webservices/AddressBook", IsNullable = false)]
public partial class ABContactUpdate
{

    private string abIdField;

    private ABContactUpdateContacts contactsField;

    /// <remarks/>
    public string abId
    {
        get
        {
            return this.abIdField;
        }
        set
        {
            this.abIdField = value;
        }
    }

    /// <remarks/>
    public ABContactUpdateContacts contacts
    {
        get
        {
            return this.contactsField;
        }
        set
        {
            this.contactsField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.msn.com/webservices/AddressBook")]
public partial class ABContactUpdateContacts
{

    private ABContactUpdateContactsContact contactField;

    /// <remarks/>
    public ABContactUpdateContactsContact Contact
    {
        get
        {
            return this.contactField;
        }
        set
        {
            this.contactField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.msn.com/webservices/AddressBook")]
public partial class ABContactUpdateContactsContact
{

    private ABContactUpdateContactsContactContactInfo contactInfoField;

    private string propertiesChangedField;

    /// <remarks/>
    public ABContactUpdateContactsContactContactInfo contactInfo
    {
        get
        {
            return this.contactInfoField;
        }
        set
        {
            this.contactInfoField = value;
        }
    }

    /// <remarks/>
    public string propertiesChanged
    {
        get
        {
            return this.propertiesChangedField;
        }
        set
        {
            this.propertiesChangedField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.msn.com/webservices/AddressBook")]
public partial class ABContactUpdateContactsContactContactInfo
{

    private string contactTypeField;

    private string displayNameField;

    /// <remarks/>
    public string contactType
    {
        get
        {
            return this.contactTypeField;
        }
        set
        {
            this.contactTypeField = value;
        }
    }

    /// <remarks/>
    public string displayName
    {
        get
        {
            return this.displayNameField;
        }
        set
        {
            this.displayNameField = value;
        }
    }
}

