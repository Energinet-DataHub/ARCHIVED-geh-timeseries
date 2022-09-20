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
    public static class QualityMapper
    {
        private const string CimMissing = "A02";
        private const string CimEstimated = "A03";
        private const string CimMeasured = "A04";
        private const string CimCalculated = "Z01";

        public static Quality Map(string value)
        {
            return value switch
            {
                CimMissing => Quality.Missing,
                CimEstimated => Quality.Estimated,
                CimMeasured => Quality.Measured,
                CimCalculated => Quality.Calculated,
                _ => Quality.Invalid,
            };
        }
    }
}
