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
using System.Threading;
using System.Threading.Tasks;
using GreenEnergyHub.Messaging.Transport;
using GreenEnergyHub.Queues.Kafka;

namespace GreenEnergyHub.TimeSeries.Infrastructure.Messaging
{
    /// <summary>
    /// Implementation of a channel using the Azure EventHub
    /// </summary>
    /// <typeparam name="TOutboundMessage">Type of the message this channel is responsible for</typeparam>
    public class EventHubChannel<TOutboundMessage> : Channel<TOutboundMessage>
        where TOutboundMessage : IOutboundMessage
    {
        private readonly IKafkaDispatcher _kafkaDispatcher;

        public EventHubChannel(
            [NotNull] IKafkaDispatcher kafkaDispatcher)
        {
            _kafkaDispatcher = kafkaDispatcher;
        }

        /// <summary>
        /// Writes an array of bytes to the EventHub
        /// </summary>
        /// <param name="data">Data to send</param>
        /// <param name="cancellationToken">Token to use for cancelling the operation</param>
        /// <returns>The task sending the data to the EventHub</returns>
        protected override async Task WriteAsync(byte[] data, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}
