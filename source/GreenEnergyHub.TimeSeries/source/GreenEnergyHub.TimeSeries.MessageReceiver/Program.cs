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
            const string eventHubQueueString = "TIMESERIES_QUEUE_URL";
            var eventHubQueue = Environment.GetEnvironmentVariable(eventHubQueueString) ??
                                 throw new ArgumentNullException(
                                     eventHubQueueString,
                                     "does not exist in configuration settings");
            const string eventHubPasswordString = "TIMESERIES_QUEUE_CONNECTION_STRING";
            var eventHubPassword = Environment.GetEnvironmentVariable(eventHubPasswordString) ??
                                   throw new ArgumentNullException(
                                       eventHubPasswordString,
                                       "does not exist in configuration settings");
            const string eventHubTopicString = "TIMESERIES_QUEUE_TOPIC";
            var eventHubTopic = Environment.GetEnvironmentVariable(eventHubTopicString) ??
                                   throw new ArgumentNullException(
                                       eventHubTopicString,
                                       "does not exist in configuration settings");

            services.AddScoped<TimeSeriesCommandConverter>();
            services.AddScoped<MessageDeserializer, TimeSeriesCommandDeserializer>();
            services.AddMessaging()
                .AddEventHubMessageDispatcher<TimeSeriesCommand>(
                    eventHubQueue,
                    eventHubPassword,
                    eventHubTopic);
        }

        private static void ConfigureIso8601Services(IServiceCollection services)
        {
            const string timeZoneIdString = "LOCAL_TIMEZONENAME";
            var variables = Environment.GetEnvironmentVariables();
            var timeZoneId = Environment.GetEnvironmentVariable(timeZoneIdString) ??
                             throw new ArgumentNullException(
                                 timeZoneIdString,
                                 "does not exist in configuration settings");
            var timeZoneConfiguration = new Iso8601ConversionConfiguration(timeZoneId);
            services.AddSingleton<IIso8601ConversionConfiguration>(timeZoneConfiguration);
            services.AddSingleton<IIso8601Durations, Iso8601Durations>();
        }
    }
}
