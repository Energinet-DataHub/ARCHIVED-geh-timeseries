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
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Azurite;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Energinet.DataHub.Core.FunctionApp.TestCommon.EventHub.ListenerMock;
using Energinet.DataHub.Core.FunctionApp.TestCommon.EventHub.ResourceProvider;
using Energinet.DataHub.Core.FunctionApp.TestCommon.FunctionAppHost;
using Microsoft.Extensions.Configuration;

namespace Energinet.DataHub.TimeSeries.MessageReceiver.IntegrationTests.Fixtures
{
    public class TimeSeriesFunctionAppFixture : FunctionAppFixture
    {
        public TimeSeriesFunctionAppFixture()
        {
            AzuriteManager = new AzuriteManager();
            IntegrationTestConfiguration = new IntegrationTestConfiguration();
            AuthorizationConfiguration = new AuthorizationConfiguration();
            EventHubResourceProvider = new EventHubResourceProvider(IntegrationTestConfiguration.EventHubConnectionString, IntegrationTestConfiguration.ResourceManagementSettings, TestLogger);
            LogContainerClient = new BlobContainerClient("UseDevelopmentStorage=true", "marketoplogs");
        }

        [NotNull]
        public EventHubListenerMock? EventHubListener { get; private set; }

        public AuthorizationConfiguration AuthorizationConfiguration { get; }

        public BlobContainerClient LogContainerClient { get; }

        private IntegrationTestConfiguration IntegrationTestConfiguration { get; }

        private AzuriteManager AzuriteManager { get; }

        private EventHubResourceProvider EventHubResourceProvider { get; }

        /// <inheritdoc/>
        protected override void OnConfigureHostSettings(FunctionAppHostSettings hostSettings)
        {
            if (hostSettings == null)
            {
                return;
            }

            var buildConfiguration = GetBuildConfiguration();
            hostSettings.FunctionApplicationPath = $"..\\..\\..\\..\\TimeSeriesBundleIngestor\\bin\\{buildConfiguration}\\net5.0";

            // The log message we expect in the host log when the host is started and ready to server.
            hostSettings.HostStartedEvent = "Worker process started and initialized";
        }

        /// <inheritdoc/>
        protected override async Task OnInitializeFunctionAppDependenciesAsync(IConfiguration localSettingsSnapshot)
        {
            try
            {
                AzuriteManager.StartAzurite();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            // Shared logging blob storage container
            await LogContainerClient.CreateIfNotExistsAsync().ConfigureAwait(false);

            // => Event Hub
            // Overwrite event hub related settings, so the function app uses the names we have control of in the test
            Environment.SetEnvironmentVariable("EVENT_HUB_CONNECTION_STRING", EventHubResourceProvider.ConnectionString);

            var eventHub = await EventHubResourceProvider
                .BuildEventHub("evh-timeseries").SetEnvironmentVariableToEventHubName("EVENT_HUB_NAME")
                .CreateAsync().ConfigureAwait(false);

            EventHubListener = new EventHubListenerMock(EventHubResourceProvider.ConnectionString, eventHub.Name, "UseDevelopmentStorage=true", "container", TestLogger);
            await EventHubListener.InitializeAsync().ConfigureAwait(false);
        }

        /// <inheritdoc/>
        protected override void OnConfigureEnvironment()
        {
            Environment.SetEnvironmentVariable("AzureWebJobsStorage", "UseDevelopmentStorage=true");
            Environment.SetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY", IntegrationTestConfiguration.ApplicationInsightsInstrumentationKey);
            Environment.SetEnvironmentVariable("REQUEST_RESPONSE_LOGGING_CONNECTION_STRING", "UseDevelopmentStorage=true");
            Environment.SetEnvironmentVariable("REQUEST_RESPONSE_LOGGING_CONTAINER_NAME", "marketoplogs");
            Environment.SetEnvironmentVariable("B2C_TENANT_ID", AuthorizationConfiguration.B2cTenantId);
            Environment.SetEnvironmentVariable("BACKEND_SERVICE_APP_ID", AuthorizationConfiguration.BackendAppId);
        }

        /// <inheritdoc/>
        protected override Task OnFunctionAppHostFailedAsync(IReadOnlyList<string> hostLogSnapshot, Exception exception)
        {
            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }

            return base.OnFunctionAppHostFailedAsync(hostLogSnapshot, exception);
        }

        /// <inheritdoc/>
        protected override async Task OnDisposeFunctionAppDependenciesAsync()
        {
            // => Event Hub
            await EventHubListener.DisposeAsync().ConfigureAwait(false);
            await EventHubResourceProvider.DisposeAsync().ConfigureAwait(false);

            // => Storage
            AzuriteManager.Dispose();
        }

        private static string GetBuildConfiguration()
        {
#if DEBUG
            return "Debug";
#else
            return "Release";
#endif
        }
    }
}
