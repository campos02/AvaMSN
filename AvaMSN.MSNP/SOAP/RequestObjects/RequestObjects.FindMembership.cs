using AvaMSN.MSNP.SOAP.SerializableClasses.FindMembership;

namespace AvaMSN.MSNP.SOAP.RequestObjects;

internal static partial class RequestObjects
{
    public static Envelope FindMembership()
    {
        return new Envelope
        {
            Header = new EnvelopeHeader
            {
                ABApplicationHeader = new ABApplicationHeader
                {
                    ApplicationId = "CFE80F9D-180F-4399-82AB-413F33A1FA11",
                    PartnerScenario = "Initial"
                },
                ABAuthHeader = new ABAuthHeader
                {
                    TicketToken = new object()
                }
            },
            Body = new EnvelopeBody
            {
                FindMembership = new FindMembership
                {
                    serviceFilter = new FindMembershipServiceFilter
                    {
                        Types = new string[]
                {
                    "Messenger",
                    "Space",
                    "Profile"
                }
                    }
                }
            }
        };
    }
}
