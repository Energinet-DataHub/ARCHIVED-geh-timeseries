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

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using GreenEnergyHub.Messaging.Transport;
using GreenEnergyHub.TimeSeries.Application;

namespace GreenEnergyHub.TimeSeries.Infrastructure.Messaging
{
    public class MessageDispatcher<TOutboundMessage> : MessageDispatcher, IMessageDispatcher<TOutboundMessage>
        where TOutboundMessage : IOutboundMessage
    {
        public MessageDispatcher([NotNull] MessageSerializer serializer, [NotNull] Channel<TOutboundMessage> channel)
            : base(serializer, channel)
        {
        }

        public async Task DispatchAsync(TOutboundMessage message, CancellationToken cancellationToken = default)
        {
            await base.DispatchAsync(message, cancellationToken).ConfigureAwait(false);
        }
    }
}
