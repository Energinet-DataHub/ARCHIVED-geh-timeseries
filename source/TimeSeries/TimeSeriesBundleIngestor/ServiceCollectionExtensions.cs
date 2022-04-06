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

using Energinet.DataHub.Core.App.Common.Abstractions.Security;
using Energinet.DataHub.Core.App.Common.Security;
using Energinet.DataHub.Core.App.FunctionApp.Middleware;
using Energinet.DataHub.Core.FunctionApp.Common.Abstractions.Identity;
using Energinet.DataHub.Core.FunctionApp.Common.Identity;
using Energinet.DataHub.TimeSeries.Infrastructure.Authentication;
using Energinet.DataHub.TimeSeries.Infrastructure.Registration;
using Microsoft.Extensions.DependencyInjection;
using OpenIdSettings = Energinet.DataHub.Core.App.Common.Security.OpenIdSettings;

namespace Energinet.DataHub.TimeSeries.MessageReceiver
{
    public static class ServiceCollectionExtensions
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
            serviceCollection.AddScoped<JwtTokenWrapperMiddleware>();
            serviceCollection.AddScoped<JwtTokenMiddleware>();
            serviceCollection.AddScoped(_ => new OpenIdSettings(metadataAddress, audience));
            serviceCollection.AddScoped<IJwtTokenValidator, JwtTokenValidator>();
            serviceCollection.AddScoped<IClaimsPrincipalAccessor, ClaimsPrincipalAccessor>();
            serviceCollection.AddScoped<ClaimsPrincipalContext>();
            serviceCollection.AddScoped<Core.App.Common.Identity.ClaimsPrincipalContext>();
        }
    }
}
