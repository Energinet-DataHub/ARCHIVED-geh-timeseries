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
using System.Net.Http;

namespace Energinet.DataHub.TimeSeries.MessageReceiver.IntegrationTests.TestHelpers
{
    public static class HttpRequestGenerator
    {
#pragma warning disable CA1054 // URI-like parameters should not be strings
        public static (HttpRequestMessage Request, string CorrelationId) CreateHttpGetRequest(string endpointUrl)
#pragma warning restore CA1054 // URI-like parameters should not be strings
        {
            return CreateHttpRequest(HttpMethod.Get, endpointUrl);
        }

        private static (HttpRequestMessage Request, string CorrelationId) CreateHttpRequest(
            HttpMethod httpMethod,
            string endpointUrl)
        {
            var request = new HttpRequestMessage(httpMethod, endpointUrl);

            var correlationId = Create();
            request.ConfigureTraceContext(correlationId);

            return (request, correlationId);
        }

        private static string Create()
        {
            return Guid.NewGuid().ToString().Replace("-", string.Empty);
        }
    }
}
