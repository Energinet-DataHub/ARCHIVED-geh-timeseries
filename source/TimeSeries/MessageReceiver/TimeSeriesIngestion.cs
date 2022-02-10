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

using System.Net;
using System.Threading.Tasks;
using Energinet.DataHub.Core.Messaging.Transport;
using Energinet.DataHub.Core.Messaging.Transport.SchemaValidation;
using Energinet.DataHub.Core.Schemas;
using Energinet.DataHub.Core.SchemaValidation;
using Energinet.DataHub.Core.SchemaValidation.Extensions;
using Energinet.DataHub.TimeSeries.Application.Dtos;
using Energinet.DataHub.TimeSeries.Infrastructure.Function;
using Energinet.DataHub.TimeSeries.Infrastructure.MessagingExtensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Energinet.DataHub.TimeSeries.MessageReceiver
{
    public static class TimeSeriesIngestion
    {
        private readonly ITimeSeriesBundleHandler _timeSeriesBundleHandler;
        private readonly IHttpResponseBuilder _httpResponseBuilder;

        /// <summary>
        /// The name of the function.
        /// Function name affects the URL and thus possibly dependent infrastructure.
        /// </summary>
        private readonly ValidatingMessageExtractor<TimeSeriesBundleDto> _messageExtractor;

        public TimeSeriesIngestion(
            ITimeSeriesBundleHandler timeSeriesBundleHandler,
                IHttpResponseBuilder httpResponseBuilder,
            ValidatingMessageExtractor<TimeSeriesBundleDto> messageExtractor)
        {
            _timeSeriesBundleHandler = timeSeriesBundleHandler;
            _httpResponseBuilder = httpResponseBuilder;
            _messageExtractor = messageExtractor;
        }

        [Function(TimeSeriesFunctionNames.TimeSeriesBundleIngestor)]
        public async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData req)
        {
            var inboundMessage = await ValidateMessageAsync(req).ConfigureAwait(false);
            if (inboundMessage.HasErrors)
            {
                return await _httpResponseBuilder
                    .CreateBadRequestResponseAsync(req, inboundMessage.SchemaValidationError)
                    .ConfigureAwait(false);
            }

            var timeSeriesMessageResult = await _timeSeriesBundleHandler
                .HandleAsync(inboundMessage.ValidatedMessage)
                .ConfigureAwait(false);

            return await _httpResponseBuilder
                .CreateAcceptedResponseAsync(req, timeSeriesMessageResult)
                .ConfigureAwait(false);
        }

        private async Task<SchemaValidatedInboundMessage<TimeSeriesBundleDto>> ValidateMessageAsync(HttpRequestData request)
        {
            return (SchemaValidatedInboundMessage<TimeSeriesBundleDto>)await _messageExtractor
                .ExtractAsync(request.Body)
                .ConfigureAwait(false);
        }
    }
}
