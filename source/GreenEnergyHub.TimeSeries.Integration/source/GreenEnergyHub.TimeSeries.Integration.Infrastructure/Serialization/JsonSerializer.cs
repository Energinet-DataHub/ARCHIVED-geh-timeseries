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
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using GreenEnergyHub.TimeSeries.Integration.Application.Interfaces;
using GreenEnergyHub.TimeSeries.Integration.Infrastructure.Serialization.Converters;
using GreenEnergyHub.TimeSeries.Integration.Infrastructure.Serialization.NamingPolicies;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;

namespace GreenEnergyHub.TimeSeries.Integration.Infrastructure.Serialization
{
    public class JsonSerializer : IJsonSerializer
    {
        private readonly JsonSerializerOptions _options;

        public JsonSerializer()
        {
            _options = new JsonSerializerOptions();
            _options.Converters.Add(NodaConverters.InstantConverter);
            _options.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
            _options.Converters.Add(new ConnectionStateConverter());
            _options.Converters.Add(new MeteringMethodConverter());
            _options.Converters.Add(new MeteringPointTypeConverter());
            _options.Converters.Add(new ResolutionConverter());
            _options.Converters.Add(new ProductConverter());
            _options.Converters.Add(new UnitConverter());
            _options.Converters.Add(new SettlementMethodConverter());
            _options.PropertyNamingPolicy = new ConsumptionMeteringPointCreatedEventNamingPolicy();
        }

        public async ValueTask<object?> DeserializeAsync(Stream utf8Json, Type returnType)
        {
            if (utf8Json == null)
            {
                throw new ArgumentNullException(nameof(utf8Json));
            }

            return await System.Text.Json.JsonSerializer.DeserializeAsync(utf8Json, returnType, _options).ConfigureAwait(false);
        }

        public TValue? Deserialize<TValue>(string json)
        {
            if (json == null)
            {
                throw new ArgumentNullException(nameof(json));
            }

            return System.Text.Json.JsonSerializer.Deserialize<TValue>(json, _options);
        }

        public object? Deserialize(string json, Type returnType)
        {
            if (json == null)
            {
                throw new ArgumentNullException(nameof(json));
            }

            return System.Text.Json.JsonSerializer.Deserialize(json, returnType, _options);
        }

        public string Serialize<TValue>(TValue value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return System.Text.Json.JsonSerializer.Serialize<object>(value, _options);
        }
    }
}
