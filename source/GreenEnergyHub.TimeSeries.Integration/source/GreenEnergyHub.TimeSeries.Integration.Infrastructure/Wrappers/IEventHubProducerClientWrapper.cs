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
using Azure.Messaging.EventHubs.Producer;

namespace GreenEnergyHub.TimeSeries.Integration.Infrastructure.Wrappers
{
    /// <summary>
    /// Wrapper interface for using EventHubProducerClient
    /// </summary>
    public interface IEventHubProducerClientWrapper : IAsyncDisposable
    {
        // /// <summary>
        // /// Wrapper for creating event data
        // /// </summary>
        // /// <param name="cancellationToken"></param>
        // ValueTask<EventDataBatch> CreateBatchAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Wrapper method for sending event data
        /// </summary>
        /// <param name="eventDataBatch"></param>
        /// <param name="cancellationToken"></param>
        Task SendAsync(EventDataBatch eventDataBatch, CancellationToken cancellationToken = default);

        /// <summary>
        /// Wrapper method for closing connection
        /// </summary>
        /// <param name="cancellationToken"></param>
        Task CloseAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Create a new EventDataBatch and add message to it
        /// </summary>
        /// <param name="message"></param>
        /// <param name="metadata"></param>
        /// <param name="cancellationToken"></param>
        Task<EventDataBatch> CreateEventBatchAsync(string message, Dictionary<string, string> metadata, CancellationToken cancellationToken);
    }
}
