using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Energinet.DataHub.Core.App.FunctionApp.Middleware;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;

namespace Energinet.DataHub.TimeSeries.Infrastructure.Authentication
{
    /// <summary>
    /// Temporary middleware workaround to suppress authentication requirements on selected
    /// HTTP endpoints.
    ///
    /// The idea is to not register <see cref="JwtTokenMiddleware"/> as middleware but rather invoke
    /// it from this wrapping middleware if authentication is required. This is because the
    /// <see cref="JwtTokenMiddleware"/> currently doesn't support configuration of which endpoints
    /// to authenticate.
    /// </summary>
    public class JwtTokenWrapperMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly JwtTokenMiddleware _jwtTokenMiddleware;

        // Health check endpoints must allow anonymous access so we can use them with Azure monitoring:
        // https://docs.microsoft.com/en-us/azure/app-service/monitor-instances-health-check#authentication-and-security
        private readonly List<string> _functionNamesToExclude = new () { "HealthCheck" };

        public JwtTokenWrapperMiddleware(JwtTokenMiddleware jwtTokenMiddleware)
        {
            _jwtTokenMiddleware = jwtTokenMiddleware;
        }

        public async Task Invoke(FunctionContext context, [NotNull] FunctionExecutionDelegate next)
        {
            if (context == null) throw new ArgumentException($"{nameof(context)} is null");

            var allowAnonymous = _functionNamesToExclude.Contains(context.FunctionDefinition.Name);
            if (allowAnonymous)
            {
                await next(context).ConfigureAwait(false);
                return;
            }

            await _jwtTokenMiddleware.Invoke(context, next).ConfigureAwait(false);
        }
    }
}
