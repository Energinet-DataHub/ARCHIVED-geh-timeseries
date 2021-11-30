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
using Energinet.DataHub.MarketRoles.IntegrationEventContracts;
using Google.Protobuf.WellKnownTypes;
using GreenEnergyHub.TimeSeries.Integration.Application.IntegrationEvents.MeteringPoints;
using GreenEnergyHub.TimeSeries.Integration.Infrastructure.Mappers;
using GreenEnergyHub.TimeSeries.Integration.Tests.Attributes;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.TimeSeries.Integration.Tests.Infrastructure.Mappers
{
    [UnitTest]
    public class EnergySupplierChangedMapperTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void Convert_WhenCalled_ShouldMapProtobufToInbound(
            [NotNull] EnergySupplierChanged protobufMessage,
            [NotNull] EnergySupplierChangedMapper sut)
        {
            // Arrange
            protobufMessage.EffectiveDate = Timestamp.FromDateTime(new DateTime(2021, 10, 31, 23, 00, 00, 00, DateTimeKind.Utc));

            // Act
            var result = sut.Convert(protobufMessage) as EnergySupplierChangedEvent;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(protobufMessage.AccountingpointId, result.AccountingPointId);
            Assert.Equal(protobufMessage.GsrnNumber, result.GsrnNumber);
            Assert.Equal(protobufMessage.EnergySupplierGln, result.EnergySupplierGln);
            Assert.Equal(protobufMessage.EffectiveDate.Seconds, result.EffectiveDate.ToUnixTimeSeconds());
        }
    }
}
