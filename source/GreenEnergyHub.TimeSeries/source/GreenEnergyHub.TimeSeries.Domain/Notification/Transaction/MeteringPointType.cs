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

namespace GreenEnergyHub.TimeSeries.Domain.Notification.Transaction
{
    public enum MeteringPointType
    {
        Unknown = 0,
        Consumption = 17, // This will be received in E17 in ebiX
        Production = 18, // This will be received in E18 in ebiX
        Exchange = 20, // This will be received in E20 in ebiX
        VeProduction = 101, // This will be received as D01 in ebiX
        Analysis = 102, // This will be received as D02 in ebiX
        SurplusProductionGroup = 104, // This will be received as D04 in ebiX
        NetProduction = 105, // This will be received as D05 in ebiX
        SupplyToGrid = 106, // This will be received as D06 in ebiX
        ConsumptionFromGrid = 107, // This will be received as D07 in ebiX
        WholesaleService = 108, // This will received as D08 in ebiX
        OwnProduction = 109, // This will be received as D09 i ebiX
        NetFromGrid = 110, // This will be received as D10 in ebiX
        NetToGrid = 111, // This will be received as D11 in ebiX
        TotalConsumption = 112, // This will be received as D12 in ebiX
        GridLossCorrection = 113, // This will be received as D13 in ebiX
        ElectricalHeating = 114, // This will be received as D14 in ebiX
        NetConsumption = 115, // This will be received as D15 in ebiX
        OtherConsumption = 117, // This will be received as D17 in ebiX
        OtherProduction = 118, // This will be received as D18 in ebiX
        ExchangeReactiveEnergy = 120, // This will be received as D20 in ebiX
        InternalUse = 199, // This will be received as D99 in ebiX
    }
}
