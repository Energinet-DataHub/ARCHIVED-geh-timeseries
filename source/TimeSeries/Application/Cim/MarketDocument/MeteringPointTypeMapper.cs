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

using Energinet.DataHub.TimeSeries.Application.Enums;

namespace Energinet.DataHub.TimeSeries.Application.Cim.MarketDocument
{
    public static class MeteringPointTypeMapper
    {
        private const string CimConsumption = "E17";
        private const string CimProduction = "E18";
        private const string CimExchange = "E20";
        private const string CimVeProduction = "D01";
        private const string CimAnalysis = "D02";
        private const string CimSurplusProductionGroup = "D04";
        private const string CimNetProduction = "D05";
        private const string CimSupplyToGrid = "D06";
        private const string CimConsumptionFromGrid = "D07";
        private const string CimWholesaleService = "D08";
        private const string CimOwnProduction = "D09";
        private const string CimNetFromGrid = "D10";
        private const string CimNetToGrid = "D11";
        private const string CimTotalConsumption = "D12";
        private const string CimGridLossCorrection = "D13";
        private const string CimElectricalHeating = "D14";
        private const string CimNetConsumption = "D15";
        private const string CimOtherConsumption = "D17";
        private const string CimOtherProduction = "D18";
        private const string CimExchangeReactiveEnergy = "D20";
        private const string CimInternalUse = "D99";

        public static MeteringPointType Map(string value)
        {
            return value switch
            {
                CimConsumption => MeteringPointType.Consumption,
                CimProduction => MeteringPointType.Production,
                CimExchange => MeteringPointType.Exchange,
                CimVeProduction => MeteringPointType.VeProduction,
                CimAnalysis => MeteringPointType.Analysis,
                CimSurplusProductionGroup => MeteringPointType.SurplusProductionGroup,
                CimNetProduction => MeteringPointType.NetProduction,
                CimSupplyToGrid => MeteringPointType.SupplyToGrid,
                CimConsumptionFromGrid => MeteringPointType.ConsumptionFromGrid,
                CimWholesaleService => MeteringPointType.WholesaleService,
                CimOwnProduction => MeteringPointType.OwnProduction,
                CimNetFromGrid => MeteringPointType.NetFromGrid,
                CimNetToGrid => MeteringPointType.NetToGrid,
                CimTotalConsumption => MeteringPointType.TotalConsumption,
                CimGridLossCorrection => MeteringPointType.GridLossCorrection,
                CimElectricalHeating => MeteringPointType.ElectricalHeating,
                CimNetConsumption => MeteringPointType.NetConsumption,
                CimOtherConsumption => MeteringPointType.OtherConsumption,
                CimOtherProduction => MeteringPointType.OtherProduction,
                CimExchangeReactiveEnergy => MeteringPointType.ExchangeReactiveEnergy,
                CimInternalUse => MeteringPointType.InternalUse,
                _ => MeteringPointType.Unknown,
            };
        }
    }
}
