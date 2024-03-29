﻿// Copyright 2020 Energinet DataHub A/S
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

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using GreenEnergyHub.Messaging.Transport;

namespace GreenEnergyHub.TimeSeries.Infrastructure.Messaging.Serialization.Commands
{
    public class TimeSeriesCommandDeserializer : MessageDeserializer
    {
        private readonly TimeSeriesCommandConverter _timeSeriesCommandConverter;

        public TimeSeriesCommandDeserializer(TimeSeriesCommandConverter timeSeriesCommandConverter)
        {
            _timeSeriesCommandConverter = timeSeriesCommandConverter;
        }

        public override async Task<IInboundMessage> FromBytesAsync(byte[] data, CancellationToken cancellationToken = default)
        {
            await using var stream = new MemoryStream(data);

            using var reader = XmlReader.Create(stream, new XmlReaderSettings { Async = true });

            var command = await _timeSeriesCommandConverter.ConvertAsync(reader).ConfigureAwait(false);

            return command;
        }
    }
}
