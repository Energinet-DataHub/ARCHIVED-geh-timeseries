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
using System.Diagnostics.CodeAnalysis;
using GreenEnergyHub.Iso8601;
using GreenEnergyHub.Messaging.Transport;
using GreenEnergyHub.TimeSeries.Application.Handlers;
using GreenEnergyHub.TimeSeries.Core.DateTime;
using GreenEnergyHub.TimeSeries.Domain.Notification;
using GreenEnergyHub.TimeSeries.Infrastructure.Messaging.Registration;
using GreenEnergyHub.TimeSeries.Infrastructure.Messaging.Serialization.Commands;
using GreenEnergyHub.TimeSeries.MessageReceiver;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

[assembly: FunctionsStartup(typeof(Startup))]

namespace GreenEnergyHub.TimeSeries.MessageReceiver
{
    public class Startup : FunctionsStartup
    {
        public override void Configure([NotNull] IFunctionsHostBuilder builder)
        {
            builder.Services.AddScoped(typeof(IClock), _ => SystemClock.Instance);

            ConfigureIso8601Services(builder.Services);
            ConfigureMessaging(builder);

            builder.Services.AddScoped<ITimeSeriesCommandHandler, TimeSeriesCommandHandler>();
        }

        private static void ConfigureMessaging(IFunctionsHostBuilder builder)
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

            builder.Services.AddScoped<TimeSeriesCommandConverter>();
            builder.Services.AddScoped<MessageDeserializer, TimeSeriesCommandDeserializer>();
            builder.Services.AddMessaging()
                .AddEventHubMessageDispatcher<TimeSeriesCommand>(
                    eventHubQueue,
                    eventHubPassword);
        }

        private static void ConfigureIso8601Services(IServiceCollection services)
        {
            const string timeZoneIdString = "LOCAL_TIMEZONENAME";
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
