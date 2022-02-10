using System.Collections.Generic;
using Energinet.DataHub.TimeSeries.Application.Enums;
using NodaTime;

namespace Energinet.DataHub.TimeSeries.Application.Dtos
{
    public class SeriesDto
    {
        public string Id { get; set; }

        public string TransactionId { get; set; }

        public string MeteringPointId { get; set; }

        public MeteringPointType MeteringPointType { get; set; }

        public Instant RegistrationDateTime { get; set; }

        public string Product { get; set; }

        public MeasureUnit MeasureUnit { get; set; }

        public PeriodDto PeriodDto { get; set; }
    }
}
