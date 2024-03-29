﻿// Copyright 2020 Energinet DataHub A/S
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
using NodaTime;

namespace Energinet.DataHub.TimeSeries.Application.Dtos
{
    public class DocumentDto
    {
        public DocumentDto()
        {
            Sender = new MarketParticipantDto();
            Receiver = new MarketParticipantDto();

            // Cim deserialization will ensure a value is set.
            Id = null!;
        }

        public string Id { get; set; }

        public Instant CreatedDateTime { get; set; }

        public MarketParticipantDto Sender { get; set; }

        public MarketParticipantDto Receiver { get; set; }

        /// <summary>
        /// Process type in CIM XML
        /// </summary>
        public BusinessReasonCode BusinessReasonCode { get; set; }
    }
}
