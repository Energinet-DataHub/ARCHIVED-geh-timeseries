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

using NodaTime;

#pragma warning disable 8618

namespace GreenEnergyHub.TimeSeries.Domain.MarketDocument
{
    // Non-nullable member is uninitialized is ignored
    // Only properties which is allowed to be null is nullable
    public class Document
    {
        /// <summary>
        /// An ID provided by the sender.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        ///  Point in time set by the TimeSeries domain
        /// </summary>
        public Instant RequestDateTime { get; set; } = SystemClock.Instance.GetCurrentInstant();

        /// <summary>
        /// A point in time provided by the sender
        /// </summary>
        public Instant CreatedDateTime { get; set; }

        public MarketParticipant Sender { get; set; }

        public MarketParticipant Recipient { get; set; }

        public BusinessReasonCode BusinessReasonCode { get; set; }
    }
}
