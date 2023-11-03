namespace AvaMSN.MSNP.SOAP.SerializableClasses.RstResponse;

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

    private pp ppField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://schemas.microsoft.com/Passport/SoapServices/SOAPFault")]
    public pp pp
    {
        get
        {
            return this.ppField;
        }
        set
        {
            this.ppField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/Passport/SoapServices/SOAPFault")]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://schemas.microsoft.com/Passport/SoapServices/SOAPFault", IsNullable = false)]
public partial class pp
{

    private byte serverVersionField;

    private string pUIDField;

    private string configVersionField;

    private string uiVersionField;

    private string mobileConfigVersionField;

    private string authstateField;

    private string reqstatusField;

    private ppServerInfo serverInfoField;

    private object cookiesField;

    private ppBrowserCookie[] browserCookiesField;

    private ppCredProperty[] credPropertiesField;

    private ppExtProperty[] extPropertiesField;

    private object responseField;

    /// <remarks/>
    public byte serverVersion
    {
        get
        {
            return this.serverVersionField;
        }
        set
        {
            this.serverVersionField = value;
        }
    }

    /// <remarks/>
    public string PUID
    {
        get
        {
            return this.pUIDField;
        }
        set
        {
            this.pUIDField = value;
        }
    }

    /// <remarks/>
    public string configVersion
    {
        get
        {
            return this.configVersionField;
        }
        set
        {
            this.configVersionField = value;
        }
    }

    /// <remarks/>
    public string uiVersion
    {
        get
        {
            return this.uiVersionField;
        }
        set
        {
            this.uiVersionField = value;
        }
    }

    /// <remarks/>
    public string mobileConfigVersion
    {
        get
        {
            return this.mobileConfigVersionField;
        }
        set
        {
            this.mobileConfigVersionField = value;
        }
    }

    /// <remarks/>
    public string authstate
    {
        get
        {
            return this.authstateField;
        }
        set
        {
            this.authstateField = value;
        }
    }

    /// <remarks/>
    public string reqstatus
    {
        get
        {
            return this.reqstatusField;
        }
        set
        {
            this.reqstatusField = value;
        }
    }

    /// <remarks/>
    public ppServerInfo serverInfo
    {
        get
        {
            return this.serverInfoField;
        }
        set
        {
            this.serverInfoField = value;
        }
    }

    /// <remarks/>
    public object cookies
    {
        get
        {
            return this.cookiesField;
        }
        set
        {
            this.cookiesField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute("browserCookie", IsNullable = false)]
    public ppBrowserCookie[] browserCookies
    {
        get
        {
            return this.browserCookiesField;
        }
        set
        {
            this.browserCookiesField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute("credProperty", IsNullable = false)]
    public ppCredProperty[] credProperties
    {
        get
        {
            return this.credPropertiesField;
        }
        set
        {
            this.credPropertiesField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute("extProperty", IsNullable = false)]
    public ppExtProperty[] extProperties
    {
        get
        {
            return this.extPropertiesField;
        }
        set
        {
            this.extPropertiesField = value;
        }
    }

    /// <remarks/>
    public object response
    {
        get
        {
            return this.responseField;
        }
        set
        {
            this.responseField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/Passport/SoapServices/SOAPFault")]
public partial class ppServerInfo
{

    private string pathField;

    private string rollingUpgradeStateField;

    private byte locVersionField;

    private System.DateTime serverTimeField;

    private string valueField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Path
    {
        get
        {
            return this.pathField;
        }
        set
        {
            this.pathField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string RollingUpgradeState
    {
        get
        {
            return this.rollingUpgradeStateField;
        }
        set
        {
            this.rollingUpgradeStateField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public byte LocVersion
    {
        get
        {
            return this.locVersionField;
        }
        set
        {
            this.locVersionField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public System.DateTime ServerTime
    {
        get
        {
            return this.serverTimeField;
        }
        set
        {
            this.serverTimeField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTextAttribute()]
    public string Value
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
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/Passport/SoapServices/SOAPFault")]
public partial class ppBrowserCookie
{

    private string nameField;

    private string uRLField;

    private string valueField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
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
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string URL
    {
        get
        {
            return this.uRLField;
        }
        set
        {
            this.uRLField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTextAttribute()]
    public string Value
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
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/Passport/SoapServices/SOAPFault")]
public partial class ppCredProperty
{

    private string nameField;

    private string valueField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
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
    [System.Xml.Serialization.XmlTextAttribute()]
    public string Value
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
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/Passport/SoapServices/SOAPFault")]
public partial class ppExtProperty
{

    private string nameField;

    private string expiryField;

    private string domainsField;

    private bool ignoreRememberMeField;

    private bool ignoreRememberMeFieldSpecified;

    private string valueField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
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
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Expiry
    {
        get
        {
            return this.expiryField;
        }
        set
        {
            this.expiryField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Domains
    {
        get
        {
            return this.domainsField;
        }
        set
        {
            this.domainsField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool IgnoreRememberMe
    {
        get
        {
            return this.ignoreRememberMeField;
        }
        set
        {
            this.ignoreRememberMeField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool IgnoreRememberMeSpecified
    {
        get
        {
            return this.ignoreRememberMeFieldSpecified;
        }
        set
        {
            this.ignoreRememberMeFieldSpecified = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTextAttribute()]
    public string Value
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
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
public partial class EnvelopeBody
{

    private RequestSecurityTokenResponseCollectionRequestSecurityTokenResponse[] requestSecurityTokenResponseCollectionField;

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayAttribute(Namespace = "http://schemas.xmlsoap.org/ws/2004/04/trust")]
    [System.Xml.Serialization.XmlArrayItemAttribute("RequestSecurityTokenResponse", IsNullable = false)]
    public RequestSecurityTokenResponseCollectionRequestSecurityTokenResponse[] RequestSecurityTokenResponseCollection
    {
        get
        {
            return this.requestSecurityTokenResponseCollectionField;
        }
        set
        {
            this.requestSecurityTokenResponseCollectionField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.xmlsoap.org/ws/2004/04/trust")]
public partial class RequestSecurityTokenResponseCollectionRequestSecurityTokenResponse
{

    private string tokenTypeField;

    private AppliesTo appliesToField;

    private RequestSecurityTokenResponseCollectionRequestSecurityTokenResponseLifeTime lifeTimeField;

    private RequestSecurityTokenResponseCollectionRequestSecurityTokenResponseRequestedSecurityToken requestedSecurityTokenField;

    private RequestSecurityTokenResponseCollectionRequestSecurityTokenResponseRequestedTokenReference requestedTokenReferenceField;

    private RequestSecurityTokenResponseCollectionRequestSecurityTokenResponseRequestedProofToken requestedProofTokenField;

    /// <remarks/>
    public string TokenType
    {
        get
        {
            return this.tokenTypeField;
        }
        set
        {
            this.tokenTypeField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://schemas.xmlsoap.org/ws/2002/12/policy")]
    public AppliesTo AppliesTo
    {
        get
        {
            return this.appliesToField;
        }
        set
        {
            this.appliesToField = value;
        }
    }

    /// <remarks/>
    public RequestSecurityTokenResponseCollectionRequestSecurityTokenResponseLifeTime LifeTime
    {
        get
        {
            return this.lifeTimeField;
        }
        set
        {
            this.lifeTimeField = value;
        }
    }

    /// <remarks/>
    public RequestSecurityTokenResponseCollectionRequestSecurityTokenResponseRequestedSecurityToken RequestedSecurityToken
    {
        get
        {
            return this.requestedSecurityTokenField;
        }
        set
        {
            this.requestedSecurityTokenField = value;
        }
    }

    /// <remarks/>
    public RequestSecurityTokenResponseCollectionRequestSecurityTokenResponseRequestedTokenReference RequestedTokenReference
    {
        get
        {
            return this.requestedTokenReferenceField;
        }
        set
        {
            this.requestedTokenReferenceField = value;
        }
    }

    /// <remarks/>
    public RequestSecurityTokenResponseCollectionRequestSecurityTokenResponseRequestedProofToken RequestedProofToken
    {
        get
        {
            return this.requestedProofTokenField;
        }
        set
        {
            this.requestedProofTokenField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.xmlsoap.org/ws/2002/12/policy")]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://schemas.xmlsoap.org/ws/2002/12/policy", IsNullable = false)]
public partial class AppliesTo
{

    private EndpointReference endpointReferenceField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://schemas.xmlsoap.org/ws/2004/03/addressing")]
    public EndpointReference EndpointReference
    {
        get
        {
            return this.endpointReferenceField;
        }
        set
        {
            this.endpointReferenceField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.xmlsoap.org/ws/2004/03/addressing")]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://schemas.xmlsoap.org/ws/2004/03/addressing", IsNullable = false)]
public partial class EndpointReference
{

    private string addressField;

    /// <remarks/>
    public string Address
    {
        get
        {
            return this.addressField;
        }
        set
        {
            this.addressField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.xmlsoap.org/ws/2004/04/trust")]
public partial class RequestSecurityTokenResponseCollectionRequestSecurityTokenResponseLifeTime
{

    private System.DateTime createdField;

    private System.DateTime expiresField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xs" +
        "d")]
    public System.DateTime Created
    {
        get
        {
            return this.createdField;
        }
        set
        {
            this.createdField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xs" +
        "d")]
    public System.DateTime Expires
    {
        get
        {
            return this.expiresField;
        }
        set
        {
            this.expiresField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.xmlsoap.org/ws/2004/04/trust")]
public partial class RequestSecurityTokenResponseCollectionRequestSecurityTokenResponseRequestedSecurityToken
{

    private BinarySecurityToken binarySecurityTokenField;

    private EncryptedData encryptedDataField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://schemas.xmlsoap.org/ws/2003/06/secext")]
    public BinarySecurityToken BinarySecurityToken
    {
        get
        {
            return this.binarySecurityTokenField;
        }
        set
        {
            this.binarySecurityTokenField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.w3.org/2001/04/xmlenc#")]
    public EncryptedData EncryptedData
    {
        get
        {
            return this.encryptedDataField;
        }
        set
        {
            this.encryptedDataField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.xmlsoap.org/ws/2003/06/secext")]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://schemas.xmlsoap.org/ws/2003/06/secext", IsNullable = false)]
public partial class BinarySecurityToken
{

    private string idField;

    private string valueField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
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
    [System.Xml.Serialization.XmlTextAttribute()]
    public string Value
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
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.w3.org/2001/04/xmlenc#")]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.w3.org/2001/04/xmlenc#", IsNullable = false)]
public partial class EncryptedData
{

    private EncryptedDataEncryptionMethod encryptionMethodField;

    private KeyInfo keyInfoField;

    private EncryptedDataCipherData cipherDataField;

    private string idField;

    private string typeField;

    /// <remarks/>
    public EncryptedDataEncryptionMethod EncryptionMethod
    {
        get
        {
            return this.encryptionMethodField;
        }
        set
        {
            this.encryptionMethodField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    public KeyInfo KeyInfo
    {
        get
        {
            return this.keyInfoField;
        }
        set
        {
            this.keyInfoField = value;
        }
    }

    /// <remarks/>
    public EncryptedDataCipherData CipherData
    {
        get
        {
            return this.cipherDataField;
        }
        set
        {
            this.cipherDataField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
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
    [System.Xml.Serialization.XmlAttributeAttribute()]
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
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.w3.org/2001/04/xmlenc#")]
public partial class EncryptedDataEncryptionMethod
{

    private string algorithmField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Algorithm
    {
        get
        {
            return this.algorithmField;
        }
        set
        {
            this.algorithmField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.w3.org/2000/09/xmldsig#")]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.w3.org/2000/09/xmldsig#", IsNullable = false)]
public partial class KeyInfo
{

    private string keyNameField;

    /// <remarks/>
    public string KeyName
    {
        get
        {
            return this.keyNameField;
        }
        set
        {
            this.keyNameField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.w3.org/2001/04/xmlenc#")]
public partial class EncryptedDataCipherData
{

    private string cipherValueField;

    /// <remarks/>
    public string CipherValue
    {
        get
        {
            return this.cipherValueField;
        }
        set
        {
            this.cipherValueField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.xmlsoap.org/ws/2004/04/trust")]
public partial class RequestSecurityTokenResponseCollectionRequestSecurityTokenResponseRequestedTokenReference
{

    private KeyIdentifier keyIdentifierField;

    private Reference referenceField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://schemas.xmlsoap.org/ws/2003/06/secext")]
    public KeyIdentifier KeyIdentifier
    {
        get
        {
            return this.keyIdentifierField;
        }
        set
        {
            this.keyIdentifierField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://schemas.xmlsoap.org/ws/2003/06/secext")]
    public Reference Reference
    {
        get
        {
            return this.referenceField;
        }
        set
        {
            this.referenceField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.xmlsoap.org/ws/2003/06/secext")]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://schemas.xmlsoap.org/ws/2003/06/secext", IsNullable = false)]
public partial class KeyIdentifier
{

    private string valueTypeField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string ValueType
    {
        get
        {
            return this.valueTypeField;
        }
        set
        {
            this.valueTypeField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.xmlsoap.org/ws/2003/06/secext")]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://schemas.xmlsoap.org/ws/2003/06/secext", IsNullable = false)]
public partial class Reference
{

    private string uRIField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string URI
    {
        get
        {
            return this.uRIField;
        }
        set
        {
            this.uRIField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.xmlsoap.org/ws/2004/04/trust")]
public partial class RequestSecurityTokenResponseCollectionRequestSecurityTokenResponseRequestedProofToken
{

    private string binarySecretField;

    /// <remarks/>
    public string BinarySecret
    {
        get
        {
            return this.binarySecretField;
        }
        set
        {
            this.binarySecretField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.xmlsoap.org/ws/2004/04/trust")]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://schemas.xmlsoap.org/ws/2004/04/trust", IsNullable = false)]
public partial class RequestSecurityTokenResponseCollection
{

    private RequestSecurityTokenResponseCollectionRequestSecurityTokenResponse[] requestSecurityTokenResponseField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("RequestSecurityTokenResponse")]
    public RequestSecurityTokenResponseCollectionRequestSecurityTokenResponse[] RequestSecurityTokenResponse
    {
        get
        {
            return this.requestSecurityTokenResponseField;
        }
        set
        {
            this.requestSecurityTokenResponseField = value;
        }
    }
}

