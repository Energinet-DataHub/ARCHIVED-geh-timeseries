﻿// Copyright 2020 Energinet DataHub A/S
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

using GreenEnergyHub.TimeSeries.Domain.Common;

namespace GreenEnergyHub.TimeSeries.Infrastructure.Messaging.Serialization.Common
{
    public static class MarketParticipantRoleMapper
    {
        public static MarketParticipantRole Map(string value)
        {
            return value switch
            {
                "DDQ" => MarketParticipantRole.EnergySupplier,
                "DDM" => MarketParticipantRole.GridAccessProvider,
                "EZ" => MarketParticipantRole.SystemOperator,
                "MDR" => MarketParticipantRole.MeteredDataResponsible,
                "STS" => MarketParticipantRole.EnergyAgency,
                _ => MarketParticipantRole.Unknown
            };
        }
    }
}
