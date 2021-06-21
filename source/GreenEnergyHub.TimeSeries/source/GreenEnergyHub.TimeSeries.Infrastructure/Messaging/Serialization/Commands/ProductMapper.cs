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
    public static class ProductMapper
    {
        public static Product Map(string value)
        {
            return value switch
            {
                "8716867000030" => Product.EnergyActive,
                "8716867000047" => Product.EnergyReactive,
                "8716867000016" => Product.PowerActive,
                "8716867000023" => Product.PowerReactive,
                "5790001330606" => Product.FuelQuantity,
                "5790001330590" => Product.Tariff,
                _ => Product.Unknown,
            };
        }
    }
}
