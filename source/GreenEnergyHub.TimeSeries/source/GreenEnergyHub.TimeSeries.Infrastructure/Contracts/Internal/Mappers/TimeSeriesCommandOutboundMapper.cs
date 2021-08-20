// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Linq;
using GreenEnergyHub.Messaging.Protobuf;
using GreenEnergyHub.TimeSeries.Core.DateTime;
using GreenEnergyHub.TimeSeries.Core.Enumeration;
using GreenEnergyHub.TimeSeries.Domain.MarketDocument;
using domain = GreenEnergyHub.TimeSeries.Domain.Notification;
using proto = GreenEnergyHub.TimeSeries.Contracts.Internal;

namespace GreenEnergyHub.TimeSeries.Infrastructure.Contracts.Internal.Mappers
{
    public class TimeSeriesCommandOutboundMapper : ProtobufOutboundMapper<domain.TimeSeriesCommand>
    {
        protected override Google.Protobuf.IMessage Convert(domain.TimeSeriesCommand obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            return ConvertTimeSeriesCommand(obj);
        }

        private static proto.TimeSeriesCommand ConvertTimeSeriesCommand(domain.TimeSeriesCommand obj)
        {
            return new proto.TimeSeriesCommand
            {
                Document = ConvertDocument(obj.Document),
                Series = ConvertSeries(obj.Series),
                CorrelationId = obj.CorrelationId,
            };
        }

        private static proto.Document ConvertDocument(Document document)
        {
            return new proto.Document
            {
                Id = document.Id,
                RequestDateTime = document.RequestDateTime.ToTimestamp().TruncateToSeconds(),
                CreatedDateTime = document.CreatedDateTime.ToTimestamp().TruncateToSeconds(),
                Sender = ConvertSender(document.Sender),
                BusinessReasonCode = document.BusinessReasonCode.Cast<proto.BusinessReasonCode>(),
            };
        }

        private static proto.MarketParticipant ConvertSender(MarketParticipant sender)
        {
            return new proto.MarketParticipant
            {
                Id = sender.Id,
                BusinessProcessRole = sender.BusinessProcessRole.Cast<proto.BusinessProcessRole>(),
            };
        }

        private static proto.Series ConvertSeries(domain.Series series)
        {
            return new proto.Series
            {
                Id = series.Id,
                MeteringPointId = series.MeteringPointId,
                MeteringPointType = series.MeteringPointType.Cast<proto.MeteringPointType>(),

                SettlementMethod = series.SettlementMethod?.Cast<proto.SettlementMethod>() ?? proto.SettlementMethod.SmNotSet,
                RegistrationDateTime = series.RegistrationDateTime.ToTimestamp().TruncateToSeconds(),
                Product = series.Product.Cast<proto.Product>(),
                Unit = series.Unit.Cast<proto.MeasureUnit>(),
                Resolution = series.Resolution.Cast<proto.Resolution>(),
                StartDateTime = series.StartDateTime.ToTimestamp().TruncateToSeconds(),
                EndDateTime = series.EndDateTime.ToTimestamp().TruncateToSeconds(),
                Points =
                {
                    series.Points.Select(ConvertPoint),
                },
            };
        }

        private static proto.Point ConvertPoint(domain.Point p)
        {
            return new proto.Point
            {
                Position = p.Position,
                Quality = p.Quality.Cast<proto.Quality>(),

                Quantity = p.Quantity,
                ObservationDateTime = p.ObservationDateTime.ToTimestamp().TruncateToSeconds(),
            };
        }
    }
}
