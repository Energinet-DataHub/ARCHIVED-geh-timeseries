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

#pragma warning disable 8618
namespace GreenEnergyHub.TimeSeries.Domain.Common
{
    // Non-nullable member is uninitialized is ignored
    // Only properties which is allowed to be null is nullable

    /// <summary>
    /// A market participant, e.g. a metered data responsible, whom may submit a time series.
    /// </summary>
    public class MarketParticipant
    {
        /// <summary>
        /// Contains an ID that identifies the Market Participants. In Denmark this would be the GLN number or EIC code.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Contains the role a market participant uses when initiating and communicating with Green Energy Hub
        /// about a specific business process, e.g. Metered Data Responsible use 'MDR' when sending a time series.
        /// </summary>
        public MarketParticipantRole BusinessProcessRole { get; set; }
    }
}
