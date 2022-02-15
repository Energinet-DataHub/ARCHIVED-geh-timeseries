using Energinet.DataHub.TimeSeries.Application.Enums;
using NodaTime;

namespace Energinet.DataHub.TimeSeries.Application.Dtos
{
    public class PointDto
    {
        public PointDto()
        {
            Quality = Quality.AsProvided;
        }

        public decimal? Quantity { get; set; }

        public Quality Quality { get; set; }

        public int Position { get; set; }
    }
}
