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
    public static class MeteringPointTypeMapper
    {
        public static MeteringPointType Map(string value)
        {
            return value switch
            {
                "E17" => MeteringPointType.Consumption,
                "E18" => MeteringPointType.Production,
                "E20" => MeteringPointType.Exchange,
                "D01" => MeteringPointType.VeProduction,
                "D02" => MeteringPointType.Analysis,
                "D04" => MeteringPointType.SurplusProductionGroup,
                "D05" => MeteringPointType.NetProduction,
                "D06" => MeteringPointType.SupplyToGrid,
                "D07" => MeteringPointType.ConsumptionFromGrid,
                "D08" => MeteringPointType.WholesaleService,
                "D09" => MeteringPointType.OwnProduction,
                "D10" => MeteringPointType.NetFromGrid,
                "D11" => MeteringPointType.NetToGrid,
                "D12" => MeteringPointType.TotalConsumption,
                "D13" => MeteringPointType.GridLossCorrection,
                "D14" => MeteringPointType.ElectricalHeating,
                "D15" => MeteringPointType.NetConsumption,
                "D17" => MeteringPointType.OtherConsumption,
                "D18" => MeteringPointType.OtherProduction,
                "D20" => MeteringPointType.ExchangeReactiveEnergy,
                "D99" => MeteringPointType.InternalUse,
                _ => MeteringPointType.Unknown,
            };
        }
    }
}
