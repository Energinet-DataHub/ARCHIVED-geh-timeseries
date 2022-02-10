using System.Collections.Generic;
using Energinet.DataHub.TimeSeries.Application.Enums;
using NodaTime;

namespace Energinet.DataHub.TimeSeries.Application.Dtos
{
    public class PeriodDto
    {
        public Resolution Resolution { get; set; }

        public Instant StartDateTime { get; set; }

        public Instant EndDateTime { get; set; }

        public IEnumerable<PointDto> Points { get; set; }
    }
}
