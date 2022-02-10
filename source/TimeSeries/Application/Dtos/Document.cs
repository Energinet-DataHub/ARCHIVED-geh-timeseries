using Energinet.DataHub.TimeSeries.Application.Enums;
using NodaTime;

namespace Energinet.DataHub.TimeSeries.Application.Dtos
{
    public class Document
    {
        public string Id { get; set; }

        public Instant CreatedDateTime { get; set; }

        public MarketParticipant Sender { get; set; }

        public MarketParticipant Receiver { get; set; }

        /// <summary>
        /// Process type in CIM XML
        /// </summary>
        public BusinessReasonCode BusinessReasonCode { get; set; }
    }
}
