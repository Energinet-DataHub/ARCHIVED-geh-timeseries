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
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.IntegrationEventContracts;
using GreenEnergyHub.TimeSeries.Integration.Application.Interfaces;
using GreenEnergyHub.TimeSeries.Integration.Infrastructure.Messaging;
using GreenEnergyHub.TimeSeries.Integration.IntegrationEventListener.Common;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.TimeSeries.Integration.IntegrationEventListener.MeteringPoint
{
    public class MeteringPointConnectedListener
    {
        private readonly MessageExtractor<MeteringPointConnected> _messageExtractor;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly EventDataHelper _eventDataHelper;
        private readonly ILogger<MeteringPointConnectedListener> _logger;

        public MeteringPointConnectedListener(MessageExtractor<MeteringPointConnected> messageExtractor, IEventDispatcher eventDispatcher, EventDataHelper eventDataHelper, ILogger<MeteringPointConnectedListener> logger)
        {
            _messageExtractor = messageExtractor;
            _eventDispatcher = eventDispatcher;
            _eventDataHelper = eventDataHelper;
            _logger = logger;
        }

        [Function("MeteringPointConnectedListener")]
        public async Task RunAsync(
            [ServiceBusTrigger(
                "%METERING_POINT_CONNECTED_TOPIC_NAME%",
                "%METERING_POINT_CONNECTED_SUBSCRIPTION_NAME%",
                Connection = "INTEGRATION_EVENT_LISTENER_CONNECTION_STRING")] byte[] data,
            FunctionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var eventMetaData = _eventDataHelper.GetEventMetaData(context);

            _logger.LogTrace("MeteringPointConnected event received with {OperationCorrelationId}", eventMetaData.OperationCorrelationId);

            var request = await _messageExtractor.ExtractAsync(data).ConfigureAwait(false);

            await _eventDispatcher.DispatchAsync(request, _eventDataHelper.GetEventhubMetaData(eventMetaData, "MeteringPoint")).ConfigureAwait(false);
        }
    }
}
