using System;
using System.Globalization;
using System.Linq;
using Energinet.DataHub.TimeSeries.InternalContracts;
using GreenEnergyHub.Messaging.Protobuf;
using GreenEnergyHub.TimeSeries.Domain.Notification;
using Point = Energinet.DataHub.TimeSeries.InternalContracts.Point;

namespace GreenEnergyHub.TimeSeries.Infrastructure.Internal.Mappers
{
    public class TimeSeriesCommandOutboundMapper : ProtobufOutboundMapper<TimeSeriesCommand>
    {
        protected override Google.Protobuf.IMessage Convert(TimeSeriesCommand obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            var document = obj.Document;
            var series = obj.Series;

            return new TimeSeriesCommandDomain
            {
                Document = new DocumentDomain
                {
                    Id = document.Id,
                    RequestDateTime = document.RequestDateTime.ToString(),
                    Type = document.Type.ToString(),
                    CreatedDateTime = document.CreatedDateTime.ToString(),
                    Sender = new MarketParticipantDomain
                    {
                        Id = document.Sender.Id,
                        MarketParticipantRole = document.Sender.BusinessProcessRole.ToString(),
                    },
                    Recipient = new MarketParticipantDomain
                    {
                        Id = document.Recipient.Id,
                        MarketParticipantRole = document.Recipient.BusinessProcessRole.ToString(),
                    },
                    BusinessReasonCode = document.BusinessReasonCode.ToString(),
                },
                Series = new SeriesDomain
                {
                    Id = obj.Series.Id,
                    MeteringPointId = series.MeteringPointId,
                    MeteringPointType = series.MeteringPointType.ToString(),
                    SettlementMethod = series.SettlementMethod.ToString(),
                    RegistrationDateTime = series.StartDateTime.ToString(),
                    Product = series.Product.ToString(),
                    MeasureUnit = series.Unit.ToString(),
                    TimeSeriesResolution = series.Resolution.ToString(),
                    StartDateTime = series.StartDateTime.ToString(),
                    EndDateTime = series.EndDateTime.ToString(),
                    Points =
                    {
                        obj.Series.Points.Select(p => new Point
                        {
                            Position = p.Position.ToString(CultureInfo.InvariantCulture),
                            Quality = p.Quality.ToString(),
                            Quantity = p.Quantity.ToString(CultureInfo.InvariantCulture),
                            ObservationDateTime = p.ObservationDateTime.ToString(),
                        }),
                    },
                },
                CorrelationId = obj.CorrelationId,
            };
        }
    }
}
