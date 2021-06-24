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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using GreenEnergyHub.Messaging.Transport;
using GreenEnergyHub.TimeSeries.Domain.Common;
using GreenEnergyHub.TimeSeries.Domain.Notification;
using NodaTime;

namespace GreenEnergyHub.TimeSeries.Infrastructure.Messaging.Serialization.Commands
{
    public class TimeSeriesCommandDeserializer : MessageDeserializer
    {
        private readonly ICorrelationContext _correlationContext;
        private readonly TimeSeriesCommandConverter _timeSeriesCommandConverter;

        public TimeSeriesCommandDeserializer(
            ICorrelationContext correlationContext,
            TimeSeriesCommandConverter timeSeriesCommandConverter)
        {
            _correlationContext = correlationContext;
            _timeSeriesCommandConverter = timeSeriesCommandConverter;
        }

        public override async Task<IInboundMessage> FromBytesAsync(byte[] data, CancellationToken cancellationToken = default)
        {
            await using var stream = new MemoryStream(data);

            using var reader = XmlReader.Create(stream, new XmlReaderSettings { Async = true });

            var command = await _timeSeriesCommandConverter.ConvertAsync(reader).ConfigureAwait(false);

            return command;
        }

        private TimeSeriesCommand GetTimeSeriesCommandExample()
        {
            var correlationId = _correlationContext.CorrelationId;
            var command = new TimeSeriesCommand(correlationId)
            {
                Document = new Document()
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = DocumentType.NotifyValidatedMeasureData,
                    Sender = new MarketParticipant()
                    {
                        Id = "8100000000030",
                        BusinessProcessRole = MarketParticipantRole.MeteredDataResponsible,
                    },
                    Recipient = new MarketParticipant()
                    {
                        Id = "5790001330552",
                        BusinessProcessRole = MarketParticipantRole.SystemOperator,
                    },
                    CreatedDateTime = SystemClock.Instance.GetCurrentInstant(),
                    BusinessReasonCode = BusinessReasonCode.PeriodicMetering,
                    RequestDateTime = Instant.FromUtc(2020, 12, 31, 23, 00),
                },
                Series = new Series()
                {
                    Id = Guid.NewGuid().ToString(),
                    MeteringPointId = "578032999778756222",
                    MeteringPointType = MeteringPointType.Consumption,
                    SettlementMethod = SettlementMethod.NonProfiled,
                    Product = Product.EnergyActive,
                    Unit = MeasureUnit.KiloWattHour,
                    RegistrationDateTime = SystemClock.Instance.GetCurrentInstant(),
                    Resolution = TimeSeriesResolution.Hour,
                    StartDateTime = Instant.FromUtc(2020, 12, 31, 23, 00),
                    EndDateTime = Instant.FromUtc(2021, 1, 1, 23, 00),
                },
            };

            for (int i = 1; i < 25; i++)
            {
                var time = Instant.FromUtc(2020, 12, 31, 23, 00).Plus(Duration.FromHours(i - 1));
                var point = new Point()
                {
                    Position = i,
                    Quantity = i,
                    Quality = QuantityQuality.Measured,
                    ObservationTime = time,
                };
                command.Series.Points.Add(point);
            }

            return command;
        }
    }
}
