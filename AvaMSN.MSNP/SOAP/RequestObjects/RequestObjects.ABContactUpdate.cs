using AvaMSN.MSNP.SOAP.SerializableClasses.ABContactUpdate;

namespace AvaMSN.MSNP.SOAP.RequestObjects;

internal static partial class RequestObjects
{
    public static Envelope AbContactUpdate()
    {
        return new Envelope
        {
            Header = new EnvelopeHeader
            {
                ABApplicationHeader = new ABApplicationHeader
                {
                    ApplicationId = "CFE80F9D-180F-4399-82AB-413F33A1FA11",
                    PartnerScenario = "Timer"
                },
                ABAuthHeader = new ABAuthHeader
                {
                    TicketToken = new object()
                }
            },
            Body = new EnvelopeBody
            {
                ABContactUpdate = new ABContactUpdate
                {
                    abId = "00000000-0000-0000-0000-000000000000",
                    contacts = new ABContactUpdateContacts
                    {
                        Contact = new ABContactUpdateContactsContact
                        {
                            contactInfo = new ABContactUpdateContactsContactContactInfo
                            {
                                contactType = "",
                                displayName = ""
                            },
                            propertiesChanged = "DisplayName"
                        }
                    }
                }
            }
        };
    }
}
