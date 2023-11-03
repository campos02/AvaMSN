namespace AvaMSN.MSNP.SOAP.SerializableClasses.ABFindAllResponse;

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

    private ServiceHeader serviceHeaderField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.msn.com/webservices/AddressBook")]
    public ServiceHeader ServiceHeader
    {
        get
        {
            return this.serviceHeaderField;
        }
        set
        {
            this.serviceHeaderField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.msn.com/webservices/AddressBook")]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.msn.com/webservices/AddressBook", IsNullable = false)]
public partial class ServiceHeader
{

    private string versionField;

    private string cacheKeyField;

    private bool cacheKeyChangedField;

    private string preferredHostNameField;

    private string sessionIdField;

    /// <remarks/>
    public string Version
    {
        get
        {
            return this.versionField;
        }
        set
        {
            this.versionField = value;
        }
    }

    /// <remarks/>
    public string CacheKey
    {
        get
        {
            return this.cacheKeyField;
        }
        set
        {
            this.cacheKeyField = value;
        }
    }

    /// <remarks/>
    public bool CacheKeyChanged
    {
        get
        {
            return this.cacheKeyChangedField;
        }
        set
        {
            this.cacheKeyChangedField = value;
        }
    }

    /// <remarks/>
    public string PreferredHostName
    {
        get
        {
            return this.preferredHostNameField;
        }
        set
        {
            this.preferredHostNameField = value;
        }
    }

    /// <remarks/>
    public string SessionId
    {
        get
        {
            return this.sessionIdField;
        }
        set
        {
            this.sessionIdField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
public partial class EnvelopeBody
{

    private ABFindAllResponse aBFindAllResponseField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.msn.com/webservices/AddressBook")]
    public ABFindAllResponse ABFindAllResponse
    {
        get
        {
            return this.aBFindAllResponseField;
        }
        set
        {
            this.aBFindAllResponseField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.msn.com/webservices/AddressBook")]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.msn.com/webservices/AddressBook", IsNullable = false)]
public partial class ABFindAllResponse
{

    private ABFindAllResponseABFindAllResult aBFindAllResultField;

    /// <remarks/>
    public ABFindAllResponseABFindAllResult ABFindAllResult
    {
        get
        {
            return this.aBFindAllResultField;
        }
        set
        {
            this.aBFindAllResultField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.msn.com/webservices/AddressBook")]
public partial class ABFindAllResponseABFindAllResult
{

    private ABFindAllResponseABFindAllResultGroups groupsField;

    private ABFindAllResponseABFindAllResultContact[] contactsField;

    private ABFindAllResponseABFindAllResultAB abField;

    /// <remarks/>
    public ABFindAllResponseABFindAllResultGroups groups
    {
        get
        {
            return this.groupsField;
        }
        set
        {
            this.groupsField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute("Contact", IsNullable = false)]
    public ABFindAllResponseABFindAllResultContact[] contacts
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

    /// <remarks/>
    public ABFindAllResponseABFindAllResultAB ab
    {
        get
        {
            return this.abField;
        }
        set
        {
            this.abField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.msn.com/webservices/AddressBook")]
public partial class ABFindAllResponseABFindAllResultGroups
{

    private ABFindAllResponseABFindAllResultGroupsGroup groupField;

    /// <remarks/>
    public ABFindAllResponseABFindAllResultGroupsGroup Group
    {
        get
        {
            return this.groupField;
        }
        set
        {
            this.groupField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.msn.com/webservices/AddressBook")]
public partial class ABFindAllResponseABFindAllResultGroupsGroup
{

    private string groupIdField;

    private ABFindAllResponseABFindAllResultGroupsGroupGroupInfo groupInfoField;

    private object propertiesChangedField;

    private bool fDeletedField;

    private System.DateTime lastChangeField;

    /// <remarks/>
    public string groupId
    {
        get
        {
            return this.groupIdField;
        }
        set
        {
            this.groupIdField = value;
        }
    }

    /// <remarks/>
    public ABFindAllResponseABFindAllResultGroupsGroupGroupInfo groupInfo
    {
        get
        {
            return this.groupInfoField;
        }
        set
        {
            this.groupInfoField = value;
        }
    }

    /// <remarks/>
    public object propertiesChanged
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

    /// <remarks/>
    public bool fDeleted
    {
        get
        {
            return this.fDeletedField;
        }
        set
        {
            this.fDeletedField = value;
        }
    }

    /// <remarks/>
    public System.DateTime lastChange
    {
        get
        {
            return this.lastChangeField;
        }
        set
        {
            this.lastChangeField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.msn.com/webservices/AddressBook")]
public partial class ABFindAllResponseABFindAllResultGroupsGroupGroupInfo
{

    private ABFindAllResponseABFindAllResultGroupsGroupGroupInfoAnnotations annotationsField;

    private string groupTypeField;

    private string nameField;

    private bool isNotMobileVisibleField;

    private bool isPrivateField;

    private bool isFavoriteField;

    /// <remarks/>
    public ABFindAllResponseABFindAllResultGroupsGroupGroupInfoAnnotations annotations
    {
        get
        {
            return this.annotationsField;
        }
        set
        {
            this.annotationsField = value;
        }
    }

    /// <remarks/>
    public string groupType
    {
        get
        {
            return this.groupTypeField;
        }
        set
        {
            this.groupTypeField = value;
        }
    }

    /// <remarks/>
    public string name
    {
        get
        {
            return this.nameField;
        }
        set
        {
            this.nameField = value;
        }
    }

    /// <remarks/>
    public bool IsNotMobileVisible
    {
        get
        {
            return this.isNotMobileVisibleField;
        }
        set
        {
            this.isNotMobileVisibleField = value;
        }
    }

    /// <remarks/>
    public bool IsPrivate
    {
        get
        {
            return this.isPrivateField;
        }
        set
        {
            this.isPrivateField = value;
        }
    }

    /// <remarks/>
    public bool IsFavorite
    {
        get
        {
            return this.isFavoriteField;
        }
        set
        {
            this.isFavoriteField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.msn.com/webservices/AddressBook")]
public partial class ABFindAllResponseABFindAllResultGroupsGroupGroupInfoAnnotations
{

    private ABFindAllResponseABFindAllResultGroupsGroupGroupInfoAnnotationsAnnotation annotationField;

    /// <remarks/>
    public ABFindAllResponseABFindAllResultGroupsGroupGroupInfoAnnotationsAnnotation Annotation
    {
        get
        {
            return this.annotationField;
        }
        set
        {
            this.annotationField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.msn.com/webservices/AddressBook")]
public partial class ABFindAllResponseABFindAllResultGroupsGroupGroupInfoAnnotationsAnnotation
{

    private string nameField;

    private byte valueField;

    /// <remarks/>
    public string Name
    {
        get
        {
            return this.nameField;
        }
        set
        {
            this.nameField = value;
        }
    }

    /// <remarks/>
    public byte Value
    {
        get
        {
            return this.valueField;
        }
        set
        {
            this.valueField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.msn.com/webservices/AddressBook")]
public partial class ABFindAllResponseABFindAllResultContact
{

    private string contactIdField;

    private ABFindAllResponseABFindAllResultContactContactInfo contactInfoField;

    private object propertiesChangedField;

    private bool fDeletedField;

    private System.DateTime lastChangeField;

    /// <remarks/>
    public string contactId
    {
        get
        {
            return this.contactIdField;
        }
        set
        {
            this.contactIdField = value;
        }
    }

    /// <remarks/>
    public ABFindAllResponseABFindAllResultContactContactInfo contactInfo
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
    public object propertiesChanged
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

    /// <remarks/>
    public bool fDeleted
    {
        get
        {
            return this.fDeletedField;
        }
        set
        {
            this.fDeletedField = value;
        }
    }

    /// <remarks/>
    public System.DateTime lastChange
    {
        get
        {
            return this.lastChangeField;
        }
        set
        {
            this.lastChangeField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.msn.com/webservices/AddressBook")]
public partial class ABFindAllResponseABFindAllResultContactContactInfo
{

    private ABFindAllResponseABFindAllResultContactContactInfoAnnotation[] annotationsField;

    private string contactTypeField;

    private string quickNameField;

    private string passportNameField;

    private bool isPassportNameHiddenField;

    private string displayNameField;

    private byte puidField;

    private long cIDField;

    private bool isNotMobileVisibleField;

    private bool isMobileIMEnabledField;

    private bool isMessengerUserField;

    private bool isFavoriteField;

    private bool isSmtpField;

    private bool hasSpaceField;

    private string spotWatchStateField;

    private System.DateTime birthdateField;

    private string primaryEmailTypeField;

    private string primaryLocationField;

    private string primaryPhoneField;

    private bool isPrivateField;

    private string genderField;

    private string timeZoneField;

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute("Annotation", IsNullable = false)]
    public ABFindAllResponseABFindAllResultContactContactInfoAnnotation[] annotations
    {
        get
        {
            return this.annotationsField;
        }
        set
        {
            this.annotationsField = value;
        }
    }

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
    public string quickName
    {
        get
        {
            return this.quickNameField;
        }
        set
        {
            this.quickNameField = value;
        }
    }

    /// <remarks/>
    public string passportName
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

    /// <remarks/>
    public bool IsPassportNameHidden
    {
        get
        {
            return this.isPassportNameHiddenField;
        }
        set
        {
            this.isPassportNameHiddenField = value;
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

    /// <remarks/>
    public byte puid
    {
        get
        {
            return this.puidField;
        }
        set
        {
            this.puidField = value;
        }
    }

    /// <remarks/>
    public long CID
    {
        get
        {
            return this.cIDField;
        }
        set
        {
            this.cIDField = value;
        }
    }

    /// <remarks/>
    public bool IsNotMobileVisible
    {
        get
        {
            return this.isNotMobileVisibleField;
        }
        set
        {
            this.isNotMobileVisibleField = value;
        }
    }

    /// <remarks/>
    public bool isMobileIMEnabled
    {
        get
        {
            return this.isMobileIMEnabledField;
        }
        set
        {
            this.isMobileIMEnabledField = value;
        }
    }

    /// <remarks/>
    public bool isMessengerUser
    {
        get
        {
            return this.isMessengerUserField;
        }
        set
        {
            this.isMessengerUserField = value;
        }
    }

    /// <remarks/>
    public bool isFavorite
    {
        get
        {
            return this.isFavoriteField;
        }
        set
        {
            this.isFavoriteField = value;
        }
    }

    /// <remarks/>
    public bool isSmtp
    {
        get
        {
            return this.isSmtpField;
        }
        set
        {
            this.isSmtpField = value;
        }
    }

    /// <remarks/>
    public bool hasSpace
    {
        get
        {
            return this.hasSpaceField;
        }
        set
        {
            this.hasSpaceField = value;
        }
    }

    /// <remarks/>
    public string spotWatchState
    {
        get
        {
            return this.spotWatchStateField;
        }
        set
        {
            this.spotWatchStateField = value;
        }
    }

    /// <remarks/>
    public System.DateTime birthdate
    {
        get
        {
            return this.birthdateField;
        }
        set
        {
            this.birthdateField = value;
        }
    }

    /// <remarks/>
    public string primaryEmailType
    {
        get
        {
            return this.primaryEmailTypeField;
        }
        set
        {
            this.primaryEmailTypeField = value;
        }
    }

    /// <remarks/>
    public string PrimaryLocation
    {
        get
        {
            return this.primaryLocationField;
        }
        set
        {
            this.primaryLocationField = value;
        }
    }

    /// <remarks/>
    public string PrimaryPhone
    {
        get
        {
            return this.primaryPhoneField;
        }
        set
        {
            this.primaryPhoneField = value;
        }
    }

    /// <remarks/>
    public bool IsPrivate
    {
        get
        {
            return this.isPrivateField;
        }
        set
        {
            this.isPrivateField = value;
        }
    }

    /// <remarks/>
    public string Gender
    {
        get
        {
            return this.genderField;
        }
        set
        {
            this.genderField = value;
        }
    }

    /// <remarks/>
    public string TimeZone
    {
        get
        {
            return this.timeZoneField;
        }
        set
        {
            this.timeZoneField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.msn.com/webservices/AddressBook")]
public partial class ABFindAllResponseABFindAllResultContactContactInfoAnnotation
{

    private string nameField;

    private byte valueField;

    /// <remarks/>
    public string Name
    {
        get
        {
            return this.nameField;
        }
        set
        {
            this.nameField = value;
        }
    }

    /// <remarks/>
    public byte Value
    {
        get
        {
            return this.valueField;
        }
        set
        {
            this.valueField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.msn.com/webservices/AddressBook")]
public partial class ABFindAllResponseABFindAllResultAB
{

    private string abIdField;

    private ABFindAllResponseABFindAllResultABAbInfo abInfoField;

    private System.DateTime lastChangeField;

    private System.DateTime dynamicItemLastChangedField;

    private System.DateTime createDateField;

    private object propertiesChangedField;

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
    public ABFindAllResponseABFindAllResultABAbInfo abInfo
    {
        get
        {
            return this.abInfoField;
        }
        set
        {
            this.abInfoField = value;
        }
    }

    /// <remarks/>
    public System.DateTime lastChange
    {
        get
        {
            return this.lastChangeField;
        }
        set
        {
            this.lastChangeField = value;
        }
    }

    /// <remarks/>
    public System.DateTime DynamicItemLastChanged
    {
        get
        {
            return this.dynamicItemLastChangedField;
        }
        set
        {
            this.dynamicItemLastChangedField = value;
        }
    }

    /// <remarks/>
    public System.DateTime createDate
    {
        get
        {
            return this.createDateField;
        }
        set
        {
            this.createDateField = value;
        }
    }

    /// <remarks/>
    public object propertiesChanged
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
public partial class ABFindAllResponseABFindAllResultABAbInfo
{

    private byte ownerPuidField;

    private long ownerCIDField;

    private string ownerEmailField;

    private bool fDefaultField;

    private bool joinedNamespaceField;

    private bool isBotField;

    private bool isParentManagedField;

    private bool subscribeExternalPartnerField;

    private bool notifyExternalPartnerField;

    private string addressBookTypeField;

    /// <remarks/>
    public byte ownerPuid
    {
        get
        {
            return this.ownerPuidField;
        }
        set
        {
            this.ownerPuidField = value;
        }
    }

    /// <remarks/>
    public long OwnerCID
    {
        get
        {
            return this.ownerCIDField;
        }
        set
        {
            this.ownerCIDField = value;
        }
    }

    /// <remarks/>
    public string ownerEmail
    {
        get
        {
            return this.ownerEmailField;
        }
        set
        {
            this.ownerEmailField = value;
        }
    }

    /// <remarks/>
    public bool fDefault
    {
        get
        {
            return this.fDefaultField;
        }
        set
        {
            this.fDefaultField = value;
        }
    }

    /// <remarks/>
    public bool joinedNamespace
    {
        get
        {
            return this.joinedNamespaceField;
        }
        set
        {
            this.joinedNamespaceField = value;
        }
    }

    /// <remarks/>
    public bool IsBot
    {
        get
        {
            return this.isBotField;
        }
        set
        {
            this.isBotField = value;
        }
    }

    /// <remarks/>
    public bool IsParentManaged
    {
        get
        {
            return this.isParentManagedField;
        }
        set
        {
            this.isParentManagedField = value;
        }
    }

    /// <remarks/>
    public bool SubscribeExternalPartner
    {
        get
        {
            return this.subscribeExternalPartnerField;
        }
        set
        {
            this.subscribeExternalPartnerField = value;
        }
    }

    /// <remarks/>
    public bool NotifyExternalPartner
    {
        get
        {
            return this.notifyExternalPartnerField;
        }
        set
        {
            this.notifyExternalPartnerField = value;
        }
    }

    /// <remarks/>
    public string AddressBookType
    {
        get
        {
            return this.addressBookTypeField;
        }
        set
        {
            this.addressBookTypeField = value;
        }
    }
}

