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

using System.Text;
using System.Threading.Tasks;
using Energinet.DataHub.Core.JsonSerialization;
using Energinet.DataHub.TimeSeries.Application.Dtos;
using Energinet.DataHub.TimeSeries.Infrastructure.Blob;
using Energinet.DataHub.TimeSeries.Infrastructure.EventHub;

namespace Energinet.DataHub.TimeSeries.Application
{
    public class TimeSeriesForwarder : ITimeSeriesForwarder
    {
        private readonly IEventHubSender _eventHubSender;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IRawTimeSeriesStorageClient _rawTimeSeriesStorageClient;
        private readonly ITimeSeriesBundleToJsonConverter _timeSeriesBundleToJsonConverter;

        public TimeSeriesForwarder(
            ITimeSeriesBundleToJsonConverter timeSeriesBundleToJsonConverter,
            IRawTimeSeriesStorageClient rawTimeSeriesStorageClient,
            IEventHubSender eventHubSender,
            IJsonSerializer jsonSerializer)
        {
            _timeSeriesBundleToJsonConverter = timeSeriesBundleToJsonConverter;
            _rawTimeSeriesStorageClient = rawTimeSeriesStorageClient;
            _eventHubSender = eventHubSender;
            _jsonSerializer = jsonSerializer;
        }

        public async Task HandleAsync(TimeSeriesBundleDto timeSeriesBundle)
        {
            var fileName = $"{timeSeriesBundle.Document.Id}.json";
            await using var stream = await _rawTimeSeriesStorageClient.OpenWriteAsync(fileName);
            await _timeSeriesBundleToJsonConverter.ConvertAsync(timeSeriesBundle, stream);

            var body = Encoding.UTF8.GetBytes(_jsonSerializer.Serialize(timeSeriesBundle));
            await _eventHubSender.SendAsync(body).ConfigureAwait(false);
        }
    }
}
