using AvaMSN.MSNP.SOAP.SerializableClasses.RST;

namespace AvaMSN.MSNP.SOAP.RequestObjects;

public static partial class RequestObjects
{
    public static Envelope RSTRequest()
    {
        return new Envelope
        {
            Header = new EnvelopeHeader
            {
                AuthInfo = new AuthInfo
                {
                    HostingApp = "{7108E71A-9926-4FCB-BCC9-9A9D3F32E423}",
                    BinaryVersion = 4,
                    UIVersion = 1,
                    Cookies = new object(),
                    RequestParams = "AQAAAAIAAABsYwQAAAAxMDMz",
                    Id = "PPAuthInfo"
                },
                Security = new Security
                {
                    UsernameToken = new SecurityUsernameToken
                    {
                        Username = new object(),
                        Password = new object(),
                        Id = "user"
                    }
                }
            },
            Body = new EnvelopeBody
            {
                RequestMultipleSecurityTokens = new RequestMultipleSecurityTokens
                {
                    RequestSecurityToken = new RequestSecurityToken[]
            {
                new RequestSecurityToken
                {
                    RequestType = "http://schemas.xmlsoap.org/ws/2004/04/security/trust/Issue",
                    AppliesTo = new AppliesTo
                    {
                        EndpointReference = new EndpointReference
                        {
                            Address = "http://Passport.NET/tb"
                        }
                    },
                    Id = "RST0"
                },
                new RequestSecurityToken
                {
                    RequestType = "http://schemas.xmlsoap.org/ws/2004/04/security/trust/Issue",
                    AppliesTo = new AppliesTo
                    {
                        EndpointReference = new EndpointReference
                        {
                            Address = "messengerclear.live.com"
                        }
                    },
                    PolicyReference = new PolicyReference
                    {
                        URI = "policy parameter"
                    },
                    Id = "RST1"
                },
                new RequestSecurityToken
                {
                    RequestType = "http://schemas.xmlsoap.org/ws/2004/04/security/trust/Issue",
                    AppliesTo = new AppliesTo
                    {
                        EndpointReference = new EndpointReference
                        {
                            Address = "contacts.msn.com"
                        }
                    },
                    PolicyReference = new PolicyReference
                    {
                        URI = "MBI_KEY_OLD"
                    },
                    Id = "RST2"
                }
            },
                    Id = "RSTS"
                }
            }
        };

    }
}
