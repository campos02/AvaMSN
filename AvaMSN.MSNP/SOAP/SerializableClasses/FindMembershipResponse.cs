namespace AvaMSN.MSNP.SOAP.SerializableClasses.FindMembershipResponse;

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

    private FindMembershipResponse findMembershipResponseField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.msn.com/webservices/AddressBook")]
    public FindMembershipResponse FindMembershipResponse
    {
        get
        {
            return this.findMembershipResponseField;
        }
        set
        {
            this.findMembershipResponseField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.msn.com/webservices/AddressBook")]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.msn.com/webservices/AddressBook", IsNullable = false)]
public partial class FindMembershipResponse
{

    private FindMembershipResponseFindMembershipResult findMembershipResultField;

    /// <remarks/>
    public FindMembershipResponseFindMembershipResult FindMembershipResult
    {
        get
        {
            return this.findMembershipResultField;
        }
        set
        {
            this.findMembershipResultField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.msn.com/webservices/AddressBook")]
public partial class FindMembershipResponseFindMembershipResult
{

    private FindMembershipResponseFindMembershipResultServices servicesField;

    private FindMembershipResponseFindMembershipResultOwnerNamespace ownerNamespaceField;

    /// <remarks/>
    public FindMembershipResponseFindMembershipResultServices Services
    {
        get
        {
            return this.servicesField;
        }
        set
        {
            this.servicesField = value;
        }
    }

    /// <remarks/>
    public FindMembershipResponseFindMembershipResultOwnerNamespace OwnerNamespace
    {
        get
        {
            return this.ownerNamespaceField;
        }
        set
        {
            this.ownerNamespaceField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.msn.com/webservices/AddressBook")]
public partial class FindMembershipResponseFindMembershipResultServices
{

    private FindMembershipResponseFindMembershipResultServicesService serviceField;

    /// <remarks/>
    public FindMembershipResponseFindMembershipResultServicesService Service
    {
        get
        {
            return this.serviceField;
        }
        set
        {
            this.serviceField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.msn.com/webservices/AddressBook")]
public partial class FindMembershipResponseFindMembershipResultServicesService
{

    private FindMembershipResponseFindMembershipResultServicesServiceMembership[] membershipsField;

    private FindMembershipResponseFindMembershipResultServicesServiceInfo infoField;

    private object changesField;

    private System.DateTime lastChangeField;

    private bool deletedField;

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute("Membership", IsNullable = false)]
    public FindMembershipResponseFindMembershipResultServicesServiceMembership[] Memberships
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

    /// <remarks/>
    public FindMembershipResponseFindMembershipResultServicesServiceInfo Info
    {
        get
        {
            return this.infoField;
        }
        set
        {
            this.infoField = value;
        }
    }

    /// <remarks/>
    public object Changes
    {
        get
        {
            return this.changesField;
        }
        set
        {
            this.changesField = value;
        }
    }

    /// <remarks/>
    public System.DateTime LastChange
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
    public bool Deleted
    {
        get
        {
            return this.deletedField;
        }
        set
        {
            this.deletedField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.msn.com/webservices/AddressBook")]
public partial class FindMembershipResponseFindMembershipResultServicesServiceMembership
{

    private string memberRoleField;

    private FindMembershipResponseFindMembershipResultServicesServiceMembershipMember[] membersField;

    private bool membershipIsCompleteField;

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
    [System.Xml.Serialization.XmlArrayItemAttribute("Member", IsNullable = false)]
    public FindMembershipResponseFindMembershipResultServicesServiceMembershipMember[] Members
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

    /// <remarks/>
    public bool MembershipIsComplete
    {
        get
        {
            return this.membershipIsCompleteField;
        }
        set
        {
            this.membershipIsCompleteField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(TypeName = "PassportMember", Namespace = "http://www.msn.com/webservices/AddressBook")]
public partial class FindMembershipResponseFindMembershipResultServicesServiceMembershipMember
{

    private string membershipIdField;

    private string typeField;

    private string stateField;

    private bool deletedField;

    private System.DateTime lastChangedField;

    private System.DateTime joinedDateField;

    private System.DateTime expirationDateField;

    private object changesField;

    private string passportNameField;

    private bool isPassportNameHiddenField;

    private byte passportIdField;

    private long cIDField;

    private object passportChangesField;

    private bool lookedupByCIDField;

    /// <remarks/>
    public string MembershipId
    {
        get
        {
            return this.membershipIdField;
        }
        set
        {
            this.membershipIdField = value;
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
    public bool Deleted
    {
        get
        {
            return this.deletedField;
        }
        set
        {
            this.deletedField = value;
        }
    }

    /// <remarks/>
    public System.DateTime LastChanged
    {
        get
        {
            return this.lastChangedField;
        }
        set
        {
            this.lastChangedField = value;
        }
    }

    /// <remarks/>
    public System.DateTime JoinedDate
    {
        get
        {
            return this.joinedDateField;
        }
        set
        {
            this.joinedDateField = value;
        }
    }

    /// <remarks/>
    public System.DateTime ExpirationDate
    {
        get
        {
            return this.expirationDateField;
        }
        set
        {
            this.expirationDateField = value;
        }
    }

    /// <remarks/>
    public object Changes
    {
        get
        {
            return this.changesField;
        }
        set
        {
            this.changesField = value;
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
    public byte PassportId
    {
        get
        {
            return this.passportIdField;
        }
        set
        {
            this.passportIdField = value;
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
    public object PassportChanges
    {
        get
        {
            return this.passportChangesField;
        }
        set
        {
            this.passportChangesField = value;
        }
    }

    /// <remarks/>
    public bool LookedupByCID
    {
        get
        {
            return this.lookedupByCIDField;
        }
        set
        {
            this.lookedupByCIDField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.msn.com/webservices/AddressBook")]
public partial class FindMembershipResponseFindMembershipResultServicesServiceInfo
{

    private FindMembershipResponseFindMembershipResultServicesServiceInfoHandle handleField;

    private bool inverseRequiredField;

    private string authorizationCriteriaField;

    private bool isBotField;

    /// <remarks/>
    public FindMembershipResponseFindMembershipResultServicesServiceInfoHandle Handle
    {
        get
        {
            return this.handleField;
        }
        set
        {
            this.handleField = value;
        }
    }

    /// <remarks/>
    public bool InverseRequired
    {
        get
        {
            return this.inverseRequiredField;
        }
        set
        {
            this.inverseRequiredField = value;
        }
    }

    /// <remarks/>
    public string AuthorizationCriteria
    {
        get
        {
            return this.authorizationCriteriaField;
        }
        set
        {
            this.authorizationCriteriaField = value;
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
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.msn.com/webservices/AddressBook")]
public partial class FindMembershipResponseFindMembershipResultServicesServiceInfoHandle
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
public partial class FindMembershipResponseFindMembershipResultOwnerNamespace
{

    private FindMembershipResponseFindMembershipResultOwnerNamespaceInfo infoField;

    private object changesField;

    private System.DateTime createDateField;

    private System.DateTime lastChangeField;

    /// <remarks/>
    public FindMembershipResponseFindMembershipResultOwnerNamespaceInfo Info
    {
        get
        {
            return this.infoField;
        }
        set
        {
            this.infoField = value;
        }
    }

    /// <remarks/>
    public object Changes
    {
        get
        {
            return this.changesField;
        }
        set
        {
            this.changesField = value;
        }
    }

    /// <remarks/>
    public System.DateTime CreateDate
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
    public System.DateTime LastChange
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
public partial class FindMembershipResponseFindMembershipResultOwnerNamespaceInfo
{

    private FindMembershipResponseFindMembershipResultOwnerNamespaceInfoHandle handleField;

    private byte creatorPuidField;

    private ulong creatorCIDField;

    private string creatorPassportNameField;

    private FindMembershipResponseFindMembershipResultOwnerNamespaceInfoCircleAttributes circleAttributesField;

    private bool messengerApplicationServiceCreatedField;

    /// <remarks/>
    public FindMembershipResponseFindMembershipResultOwnerNamespaceInfoHandle Handle
    {
        get
        {
            return this.handleField;
        }
        set
        {
            this.handleField = value;
        }
    }

    /// <remarks/>
    public byte CreatorPuid
    {
        get
        {
            return this.creatorPuidField;
        }
        set
        {
            this.creatorPuidField = value;
        }
    }

    /// <remarks/>
    public ulong CreatorCID
    {
        get
        {
            return this.creatorCIDField;
        }
        set
        {
            this.creatorCIDField = value;
        }
    }

    /// <remarks/>
    public string CreatorPassportName
    {
        get
        {
            return this.creatorPassportNameField;
        }
        set
        {
            this.creatorPassportNameField = value;
        }
    }

    /// <remarks/>
    public FindMembershipResponseFindMembershipResultOwnerNamespaceInfoCircleAttributes CircleAttributes
    {
        get
        {
            return this.circleAttributesField;
        }
        set
        {
            this.circleAttributesField = value;
        }
    }

    /// <remarks/>
    public bool MessengerApplicationServiceCreated
    {
        get
        {
            return this.messengerApplicationServiceCreatedField;
        }
        set
        {
            this.messengerApplicationServiceCreatedField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.msn.com/webservices/AddressBook")]
public partial class FindMembershipResponseFindMembershipResultOwnerNamespaceInfoHandle
{

    private string idField;

    private bool isPassportNameHiddenField;

    private byte cIDField;

    /// <remarks/>
    public string Id
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
    public byte CID
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
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.msn.com/webservices/AddressBook")]
public partial class FindMembershipResponseFindMembershipResultOwnerNamespaceInfoCircleAttributes
{

    private bool isPresenceEnabledField;

    private string domainField;

    /// <remarks/>
    public bool IsPresenceEnabled
    {
        get
        {
            return this.isPresenceEnabledField;
        }
        set
        {
            this.isPresenceEnabledField = value;
        }
    }

    /// <remarks/>
    public string Domain
    {
        get
        {
            return this.domainField;
        }
        set
        {
            this.domainField = value;
        }
    }
}

