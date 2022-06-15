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
using System.Net;
using System.Threading.Tasks;
using Energinet.DataHub.TimeSeries.Application;
using Energinet.DataHub.TimeSeries.Application.CimDeserialization.TimeSeriesBundle;
using Energinet.DataHub.TimeSeries.Infrastructure.Functions;
using Energinet.DataHub.TimeSeries.Infrastructure.Registration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.TimeSeries.MessageReceiver
{
    public class TimeSeriesBundleIngestorEndpoint
    {
        private readonly ITimeSeriesForwarder _timeSeriesForwarder;
        private readonly IHttpResponseBuilder _httpResponseBuilder;
        private readonly ITimeSeriesBundleDtoValidatingDeserializer _timeSeriesBundleDtoValidatingDeserializer;

        public TimeSeriesBundleIngestorEndpoint(ITimeSeriesForwarder timeSeriesForwarder, ITimeSeriesBundleDtoValidatingDeserializer timeSeriesBundleDtoValidatingDeserializer, IHttpResponseBuilder httpResponseBuilder)
        {
            _timeSeriesForwarder = timeSeriesForwarder;
            _httpResponseBuilder = httpResponseBuilder;
            _timeSeriesBundleDtoValidatingDeserializer = timeSeriesBundleDtoValidatingDeserializer;
        }

        [Function(TimeSeriesFunctionNames.TimeSeriesBundleIngestor)]
        public async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
        {
            if (req == null)
                throw new ArgumentNullException(nameof(req));

            var deserializationResult = await _timeSeriesBundleDtoValidatingDeserializer
                .ValidateAndDeserializeAsync(req.Body)
                .ConfigureAwait(true);

            if (deserializationResult.HasErrors)
            {
                return await _httpResponseBuilder
                    .CreateBadRequestResponseAsync(req, deserializationResult.Errors)
                    .ConfigureAwait(false);
            }

            await _timeSeriesForwarder
                .HandleAsync(deserializationResult.TimeSeriesBundleDto)
                .ConfigureAwait(false);

            return _httpResponseBuilder.CreateAcceptedResponse(req);
        }
    }
}
