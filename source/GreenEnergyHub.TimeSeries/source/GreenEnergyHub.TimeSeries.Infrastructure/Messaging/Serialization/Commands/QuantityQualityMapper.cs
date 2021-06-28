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

using GreenEnergyHub.TimeSeries.Domain.Notification;

namespace GreenEnergyHub.TimeSeries.Infrastructure.Messaging.Serialization.Commands
{
    public static class QuantityQualityMapper
    {
        public static QuantityQuality Map(string value)
        {
            return value switch
            {
                "E01" => QuantityQuality.Measured,
                "56" => QuantityQuality.Estimated,
                "36" => QuantityQuality.Revised,
                "D01" => QuantityQuality.Calculated,
                "D99" => QuantityQuality.QuantityMissing, // D99 is probably not the correct quantity missing code.
                _ => QuantityQuality.Unknown,
            };
        }
    }
}
