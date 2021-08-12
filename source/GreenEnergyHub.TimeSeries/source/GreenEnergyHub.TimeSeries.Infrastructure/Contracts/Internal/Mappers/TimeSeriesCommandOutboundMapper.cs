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
using GreenEnergyHub.TimeSeries.Contracts.Internal;
using GreenEnergyHub.TimeSeries.Core;
using GreenEnergyHub.TimeSeries.Core.DateTime;
using GreenEnergyHub.TimeSeries.Domain.Notification;

namespace GreenEnergyHub.TimeSeries.Infrastructure.Contracts.Internal.Mappers
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

            return new TimeSeriesCommandContract
            {
                Document = new DocumentContract
                {
                    Id = document.Id,
                    RequestDateTime = document.RequestDateTime.ToTimestamp().TruncateToSeconds(),
                    CreatedDateTime = document.CreatedDateTime.ToTimestamp().TruncateToSeconds(),
                    Sender = new MarketParticipantContract
                    {
                        Id = document.Sender.Id,
                        BusinessProcesRole = document.Sender.BusinessProcessRole.Cast<BusinessProcessRoleContract>(),
                    },
                    BusinessReasonCode = document.BusinessReasonCode.Cast<BusinessReasonCodeContract>(),
                },
                Series = new SeriesContract
                {
                    Id = obj.Series.Id,
                    MeteringPointId = series.MeteringPointId,
                    MeteringPointType = series.MeteringPointType.Cast<MeteringPointTypeContract>(),

                    SettlementMethod = series.SettlementMethod?.Cast<SettlementMethodContract>() ?? SettlementMethodContract.SmcNull,
                    RegistrationDateTime = series.StartDateTime.ToTimestamp().TruncateToSeconds(),
                    Product = series.Product.Cast<ProductContract>(),
                    MeasureUnit = series.Unit.Cast<MeasureUnitContract>(),
                    Resolution = series.Resolution.Cast<ResolutionContract>(),
                    StartDateTime = series.StartDateTime.ToTimestamp().TruncateToSeconds(),
                    EndDateTime = series.EndDateTime.ToTimestamp().TruncateToSeconds(),
                    Points =
                    {
                        obj.Series.Points.Select(p => new PointContract
                        {
                            Position = p.Position,
                            Quality = p.Quality.Cast<QualityContract>(),

                            Quantity = p.Quantity,
                            ObservationDateTime = p.ObservationDateTime.ToTimestamp().TruncateToSeconds(),
                        }),
                    },
                },
                CorrelationId = obj.CorrelationId,
            };
        }
    }
}
