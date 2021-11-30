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
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Azurite;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Energinet.DataHub.Core.FunctionApp.TestCommon.EventHub.ListenerMock;
using Energinet.DataHub.Core.FunctionApp.TestCommon.EventHub.ResourceProvider;
using Energinet.DataHub.Core.FunctionApp.TestCommon.FunctionAppHost;
using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ResourceProvider;
using Microsoft.Extensions.Configuration;

namespace GreenEnergyHub.TimeSeries.Integration.IntegrationTests.Fixtures
{
    public class AggregationsFunctionAppFixture : FunctionAppFixture
    {
        public AggregationsFunctionAppFixture()
        {
            AzuriteManager = new AzuriteManager();
            IntegrationTestConfiguration = new IntegrationTestConfiguration();
            ServiceBusResourceProvider = new ServiceBusResourceProvider(IntegrationTestConfiguration.ServiceBusConnectionString, TestLogger);
            EventHubResourceProvider = new EventHubResourceProvider(IntegrationTestConfiguration.EventHubConnectionString, IntegrationTestConfiguration.ResourceManagementSettings, TestLogger);
        }

        [NotNull]
        public TopicResource? MPCreatedTopic { get; private set; }

        [NotNull]
        public EventHubListenerMock? EventHubListener { get; private set; }

        private AzuriteManager AzuriteManager { get; }

        private IntegrationTestConfiguration IntegrationTestConfiguration { get; }

        private ServiceBusResourceProvider ServiceBusResourceProvider { get; }

        private EventHubResourceProvider EventHubResourceProvider { get; }

        /// <inheritdoc/>
        protected override void OnConfigureHostSettings(FunctionAppHostSettings hostSettings)
        {
            if (hostSettings == null)
            {
                return;
            }

            var buildConfiguration = GetBuildConfiguration();
            hostSettings.FunctionApplicationPath = $"..\\..\\..\\..\\GreenEnergyHub.TimeSeries.IntegrationEventListener\\bin\\{buildConfiguration}\\net5.0";
        }

        /// <inheritdoc/>
        protected override void OnConfigureEnvironment()
        {
            Environment.SetEnvironmentVariable("AzureWebJobsStorage", "UseDevelopmentStorage=true");
            Environment.SetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY", IntegrationTestConfiguration.ApplicationInsightsInstrumentationKey);
        }

        /// <inheritdoc/>
        protected override async Task OnInitializeFunctionAppDependenciesAsync(IConfiguration localSettingsSnapshot)
        {
            // => Storage
            AzuriteManager.StartAzurite();

            // => Service Bus
            // Overwrite service bus related settings, so the function app uses the names we have control of in the test
            Environment.SetEnvironmentVariable("INTEGRATION_EVENT_LISTENER_CONNECTION_STRING", ServiceBusResourceProvider.ConnectionString);

            MPCreatedTopic = await ServiceBusResourceProvider
                .BuildTopic("sbt-mp-created").SetEnvironmentVariableToTopicName("CONSUMPTION_METERING_POINT_CREATED_TOPIC_NAME")
                .AddSubscription("subscription").SetEnvironmentVariableToSubscriptionName("CONSUMPTION_METERING_POINT_CREATED_SUBSCRIPTION_NAME")
                .CreateAsync().ConfigureAwait(false);

            await ServiceBusResourceProvider
                .BuildTopic("sbt-mp-connected").SetEnvironmentVariableToTopicName("METERING_POINT_CONNECTED_TOPIC_NAME")
                .AddSubscription("subscription").SetEnvironmentVariableToSubscriptionName("METERING_POINT_CONNECTED_SUBSCRIPTION_NAME")
                .CreateAsync().ConfigureAwait(false);

            await ServiceBusResourceProvider
                .BuildTopic("sbt-supplier-changed").SetEnvironmentVariableToTopicName("ENERGY_SUPPLIER_CHANGED_TOPIC_NAME")
                .AddSubscription("subscription").SetEnvironmentVariableToSubscriptionName("ENERGY_SUPPLIER_CHANGED_SUBSCRIPTION_NAME")
                .CreateAsync().ConfigureAwait(false);

            // => Event Hub
            // Overwrite event hub related settings, so the function app uses the names we have control of in the test
            Environment.SetEnvironmentVariable("EVENT_HUB_CONNECTION", EventHubResourceProvider.ConnectionString);

            var eventHub = await EventHubResourceProvider
                .BuildEventHub("evh-aggregation").SetEnvironmentVariableToEventHubName("EVENT_HUB_NAME")
                .CreateAsync().ConfigureAwait(false);

            EventHubListener = new EventHubListenerMock(EventHubResourceProvider.ConnectionString, eventHub.Name, "UseDevelopmentStorage=true", "container", TestLogger);
            await EventHubListener.InitializeAsync().ConfigureAwait(false);
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
            // => Service Bus
            await ServiceBusResourceProvider.DisposeAsync().ConfigureAwait(false);

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
