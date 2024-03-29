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

using Energinet.DataHub.Core.App.Common.Abstractions.Identity;
using Energinet.DataHub.Core.App.Common.Abstractions.Security;
using Energinet.DataHub.Core.App.Common.Identity;
using Energinet.DataHub.Core.App.Common.Security;
using Energinet.DataHub.Core.App.FunctionApp.Middleware;
using Energinet.DataHub.TimeSeries.Infrastructure.Registration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Energinet.DataHub.TimeSeries.TimeSeriesBundleIngestor
{
    internal static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds registrations of JwtTokenMiddleware and corresponding dependencies.
        /// </summary>
        /// <param name="serviceCollection">ServiceCollection container</param>
        public static void AddJwtTokenSecurity(this IServiceCollection serviceCollection)
        {
            var tenantId = EnvironmentHelper.GetEnv(EnvironmentSettingNames.B2CTenantId);
            var audience = EnvironmentHelper.GetEnv(EnvironmentSettingNames.BackendServiceAppId);
            var metadataAddress = $"https://login.microsoftonline.com/{tenantId}/v2.0/.well-known/openid-configuration";

            serviceCollection.AddScoped<IJwtTokenValidator, JwtTokenValidator>();
            serviceCollection.AddScoped<IClaimsPrincipalAccessor, ClaimsPrincipalAccessor>();
            serviceCollection.AddScoped<ClaimsPrincipalContext>();
            serviceCollection.AddScoped(_ => new OpenIdSettings(metadataAddress, audience));
            serviceCollection.AddScoped(_ => new JwtTokenMiddleware(
                _.GetRequiredService<ClaimsPrincipalContext>(),
                _.GetRequiredService<IJwtTokenValidator>(),
                new[] { "HealthCheck" }));
        }

        /// <summary>
        /// Gets an options builder that forwards Configure calls for the same TOptions to the underlying service collection
        /// </summary>
        /// <param name="serviceCollection">ServiceCollection container</param>
        /// <param name="key">The key of the configuration section</param>
        public static void AddOptions<TOptions>(this IServiceCollection serviceCollection, string key)
            where TOptions : class
        {
            serviceCollection.AddOptions<TOptions>().Configure<IConfiguration>((settings, configuration) =>
            {
                configuration.GetSection(key).Bind(settings);
            });
        }
    }
}
