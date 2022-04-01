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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;

namespace GreenEnergyHub.TimeSeries.Integration.Infrastructure.Wrappers
{
    public class EventHubProducerClientWrapper : IEventHubProducerClientWrapper
    {
        private readonly EventHubProducerClient _eventHubProducerClient;

        public EventHubProducerClientWrapper(EventHubProducerClient eventHubProducerClient)
        {
            _eventHubProducerClient = eventHubProducerClient;
        }

        public async Task SendAsync(EventDataBatch eventDataBatch, CancellationToken cancellationToken = default)
        {
            await _eventHubProducerClient.SendAsync(eventDataBatch, cancellationToken).ConfigureAwait(false);
        }

        public async Task CloseAsync(CancellationToken cancellationToken = default)
        {
            await _eventHubProducerClient.CloseAsync(cancellationToken).ConfigureAwait(false);
        }

        public async ValueTask DisposeAsync()
        {
            await _eventHubProducerClient.DisposeAsync().ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }

        public async Task<EventDataBatch> CreateEventBatchAsync(string message, Dictionary<string, string> metadata, CancellationToken cancellationToken)
        {
            if (metadata == null) throw new ArgumentNullException(nameof(metadata));
            var eventBatch = await _eventHubProducerClient.CreateBatchAsync(cancellationToken).ConfigureAwait(false);
            var eventData = new EventData(message);

            foreach (var (key, value) in metadata)
            {
                eventData.Properties.Add(key, value);
            }

            if (eventBatch.TryAdd(eventData)) return eventBatch;

            throw new InvalidOperationException($"Could not add event data to event batch: {eventData}");
        }
    }
}
