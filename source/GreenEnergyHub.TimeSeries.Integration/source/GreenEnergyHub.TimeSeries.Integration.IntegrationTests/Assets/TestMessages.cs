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

using System;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.MeteringPoints.IntegrationEventContracts;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

namespace GreenEnergyHub.TimeSeries.Integration.IntegrationTests.Assets
{
    public static class TestMessages
    {
        public static ServiceBusMessage CreateMpCreatedMessage()
        {
            var message = new ConsumptionMeteringPointCreated
            {
                Product = ConsumptionMeteringPointCreated.Types.ProductType.PtEnergyactive,
                ConnectionState = ConsumptionMeteringPointCreated.Types.ConnectionState.CsNew,
                GsrnNumber = "1234",
                MeteringMethod = ConsumptionMeteringPointCreated.Types.MeteringMethod.MmPhysical,
                SettlementMethod = ConsumptionMeteringPointCreated.Types.SettlementMethod.SmFlex,
                UnitType = ConsumptionMeteringPointCreated.Types.UnitType.UtKwh,
                GridAreaCode = "500",
                MeteringPointId = "1",
                MeterReadingPeriodicity = ConsumptionMeteringPointCreated.Types.MeterReadingPeriodicity.MrpHourly,
                NetSettlementGroup = ConsumptionMeteringPointCreated.Types.NetSettlementGroup.NsgOne,
                EffectiveDate = Timestamp.FromDateTime(DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc)),
            };

            var serviceBusMessage = new ServiceBusMessage(message.ToByteArray());
            serviceBusMessage.ApplicationProperties.Add("MessageType", "ConsumptionMeteringPointCreated");
            return serviceBusMessage;
        }
    }
}
