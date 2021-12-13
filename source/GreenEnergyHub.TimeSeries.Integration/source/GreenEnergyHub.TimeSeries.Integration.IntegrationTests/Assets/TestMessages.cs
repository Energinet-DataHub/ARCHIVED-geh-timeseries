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
            var date = new DateTime(2021, 1, 2, 3, 4, 5, DateTimeKind.Utc);
            var timestamp = Timestamp.FromDateTime(DateTime.SpecifyKind(date, DateTimeKind.Utc));
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
                EffectiveDate = timestamp,
            };

            var serviceBusMessage = new ServiceBusMessage(message.ToByteArray());
            serviceBusMessage.ApplicationProperties.Add("OperationTimestamp", date.ToUniversalTime());
            serviceBusMessage.ApplicationProperties.Add("OperationCorrelationId", "1bf1b76337f14b78badc248a3289d021");
            serviceBusMessage.ApplicationProperties.Add("MessageVersion", 1);
            serviceBusMessage.ApplicationProperties.Add("MessageType", "ConsumptionMeteringPointCreated");
            serviceBusMessage.ApplicationProperties.Add("EventIdentification", "2542ed0d242e46b68b8b803e93ffbf7b");
            return serviceBusMessage;
        }
    }
}
