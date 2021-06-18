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

namespace GreenEnergyHub.TimeSeries.Infrastructure.Messaging.Serialization.Common
{
    internal class DocumentConverterConstants
    {
        internal const string Id = "mRID";

        internal const string Type = "type";

        internal const string BusinessReasonCode = "process.processType";

        internal const string IndustryClassification = "businessSector.type";

        internal const string SenderId = "sender_MarketParticipant.mRID";

        internal const string SenderBusinessProcessRole = "sender_MarketParticipant.marketRole.type";

        internal const string RecipientId = "receiver_MarketParticipant.mRID";

        internal const string RecipientBusinessProcessRole = "receiver_MarketParticipant.marketRole.type";
    }
}
