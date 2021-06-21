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

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Xml;
using GreenEnergyHub.Messaging.Transport;
using GreenEnergyHub.TimeSeries.Domain.Common;
using GreenEnergyHub.TimeSeries.Domain.Notification;
using GreenEnergyHub.TimeSeries.Infrastructure.Messaging.Serialization.Common;

namespace GreenEnergyHub.TimeSeries.Infrastructure.Messaging.Serialization.Commands
{
    public class TimeSeriesCommandConverter : DocumentConverter
    {
        private readonly ICorrelationContext _correlationContext;

        public TimeSeriesCommandConverter(ICorrelationContext correlationContext)
        {
            _correlationContext = correlationContext;
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

        private static async Task<Series> ParseSeriesAsync(XmlReader reader)
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
            }

            return series;
        }
    }
}
