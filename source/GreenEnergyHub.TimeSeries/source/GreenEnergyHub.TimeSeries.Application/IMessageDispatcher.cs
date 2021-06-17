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

using System.Threading;
using System.Threading.Tasks;
using GreenEnergyHub.Messaging.Transport;

namespace GreenEnergyHub.TimeSeries.Application
{
    /// <summary>
    /// Interface for dispatching messages
    /// </summary>
    /// <typeparam name="TOutboundMessage">The type of message to dispatch</typeparam>
    public interface IMessageDispatcher<in TOutboundMessage>
        where TOutboundMessage : IOutboundMessage
    {
        /// <summary>
        /// Dispatches an outbound message
        /// </summary>
        /// <param name="message">Outbound message to dispatch</param>
        /// <param name="cancellationToken">token used for synchronize cancellation</param>
        /// <returns>Task dispatching the message</returns>
        public Task DispatchAsync(TOutboundMessage message, CancellationToken cancellationToken = default);
    }
}
