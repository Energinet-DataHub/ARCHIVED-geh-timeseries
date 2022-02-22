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
using NodaTime;
using NodaTime.Serialization.SystemTextJson;

namespace Energinet.DataHub.TimeSeries.Infrastructure.Serialization
{
    public class JsonSerializer : IJsonSerializer
    {
        private readonly JsonSerializerOptions _options;

        public JsonSerializer()
        {
            _options = new JsonSerializerOptions();
            _options.Converters.Add(NodaConverters.InstantConverter);
            _options.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
        }

        public async ValueTask<object> DeserializeAsync(Stream utf8Json, Type returnType)
        {
            if (utf8Json == null)
            {
                throw new ArgumentNullException(nameof(utf8Json));
            }

#pragma warning disable CS8603 // Possible null reference return.
            return await System.Text.Json.JsonSerializer.DeserializeAsync(utf8Json, returnType, _options).ConfigureAwait(false);
#pragma warning restore CS8603 // Possible null reference return.
        }

        public TValue Deserialize<TValue>(string json)
        {
            if (json == null)
            {
                throw new ArgumentNullException(nameof(json));
            }

#pragma warning disable CS8603
            return System.Text.Json.JsonSerializer.Deserialize<TValue>(json, _options);
#pragma warning restore CS8603
        }

        public object Deserialize(string json, Type returnType)
        {
            if (json == null)
            {
                throw new ArgumentNullException(nameof(json));
            }

#pragma warning disable CS8603
            return System.Text.Json.JsonSerializer.Deserialize(json, returnType, _options);
#pragma warning restore CS8603
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
