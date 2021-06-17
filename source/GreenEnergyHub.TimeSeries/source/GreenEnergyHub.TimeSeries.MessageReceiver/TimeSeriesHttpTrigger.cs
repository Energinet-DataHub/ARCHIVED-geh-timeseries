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

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using GreenEnergyHub.TimeSeries.Domain.Notification;
using GreenEnergyHub.TimeSeries.Infrastructure.Messaging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.TimeSeries.MessageReceiver
{
    public class TimeSeriesHttpTrigger
    {
        /// <summary>
        /// The name of the function.
        /// Function name affects the URL and thus possibly dependent infrastructure.
        /// </summary>
        private const string FunctionName = "TimeSeriesHttpTrigger";
        private readonly ICorrelationContext _correlationContext;
        private readonly MessageExtractor<TimeSeriesCommand> _messageExtractor;

        public TimeSeriesHttpTrigger(
            ICorrelationContext correlationContext,
            MessageExtractor<TimeSeriesCommand> messageExtractor)
        {
            _correlationContext = correlationContext;
            _messageExtractor = messageExtractor;
        }

        [FunctionName(FunctionName)]
        public async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
            [NotNull] HttpRequest req,
            [NotNull] ExecutionContext context,
            ILogger log)
        {
            log.LogInformation("Function {FunctionName} started to process a request with a body of size {SizeOfBody}", FunctionName, req.Body.Length);

            SetupCorrelationContext(context);

            return await Task.FromResult(new OkResult()).ConfigureAwait(false);
        }

        private async Task<TimeSeriesCommand> GetTimeSeriesCommandAsync(HttpRequest req)
        {
            var command = await _messageExtractor.ExtractAsync(req.Body).ConfigureAwait(false);

            return command;
        }

        private void SetupCorrelationContext(ExecutionContext context)
        {
            _correlationContext.CorrelationId = context.InvocationId.ToString();
        }
    }
}
