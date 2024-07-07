using AvaMSN.MSNP.SOAP.SerializableClasses.ABContactDelete;

namespace AvaMSN.MSNP.SOAP.RequestObjects;

internal static partial class RequestObjects
{
    public static Envelope ABContactDelete()
    {
        return new Envelope
        {
            Header = new EnvelopeHeader
            {
                ABApplicationHeader = new ABApplicationHeader
                {
                    ApplicationId = "996CDE1B-AA53-4477-B943-2BB802EA6166",
                    PartnerScenario = "Timer",
                    CacheKey = ""
                },
                ABAuthHeader = new ABAuthHeader
                {
                    TicketToken = ""
                }
            },
            Body = new EnvelopeBody
            {
                ABContactDelete = new ABContactDelete
                {
                    abId = "00000000-0000-0000-0000-000000000000",
                    contacts = new ABContactDeleteContacts
                    {
                        Contact = new ABContactDeleteContactsContact
                        {
                            contactId = ""
                        }
                    }
                }
            }
        };
    }
}
