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

namespace Energinet.DataHub.TimeSeries.Application.Enums
{
    public enum MeteringPointType
    {
        Unknown = 0,
        Consumption = 1, // E17
        Production = 2, // E18
        Exchange = 3, // E20
        VeProduction = 4, // D01
        Analysis = 5, // D02
        SurplusProductionGroup = 6, // D04
        NetProduction = 7, // D05
        SupplyToGrid = 8, // D06
        ConsumptionFromGrid = 9, // D07
        WholesaleService = 10, // D08
        OwnProduction = 11, // D09
        NetFromGrid = 12, // D10
        NetToGrid = 13, // D11
        TotalConsumption = 14, // D12
        GridLossCorrection = 15, // D13
        ElectricalHeating = 16, // D14
        NetConsumption = 17, // D15
        OtherConsumption = 18, // D17
        OtherProduction = 19, // D18
        ExchangeReactiveEnergy = 20, // D20
        InternalUse = 21, // D99
    }
}
