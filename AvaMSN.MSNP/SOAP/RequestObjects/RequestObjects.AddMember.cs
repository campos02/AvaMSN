using AvaMSN.MSNP.SOAP.SerializableClasses.AddMember;

namespace AvaMSN.MSNP.SOAP.RequestObjects;

public static partial class RequestObjects
{
    public static Envelope AddMember()
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
                AddMember = new AddMember
                {
                    serviceHandle = new AddMemberServiceHandle
                    {
                        Id = 0,
                        Type = "Messenger",
                        ForeignId = new object()
                    },
                    memberships = new AddMemberMemberships
                    {
                        Membership = new AddMemberMembershipsMembership
                        {
                            MemberRole = "",
                            Members = new AddMemberMembershipsMembershipMembers
                            {
                                Member = new AddMemberMembershipsMembershipMembersMember
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
