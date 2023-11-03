namespace AvaMSN.MSNP.SOAP.SerializableClasses.RST;

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

    private AuthInfo authInfoField;

    private Security securityField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://schemas.microsoft.com/Passport/SoapServices/PPCRL")]
    public AuthInfo AuthInfo
    {
        get
        {
            return this.authInfoField;
        }
        set
        {
            this.authInfoField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://schemas.xmlsoap.org/ws/2003/06/secext")]
    public Security Security
    {
        get
        {
            return this.securityField;
        }
        set
        {
            this.securityField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/Passport/SoapServices/PPCRL")]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://schemas.microsoft.com/Passport/SoapServices/PPCRL", IsNullable = false)]
public partial class AuthInfo
{

    private string hostingAppField;

    private byte binaryVersionField;

    private byte uIVersionField;

    private object cookiesField;

    private string requestParamsField;

    private string idField;

    /// <remarks/>
    public string HostingApp
    {
        get
        {
            return this.hostingAppField;
        }
        set
        {
            this.hostingAppField = value;
        }
    }

    /// <remarks/>
    public byte BinaryVersion
    {
        get
        {
            return this.binaryVersionField;
        }
        set
        {
            this.binaryVersionField = value;
        }
    }

    /// <remarks/>
    public byte UIVersion
    {
        get
        {
            return this.uIVersionField;
        }
        set
        {
            this.uIVersionField = value;
        }
    }

    /// <remarks/>
    public object Cookies
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
    public string RequestParams
    {
        get
        {
            return this.requestParamsField;
        }
        set
        {
            this.requestParamsField = value;
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
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.xmlsoap.org/ws/2003/06/secext")]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://schemas.xmlsoap.org/ws/2003/06/secext", IsNullable = false)]
public partial class Security
{

    private SecurityUsernameToken usernameTokenField;

    /// <remarks/>
    public SecurityUsernameToken UsernameToken
    {
        get
        {
            return this.usernameTokenField;
        }
        set
        {
            this.usernameTokenField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.xmlsoap.org/ws/2003/06/secext")]
public partial class SecurityUsernameToken
{

    private object usernameField;

    private object passwordField;

    private string idField;

    /// <remarks/>
    public object Username
    {
        get
        {
            return this.usernameField;
        }
        set
        {
            this.usernameField = value;
        }
    }

    /// <remarks/>
    public object Password
    {
        get
        {
            return this.passwordField;
        }
        set
        {
            this.passwordField = value;
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
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
public partial class EnvelopeBody
{

    private RequestMultipleSecurityTokens requestMultipleSecurityTokensField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://schemas.microsoft.com/Passport/SoapServices/PPCRL")]
    public RequestMultipleSecurityTokens RequestMultipleSecurityTokens
    {
        get
        {
            return this.requestMultipleSecurityTokensField;
        }
        set
        {
            this.requestMultipleSecurityTokensField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/Passport/SoapServices/PPCRL")]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://schemas.microsoft.com/Passport/SoapServices/PPCRL", IsNullable = false)]
public partial class RequestMultipleSecurityTokens
{

    private RequestSecurityToken[] requestSecurityTokenField;

    private string idField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("RequestSecurityToken", Namespace = "http://schemas.xmlsoap.org/ws/2004/04/trust")]
    public RequestSecurityToken[] RequestSecurityToken
    {
        get
        {
            return this.requestSecurityTokenField;
        }
        set
        {
            this.requestSecurityTokenField = value;
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
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.xmlsoap.org/ws/2004/04/trust")]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://schemas.xmlsoap.org/ws/2004/04/trust", IsNullable = false)]
public partial class RequestSecurityToken
{

    private string requestTypeField;

    private AppliesTo appliesToField;

    private PolicyReference policyReferenceField;

    private string idField;

    /// <remarks/>
    public string RequestType
    {
        get
        {
            return this.requestTypeField;
        }
        set
        {
            this.requestTypeField = value;
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
    [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://schemas.xmlsoap.org/ws/2003/06/secext")]
    public PolicyReference PolicyReference
    {
        get
        {
            return this.policyReferenceField;
        }
        set
        {
            this.policyReferenceField = value;
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
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.xmlsoap.org/ws/2003/06/secext")]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://schemas.xmlsoap.org/ws/2003/06/secext", IsNullable = false)]
public partial class PolicyReference
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

