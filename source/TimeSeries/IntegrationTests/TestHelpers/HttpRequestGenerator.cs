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
