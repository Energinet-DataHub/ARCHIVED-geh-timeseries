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

using System.Collections.Generic;
using Energinet.DataHub.Core.Messaging.MessageTypes.Common;
using Energinet.DataHub.Core.Messaging.Transport;
using NodaTime;

namespace Energinet.DataHub.TimeSeries.Application.Dtos
{
    public class TimeSeriesBundleDto : IInboundMessage
    {
        public Document Document { get; set; }

        public IEnumerable<Series> Series { get; set; }

        public Transaction Transaction { get; set; }
    }

    public class Series
    {
    }

    public class Document
    {
        public string Id { get; set; }

        public Instant CreatedDateTime { get; set; }

        public MarketParticipant Sender { get; set; }

        public MarketParticipant Receiver { get; set; }

        /// <summary>
        /// Process type in CIM XML
        /// </summary>
        public BusinessReasonCode BusinessReasonCode { get; set; }
    }

    public class MarketParticipant
    {
        public string Id { get; set; }

        public MarketParticipantRole BusinessProcessRole { get; set; }
    }

    public enum MarketParticipantRole
    {
        Unknown = 0,
        EnergySupplier = 1,
        GridAccessProvider = 2,
        SystemOperator = 3,
        MeteredDataResponsible = 4,
        EnergyAgency = 5,
        MeteredDataAdministrator = 6,
    }

    public enum BusinessReasonCode
    {
        Unknown = 0,
        ContinuousMeterReading = 1,
        PeriodicMetering = 2,
        HistoricalData = 3,
        PeriodicFlexMetering = 4,
    }
}
