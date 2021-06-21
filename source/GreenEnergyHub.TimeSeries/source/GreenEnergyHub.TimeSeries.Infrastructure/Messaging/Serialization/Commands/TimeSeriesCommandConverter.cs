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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading.Tasks;
using System.Xml;
using GreenEnergyHub.Iso8601;
using GreenEnergyHub.Messaging.Transport;
using GreenEnergyHub.TimeSeries.Domain.Common;
using GreenEnergyHub.TimeSeries.Domain.Notification;
using GreenEnergyHub.TimeSeries.Infrastructure.Messaging.Serialization.Common;
using NodaTime;

namespace GreenEnergyHub.TimeSeries.Infrastructure.Messaging.Serialization.Commands
{
    public class TimeSeriesCommandConverter : DocumentConverter
    {
        private readonly ICorrelationContext _correlationContext;
        private readonly IIso8601Durations _iso8601Durations;

        public TimeSeriesCommandConverter(
            ICorrelationContext correlationContext,
            IIso8601Durations iso8601Durations)
        {
            _correlationContext = correlationContext;
            _iso8601Durations = iso8601Durations;
        }

        protected override async Task<IInboundMessage> ConvertSpecializedContentAsync(
            [NotNull]XmlReader reader,
            Document document)
        {
            var correlationId = _correlationContext.CorrelationId;

            var command = new TimeSeriesCommand(correlationId)
            {
                Document = document,
            };

            command.Series = await ParseSeriesAsync(reader).ConfigureAwait(false);

            return command;
        }

        private async Task<Series> ParseSeriesAsync(XmlReader reader)
        {
            var series = new Series();

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                if (reader.Is(TimeSeriesCommandConstants.Id, TimeSeriesCommandConstants.Namespace))
                {
                    var content = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                    series.Id = content;
                }
                else if (reader.Is(TimeSeriesCommandConstants.Product, TimeSeriesCommandConstants.Namespace))
                {
                    var content = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                    series.Product = ProductMapper.Map(content);
                }
                else if (reader.Is(TimeSeriesCommandConstants.MeteringPointId, TimeSeriesCommandConstants.Namespace))
                {
                    var content = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                    series.MeteringPointId = content;
                }
                else if (reader.Is(TimeSeriesCommandConstants.MeteringPointType, TimeSeriesCommandConstants.Namespace))
                {
                    var content = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                    series.MeteringPointType = MeteringPointTypeMapper.Map(content);
                }
                else if (reader.Is(TimeSeriesCommandConstants.SettlementMethod, TimeSeriesCommandConstants.Namespace))
                {
                    var content = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                    series.SettlementMethod = SettlementMethodMapper.Map(content);
                }
                else if (reader.Is(TimeSeriesCommandConstants.RegistrationDateTime, TimeSeriesCommandConstants.Namespace))
                {
                    series.RegistrationDateTime = Instant.FromDateTimeUtc(reader.ReadElementContentAsDateTime());
                }
                else if (reader.Is(TimeSeriesCommandConstants.Unit, TimeSeriesCommandConstants.Namespace))
                {
                    var content = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                    series.Unit = MeasureUnitMapper.Map(content);
                }
                else if (reader.Is(TimeSeriesCommandConstants.Period, TimeSeriesCommandConstants.Namespace))
                {
                    await ParsePeriodAsync(reader, series).ConfigureAwait(false);
                }
            }

            return series;
        }

        private async Task ParsePeriodAsync(XmlReader reader, Series series)
        {
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                if (reader.Is(TimeSeriesCommandConstants.Resolution, TimeSeriesCommandConstants.Namespace))
                {
                    var content = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                    series.Resolution = TimeSeriesResolutionMapper.Map(content);
                }
                else if (reader.Is(TimeSeriesCommandConstants.TimeInterval, TimeSeriesCommandConstants.Namespace))
                {
                    await ParseTimeIntervalAsync(reader, series).ConfigureAwait(false);
                }
                else if (reader.Is(TimeSeriesCommandConstants.Point, TimeSeriesCommandConstants.Namespace))
                {
                    var point = await ParsePointAsync(reader, series).ConfigureAwait(false);
                    series.Points.Add(point);
                }
                else if (reader.Is(TimeSeriesCommandConstants.Period, TimeSeriesCommandConstants.Namespace, XmlNodeType.EndElement))
                {
                    break;
                }
            }
        }

#pragma warning disable CA1822 // Mark members as static
        private async Task ParseTimeIntervalAsync(XmlReader reader, Series series)
#pragma warning restore CA1822 // Mark members as static
        {
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                if (reader.Is(TimeSeriesCommandConstants.StartDateTime, TimeSeriesCommandConstants.Namespace))
                {
                    series.StartDateTime = Instant.FromDateTimeUtc(reader.ReadElementContentAsDateTime());
                }
                else if (reader.Is(TimeSeriesCommandConstants.EndDateTime, TimeSeriesCommandConstants.Namespace))
                {
                    series.EndDateTime = Instant.FromDateTimeUtc(reader.ReadElementContentAsDateTime());
                }
                else if (reader.Is(TimeSeriesCommandConstants.TimeInterval, TimeSeriesCommandConstants.Namespace, XmlNodeType.EndElement))
                {
                    break;
                }
            }
        }

        private async Task<Point> ParsePointAsync(XmlReader reader, Series series)
        {
            var point = new Point();

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                if (reader.Is(TimeSeriesCommandConstants.Position, TimeSeriesCommandConstants.Namespace))
                {
                    var content = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                    point.Position = int.Parse(content, CultureInfo.InvariantCulture);
                }
                else if (reader.Is(TimeSeriesCommandConstants.Quantity, TimeSeriesCommandConstants.Namespace))
                {
                    var content = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                    point.Quantity = decimal.Parse(content, CultureInfo.InvariantCulture);
                }
                else if (reader.Is(TimeSeriesCommandConstants.Quality, TimeSeriesCommandConstants.Namespace))
                {
                    var content = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                    point.Quality = QuantityQualityMapper.Map(content);
                }
                else if (reader.Is(TimeSeriesCommandConstants.Point, TimeSeriesCommandConstants.Namespace, XmlNodeType.EndElement))
                {
                    point.ObservationTime = _iso8601Durations.AddDuration(
                        series.StartDateTime,
                        TimeSeriesResolutionMapper.Map(series.Resolution),
                        point.Position - 1);
                    break;
                }
            }

            return point;
        }
    }
}
