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
    public class ConsumptionMeteringPointCreatedListener
    {
        private readonly MessageExtractor<ConsumptionMeteringPointCreated> _messageExtractor;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly EventDataHelper _eventDataHelper;
        private readonly ILogger<ConsumptionMeteringPointCreatedListener> _logger;

        public ConsumptionMeteringPointCreatedListener(MessageExtractor<ConsumptionMeteringPointCreated> messageExtractor, IEventDispatcher eventDispatcher, EventDataHelper eventDataHelper, ILogger<ConsumptionMeteringPointCreatedListener> logger)
        {
            _messageExtractor = messageExtractor;
            _eventDispatcher = eventDispatcher;
            _eventDataHelper = eventDataHelper;
            _logger = logger;
        }

        [Function("ConsumptionMeteringPointCreatedListener")]
        public async Task RunAsync(
            [ServiceBusTrigger(
                "%CONSUMPTION_METERING_POINT_CREATED_TOPIC_NAME%",
                "%CONSUMPTION_METERING_POINT_CREATED_SUBSCRIPTION_NAME%",
                Connection = "INTEGRATION_EVENT_LISTENER_CONNECTION_STRING")] byte[] data,
            FunctionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var eventMetaData = _eventDataHelper.GetEventMetaData(context);

            _logger.LogTrace("ConsumptionMeteringPointCreated event received with {OperationCorrelationId}", eventMetaData.CorrelationId);

            var request = await _messageExtractor.ExtractAsync(data).ConfigureAwait(false);

            await _eventDispatcher.DispatchAsync(request, EventDataHelper.GetEventhubMetaData(eventMetaData, "MeteringPoint")).ConfigureAwait(false);
        }
    }
}
