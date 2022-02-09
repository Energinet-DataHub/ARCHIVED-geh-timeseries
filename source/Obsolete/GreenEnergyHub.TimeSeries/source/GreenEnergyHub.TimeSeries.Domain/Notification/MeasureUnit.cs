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

namespace GreenEnergyHub.TimeSeries.Domain.Notification
{
    public enum MeasureUnit
    {
        Unknown = 0,
        KiloWattHour = 1,
        MegaWattHour = 2,
        KiloWatt = 3,
        MegaWatt = 4,
        // A question has been raised to the SME group about the relevancy of the enums below in the time series domain. For now they remain here.
        KiloVarHour = 5,
        MegaVar = 6,
        Tariff = 7,
        Tonne = 8,
    }
}
