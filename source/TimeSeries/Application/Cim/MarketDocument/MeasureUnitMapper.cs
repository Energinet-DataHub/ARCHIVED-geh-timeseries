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
    public static class MeasureUnitMapper
    {
        private const string CimKiloWattHour = "KWH";
        private const string CimMegaWattHour = "MWH";
        private const string CimKiloWatt = "KWT";
        private const string CimMegaWatt = "MAW";
        private const string CimKiloVarHour = "K3";
        private const string CimMegaVar = "Z03";
        private const string CimTariff = "Z14";
        private const string CimTonne = "TNE";
        private const string CimAmpere = "AMP";
        private const string CimStk = "H87";

        public static MeasureUnit Map(string value)
        {
            return value switch
            {
                CimKiloWattHour => MeasureUnit.KiloWattHour,
                CimMegaWattHour => MeasureUnit.MegaWattHour,
                CimKiloWatt => MeasureUnit.KiloWatt,
                CimMegaWatt => MeasureUnit.MegaWatt,
                CimKiloVarHour => MeasureUnit.KiloVarHour,
                CimMegaVar => MeasureUnit.MegaVar,
                CimTariff => MeasureUnit.Tariff,
                CimTonne => MeasureUnit.Tonne,
                CimAmpere => MeasureUnit.Ampere,
                CimStk => MeasureUnit.Stk,
                _ => MeasureUnit.Unknown,
            };
        }
    }
}
