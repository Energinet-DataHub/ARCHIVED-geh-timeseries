using Energinet.DataHub.TimeSeries.Application.Enums;
using NodaTime;

namespace Energinet.DataHub.TimeSeries.Application.Dtos
{
    public class Point
    {
        public Resolution Resolution { get; set; }

        public decimal Quantity { get; set; }

        public Quality Quality { get; set; }

        public Instant MeasureDateTime { get; set; }
    }
}
