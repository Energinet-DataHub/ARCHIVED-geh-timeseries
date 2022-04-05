using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Energinet.DataHub.TimeSeries.MessageReceiver.IntegrationTests.TestHelpers
{
    public static class TraceContextHelper
    {
        public static void ConfigureTraceContext(this HttpRequestMessage request, string correlationId)
        {
            if (request == null) throw new ArgumentException($"{nameof(request)} is null");

            // See https://tsuyoshiushio.medium.com/correlation-with-activity-with-application-insights-3-w3c-tracecontext-d9fb143c0ce2
            var traceParent = CreateTraceParentHttpHeaderValue(correlationId);
            request.Headers.Add("traceparent", traceParent);
        }

        /// <summary>
        /// Creates a trace parent value that can be used to track correlation ID across HTTP requests.
        /// </summary>
        private static string CreateTraceParentHttpHeaderValue(string correlationId)
        {
            return $"00-{correlationId}-b7ad6b7169203331-01";
        }
    }
}
