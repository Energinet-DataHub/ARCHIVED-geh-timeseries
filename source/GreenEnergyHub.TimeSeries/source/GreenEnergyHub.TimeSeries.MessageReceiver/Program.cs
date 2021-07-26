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
using System.Diagnostics;
using GreenEnergyHub.Iso8601;
using GreenEnergyHub.Messaging.Transport;
using GreenEnergyHub.TimeSeries.Application.Handlers;
using GreenEnergyHub.TimeSeries.Domain.Notification;
using GreenEnergyHub.TimeSeries.Infrastructure.Messaging.Registration;
using GreenEnergyHub.TimeSeries.Infrastructure.Messaging.Serialization.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NodaTime;

namespace GreenEnergyHub.TimeSeries.MessageReceiver
{
    public static class Program
    {
        public static void Main()
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices(ConfigureServices)
                .Build();

            host.Run();
        }

        private static void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped(typeof(IClock), _ => SystemClock.Instance);
            serviceCollection.AddLogging();
            ConfigureIso8601Services(serviceCollection);
            ConfigureMessaging(serviceCollection);
            serviceCollection.AddScoped<ITimeSeriesCommandHandler, TimeSeriesCommandHandler>();
        }

        private static void ConfigureMessaging(IServiceCollection services)
        {
            var eventHubQueue = GetEnvironmentSetting("TIMESERIES_QUEUE_URL");
            var eventHubPassword = GetEnvironmentSetting("TIMESERIES_QUEUE_CONNECTION_STRING");
            var eventHubTopic = GetEnvironmentSetting("TIMESERIES_QUEUE_TOPIC");
            var cacertPath = GetEnvironmentSetting("CACERT_PATH");

            services.AddScoped<TimeSeriesCommandConverter>();
            services.AddScoped<MessageDeserializer, TimeSeriesCommandDeserializer>();
            services.AddMessaging()
                .AddKafkaMessageDispatcher<TimeSeriesCommand>(
                    eventHubQueue,
                    eventHubPassword,
                    eventHubTopic,
                    cacertPath);
        }

        private static void ConfigureIso8601Services(IServiceCollection services)
        {
            var timeZoneId = GetEnvironmentSetting("LOCAL_TIMEZONENAME");
            var timeZoneConfiguration = new Iso8601ConversionConfiguration(timeZoneId);
            services.AddSingleton<IIso8601ConversionConfiguration>(timeZoneConfiguration);
            services.AddSingleton<IIso8601Durations, Iso8601Durations>();
        }

        private static string GetEnvironmentSetting(string settingString)
        {
            return Environment.GetEnvironmentVariable(settingString) ??
                throw new ArgumentNullException(
                    settingString,
                    "does not exist in configuration settings");
        }
    }
}
