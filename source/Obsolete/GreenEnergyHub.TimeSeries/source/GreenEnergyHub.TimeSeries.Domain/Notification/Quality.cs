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
    public enum Quality
    {
        Unknown = 0,
        Measured = 1, // Received as E01 in ebIX
        Revised = 2, // Received as 36 in ebIX
        Estimated = 3, // Received as 56 in ebIX
        QuantityMissing = 4, // Received in separate boolean field in ebIX / Received as A02 in CIM
        Calculated = 5, // Received as D01 in ebIX
    }
}
