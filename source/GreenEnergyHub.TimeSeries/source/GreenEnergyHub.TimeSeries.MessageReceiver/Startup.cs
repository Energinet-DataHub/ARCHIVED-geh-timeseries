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
using GreenEnergyHub.TimeSeries.Domain.Notification;
using GreenEnergyHub.TimeSeries.Infrastructure.Messaging;
using GreenEnergyHub.TimeSeries.Infrastructure.Messaging.Registration;
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

            ConfigureMessaging(builder);
        }

        private static void ConfigureMessaging(IFunctionsHostBuilder builder)
        {
            builder.Services
                .AddMessaging()
                .AddMessageExtractor<TimeSeriesCommand>();
        }
    }
}
