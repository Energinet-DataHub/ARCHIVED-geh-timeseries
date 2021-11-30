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

using Energinet.DataHub.Core.Messaging.MessageTypes.Common;
using Energinet.DataHub.Core.Messaging.Transport;
using GreenEnergyHub.TimeSeries.Integration.Domain;
using NodaTime;

namespace GreenEnergyHub.TimeSeries.Integration.Application.IntegrationEvents.MeteringPoints
{
#pragma warning disable SA1313
    public record ConsumptionMeteringPointCreatedEvent(
            string MeteringPointId,
            MeteringPointType MeteringPointType,
            string GridArea,
            SettlementMethod SettlementMethod,
            MeteringMethod MeteringMethod,
            Resolution Resolution,
            Product Product,
            ConnectionState ConnectionState,
            Unit Unit,
            Instant EffectiveDate)
            : IInboundMessage
    {
        public Transaction Transaction { get; set; } = new ();
    }
#pragma warning restore SA1313
}
