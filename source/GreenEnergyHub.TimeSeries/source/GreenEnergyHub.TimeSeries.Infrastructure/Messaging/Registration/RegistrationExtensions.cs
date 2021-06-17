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

using GreenEnergyHub.Json;
using GreenEnergyHub.Messaging.Transport;
using GreenEnergyHub.TimeSeries.Infrastructure.Messaging.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace GreenEnergyHub.TimeSeries.Infrastructure.Messaging.Registration
{
    public static class RegistrationExtensions
    {
        public static MessagingRegistrator AddMessaging(this IServiceCollection services)
        {
            services.AddScoped<ICorrelationContext, CorrelationContext>();
            services.AddScoped<MessageExtractor>();
            services.AddScoped<MessageSerializer, JsonMessageSerializer>();
            services.AddScoped<IJsonOutboundMapperFactory, DefaultJsonMapperFactory>();
            services.AddScoped<MessageDeserializer, TimeSeriesCommandDeserializer>();
            services.AddSingleton<IJsonSerializer, GreenEnergyHub.TimeSeries.Core.Json.JsonSerializer>();

            return new MessagingRegistrator(services);
        }
    }
}
