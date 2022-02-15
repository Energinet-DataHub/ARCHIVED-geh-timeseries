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
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Energinet.DataHub.TimeSeries.Application;
using Energinet.DataHub.TimeSeries.Application.Dtos;

namespace Energinet.DataHub.TimeSeries.Infrastructure
{
    public class EventHubConnectionHandler : IEventHubConnectionHandler, IAsyncDisposable
    {
        private static readonly string? _connectionString = Environment.GetEnvironmentVariable("EVENT_HUB_CONNECTION_STRING");
        private static readonly string? _eventHubName = Environment.GetEnvironmentVariable("EVENT_HUB_NAME");
        private readonly EventHubProducerClient _eventHubProducerClient;

        public EventHubConnectionHandler()
        {
            _eventHubProducerClient = new EventHubProducerClient(_connectionString, _eventHubName);
        }

        public async Task EventSenderAsync(TimeSeriesBundleDto timeSeriesBundle)
        {
            var timeSeriesBundleJson = JsonSerializer.Serialize(timeSeriesBundle);
            using EventDataBatch eventDataBatch =
                await _eventHubProducerClient.CreateBatchAsync().ConfigureAwait(false);
            eventDataBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes(timeSeriesBundleJson)));
            await _eventHubProducerClient.SendAsync(eventDataBatch).ConfigureAwait(false);
        }

        public async ValueTask DisposeAsync()
        {
            await _eventHubProducerClient.DisposeAsync().ConfigureAwait(false);
        }
    }
}
