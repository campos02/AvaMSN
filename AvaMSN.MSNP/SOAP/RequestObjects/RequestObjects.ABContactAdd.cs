using AvaMSN.MSNP.SOAP.SerializableClasses.ABContactAdd;

namespace AvaMSN.MSNP.SOAP.RequestObjects;

public static partial class RequestObjects
{
    public static Envelope ABContactAdd()
    {
        return new Envelope
        {
            Header = new EnvelopeHeader
            {
                ABApplicationHeader = new ABApplicationHeader
                {
                    ApplicationId = "996CDE1B-AA53-4477-B943-2BB802EA6166",
                    PartnerScenario = "ContactSave",
                    CacheKey = ""
                },
                ABAuthHeader = new ABAuthHeader
                {
                    TicketToken = ""
                }
            },
            Body = new EnvelopeBody
            {
                ABContactAdd = new ABContactAdd
                {
                    abId = "00000000-0000-0000-0000-000000000000",
                    contacts = new ABContactAddContacts
                    {
                        Contact = new ABContactAddContactsContact
                        {
                            contactInfo = new ABContactAddContactsContactContactInfo
                            {
                                contactType = "LivePending",
                                passportName = "",
                                isMessengerUser = true
                            }
                        }
                    },
                    options = new ABContactAddOptions
                    {
                        EnableAllowListManagement = true
                    }
                }
            }
        };

    }
}
