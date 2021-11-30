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
using System.Text.Json;
using System.Text.Json.Serialization;
using GreenEnergyHub.TimeSeries.Integration.Application.Extensions;
using GreenEnergyHub.TimeSeries.Integration.Domain;

namespace GreenEnergyHub.TimeSeries.Integration.Infrastructure.Serialization.Converters
{
    public class MeteringMethodConverter : JsonConverter<MeteringMethod>
    {
        public override MeteringMethod Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();

            return value.GetEnumValueFromAttribute<MeteringMethod>();
        }

        public override void Write(Utf8JsonWriter writer, MeteringMethod value, JsonSerializerOptions options)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            var description = value.GetDescription();
            writer.WriteStringValue(description);
        }
    }
}
