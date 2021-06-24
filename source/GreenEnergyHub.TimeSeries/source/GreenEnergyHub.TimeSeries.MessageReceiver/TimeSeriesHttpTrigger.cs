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
using System.IO;
using System.Threading.Tasks;
using GreenEnergyHub.Messaging.Transport;
using GreenEnergyHub.TimeSeries.Application.Handlers;
using GreenEnergyHub.TimeSeries.Domain.Notification;
using GreenEnergyHub.TimeSeries.Infrastructure.Messaging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
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
        private readonly MessageExtractor _messageExtractor;
        private readonly ITimeSeriesCommandHandler _commandHandler;
        private readonly ILogger _log;

        public TimeSeriesHttpTrigger(
            ICorrelationContext correlationContext,
            MessageExtractor messageExtractor,
            ITimeSeriesCommandHandler commandHandler,
            [NotNull] ILoggerFactory loggerFactory)
        {
            _correlationContext = correlationContext;
            _messageExtractor = messageExtractor;
            _commandHandler = commandHandler;
            _log = loggerFactory.CreateLogger(nameof(TimeSeriesHttpTrigger));
        }

        [Function(FunctionName)]
        public async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            [NotNull] HttpRequestData req,
            [NotNull] FunctionContext context)
        {
            _log.LogInformation("Function {FunctionName} started to process a request", FunctionName);

            SetupCorrelationContext(context);

            var command = await GetTimeSeriesCommandAsync(req.Body).ConfigureAwait(false);

            var result = await _commandHandler.HandleAsync(command).ConfigureAwait(false);
            result.CorrelationId = _correlationContext.CorrelationId;

            return new OkObjectResult(result);
        }

        private static async Task<byte[]> ConvertStreamToBytesAsync(Stream stream)
        {
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms).ConfigureAwait(false);
            return ms.ToArray();
        }

        private async Task<TimeSeriesCommand> GetTimeSeriesCommandAsync(Stream stream)
        {
            var message = await ConvertStreamToBytesAsync(stream).ConfigureAwait(false);
            var command = (TimeSeriesCommand)await _messageExtractor.ExtractAsync(message).ConfigureAwait(false);

            return command;
        }

        private void SetupCorrelationContext(FunctionContext context)
        {
            _correlationContext.CorrelationId = context.InvocationId;
        }
    }
}
