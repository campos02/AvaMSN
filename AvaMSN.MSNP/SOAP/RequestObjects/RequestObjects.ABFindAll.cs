using AvaMSN.MSNP.SOAP.SerializableClasses.ABFindAll;
using System.Globalization;

namespace AvaMSN.MSNP.SOAP.RequestObjects;

public static partial class RequestObjects
{
    public static Envelope ABFindAll()
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
                ABFindAll = new ABFindAll
                {
                    abId = "00000000-0000-0000-0000-000000000000",
                    abView = "Full",
                    lastChange = DateTime.ParseExact("0001-01-01T06:00:00.0000000-02:00", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)
                }
            }
        };
    }
}
