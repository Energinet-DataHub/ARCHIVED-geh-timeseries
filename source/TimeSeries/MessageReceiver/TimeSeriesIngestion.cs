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

using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;
using Energinet.DataHub.Core.Schemas;
using Energinet.DataHub.Core.SchemaValidation;
using Energinet.DataHub.Core.SchemaValidation.Extensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Energinet.DataHub.TimeSeries.MessageReceiver
{
    public static class TimeSeriesIngestion
    {
        [Function(TimeSeriesFunctionNames.TimeSeriesIngestion)]
        public static async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData request)
        {
            var (succeeded, errorResponse, element) = await ValidateAndReadXmlAsync(request).ConfigureAwait(false);

            if (!succeeded)
            {
                return errorResponse ?? request.CreateResponse(HttpStatusCode.BadRequest);
            }

            var response = request.CreateResponse(HttpStatusCode.Accepted);
            return await Task.FromResult(response).ConfigureAwait(false);
        }

        private static async Task<(bool Succeeded, HttpResponseData? ErrorResponse, XElement? Element)> ValidateAndReadXmlAsync(HttpRequestData request)
        {
            var reader = new SchemaValidatingReader(request.Body, Schemas.CimXml.StructureRequestChangeAccountingPointCharacteristics);

            HttpResponseData? response = null;
            var isSucceeded = true;

            var xmlElement = await reader.AsXElementAsync().ConfigureAwait(false);

            if (!reader.HasErrors)
            {
                return (isSucceeded, response, xmlElement);
            }

            isSucceeded = false;
            response = request.CreateResponse(HttpStatusCode.BadRequest);

            await reader
                .CreateErrorResponse()
                .WriteAsXmlAsync(response.Body)
                .ConfigureAwait(false);

            return (isSucceeded, response, xmlElement);
        }

    }
}
