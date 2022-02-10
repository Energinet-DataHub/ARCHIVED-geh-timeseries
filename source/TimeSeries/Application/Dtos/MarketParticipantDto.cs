using Energinet.DataHub.TimeSeries.Application.Enums;

namespace Energinet.DataHub.TimeSeries.Application.Dtos
{
    public class MarketParticipantDto
    {
        public string Id { get; set; }

        public MarketParticipantRole BusinessProcessRole { get; set; }
    }
}
