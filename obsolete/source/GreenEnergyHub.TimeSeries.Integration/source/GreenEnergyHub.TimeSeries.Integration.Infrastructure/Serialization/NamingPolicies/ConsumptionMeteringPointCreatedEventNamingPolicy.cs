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

namespace GreenEnergyHub.TimeSeries.Integration.Infrastructure.Serialization.NamingPolicies
{
    public class ConsumptionMeteringPointCreatedEventNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name)
        {
            return name switch
            {
                "MeteringPointId" => "metering_point_id",
                "MeteringPointType" => "metering_point_type",
                "GridArea" => "grid_area",
                "SettlementMethod" => "settlement_method",
                "MeteringMethod" => "metering_method",
                "Resolution" => "resolution",
                "Product" => "product",
                "ConnectionState" => "connection_state",
                "Unit" => "unit",
                "EffectiveDate" => "effective_date",
                "MessageVersion" => "MessageVersion",
                "MessageType" => "MessageType",
                "Transaction" => "Transaction",
                "OperationTimestamp" => "OperationTimestamp",
                "EventIdentification" => "EventIdentification",
                "OperationCorrelationId" => "OperationCorrelationId",
                _ => throw new ArgumentOutOfRangeException(nameof(name), name, "Could not convert property name.")
            };
        }
    }
}
