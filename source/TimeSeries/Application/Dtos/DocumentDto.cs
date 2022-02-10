using Energinet.DataHub.TimeSeries.Application.Enums;
using NodaTime;

namespace Energinet.DataHub.TimeSeries.Application.Dtos
{
    public class DocumentDto
    {
        public string Id { get; set; }

        public Instant CreatedDateTime { get; set; }

        public MarketParticipantDto Sender { get; set; }

        public MarketParticipantDto Receiver { get; set; }

        /// <summary>
        /// Process type in CIM XML
        /// </summary>
        public BusinessReasonCode BusinessReasonCode { get; set; }
    }
}
