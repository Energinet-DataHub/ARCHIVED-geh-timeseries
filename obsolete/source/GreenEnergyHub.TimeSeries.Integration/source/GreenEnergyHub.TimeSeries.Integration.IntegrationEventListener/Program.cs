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

using System.IO;
using Azure.Messaging.EventHubs.Producer;
using GreenEnergyHub.TimeSeries.Integration.Application.Interfaces;
using GreenEnergyHub.TimeSeries.Integration.Infrastructure;
using GreenEnergyHub.TimeSeries.Integration.Infrastructure.Messaging.Registration;
using GreenEnergyHub.TimeSeries.Integration.Infrastructure.Serialization;
using GreenEnergyHub.TimeSeries.Integration.Infrastructure.Wrappers;
using GreenEnergyHub.TimeSeries.Integration.IntegrationEventListener.Common;
using GreenEnergyHub.TimeSeries.Integration.IntegrationEventListener.Configuration;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace GreenEnergyHub.TimeSeries.Integration.IntegrationEventListener
{
    public static class Program
    {
        public static void Main()
        {
            var host = new HostBuilder().ConfigureAppConfiguration(configurationBuilder =>
                {
                    configurationBuilder.SetBasePath(Directory.GetCurrentDirectory());
                    configurationBuilder.AddJsonFile("local.settings.json", true, true);
                    configurationBuilder.AddEnvironmentVariables();
                })
                .ConfigureFunctionsWorkerDefaults();

            var buildHost = host.ConfigureServices((context, services) =>
            {
                using var telemetryConfiguration = TelemetryConfiguration.CreateDefault();
                telemetryConfiguration.InstrumentationKey = context.Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"];
                var logger = new LoggerConfiguration()
                    .WriteTo.Console()
                    .WriteTo.ApplicationInsights(telemetryConfiguration, TelemetryConverter.Traces)
                    .CreateLogger();

                services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(logger));
                services.AddSingleton<IEventDispatcher, EventDispatcher>();
                services.AddSingleton<IJsonSerializer, JsonSerializer>();
                services.AddSingleton<EventDataHelper>();
                services.AddSingleton<IEventHubProducerClientWrapper, EventHubProducerClientWrapper>();
                services.AddSingleton(new EventHubProducerClient(
                    context.Configuration["EVENT_HUB_CONNECTION"],
                    context.Configuration["EVENT_HUB_NAME"]));

                services.ConfigureProtobufReception();
                ConsumptionMeteringPointCreatedHandlerConfiguration.ConfigureServices(services);
                MeteringPointConnectedHandlerConfiguration.ConfigureServices(services);
            }).Build();

            buildHost.Run();
        }
    }
}
