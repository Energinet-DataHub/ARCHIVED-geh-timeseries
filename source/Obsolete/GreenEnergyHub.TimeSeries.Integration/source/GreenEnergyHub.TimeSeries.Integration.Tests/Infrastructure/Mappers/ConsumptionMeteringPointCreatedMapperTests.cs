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
using System.Diagnostics.CodeAnalysis;
using Energinet.DataHub.MeteringPoints.IntegrationEventContracts;
using Google.Protobuf.WellKnownTypes;
using GreenEnergyHub.TimeSeries.Integration.Application.IntegrationEvents.MeteringPoints;
using GreenEnergyHub.TimeSeries.Integration.Domain;
using GreenEnergyHub.TimeSeries.Integration.Infrastructure.Mappers;
using GreenEnergyHub.TimeSeries.Integration.Tests.Attributes;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.TimeSeries.Integration.Tests.Infrastructure.Mappers
{
    [UnitTest]
    public class ConsumptionMeteringPointCreatedMapperTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void Convert_WhenCalled_ShouldMapProtobufToInbound(
            [NotNull] ConsumptionMeteringPointCreated protobufMessage,
            [NotNull] ConsumptionMeteringPointCreatedMapper sut)
        {
            // Arrange
            protobufMessage.SettlementMethod = ConsumptionMeteringPointCreated.Types.SettlementMethod.SmFlex;
            protobufMessage.MeteringMethod = ConsumptionMeteringPointCreated.Types.MeteringMethod.MmPhysical;
            protobufMessage.MeterReadingPeriodicity = ConsumptionMeteringPointCreated.Types.MeterReadingPeriodicity.MrpHourly;
            protobufMessage.ConnectionState = ConsumptionMeteringPointCreated.Types.ConnectionState.CsNew;
            protobufMessage.Product = ConsumptionMeteringPointCreated.Types.ProductType.PtEnergyactive;
            protobufMessage.UnitType = ConsumptionMeteringPointCreated.Types.UnitType.UtKwh;
            protobufMessage.EffectiveDate = Timestamp.FromDateTime(new DateTime(2021, 10, 31, 23, 00, 00, 00, DateTimeKind.Utc));

            // Act
            var result = sut.Convert(protobufMessage) as ConsumptionMeteringPointCreatedEvent;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(protobufMessage.GsrnNumber, result.MeteringPointId);
            Assert.Equal(MeteringPointType.Consumption, result.MeteringPointType);
            Assert.Equal(protobufMessage.GridAreaCode, result.GridArea);
            Assert.Equal(SettlementMethod.Flex, result.SettlementMethod);
            Assert.Equal(MeteringMethod.Physical, result.MeteringMethod);
            Assert.Equal(Resolution.Hourly, result.Resolution);
            Assert.Equal(ConnectionState.New, result.ConnectionState);
            Assert.Equal(Product.EnergyActive, result.Product);
            Assert.Equal(Unit.Kwh, result.Unit);
            Assert.Equal(protobufMessage.EffectiveDate.Seconds, result.EffectiveDate.ToUnixTimeSeconds());
        }
    }
}
