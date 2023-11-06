using AvaMSN.MSNP.SOAP.SerializableClasses.DeleteMember;

namespace AvaMSN.MSNP.SOAP.RequestObjects;

public static partial class RequestObjects
{
    public static Envelope DeleteMember()
    {
        return new Envelope
        {
            Header = new EnvelopeHeader
            {
                ABApplicationHeader = new ABApplicationHeader
                {
                    ApplicationId = "CFE80F9D-180F-4399-82AB-413F33A1FA11",
                    PartnerScenario = ""
                },
                ABAuthHeader = new ABAuthHeader
                {
                    TicketToken = new object()
                }
            },
            Body = new EnvelopeBody
            {
                DeleteMember = new DeleteMember
                {
                    serviceHandle = new DeleteMemberServiceHandle
                    {
                        Id = 0,
                        Type = "Messenger",
                        ForeignId = new object()
                    },
                    memberships = new DeleteMemberMemberships
                    {
                        Membership = new DeleteMemberMembershipsMembership
                        {
                            MemberRole = "",
                            Members = new DeleteMemberMembershipsMembershipMembers
                            {
                                Member = new PassportMember
                                {
                                    Type = "Passport",
                                    State = "Accepted",
                                    PassportName = ""
                                }
                            }
                        }
                    }
                }
            }
        };
    }
}
