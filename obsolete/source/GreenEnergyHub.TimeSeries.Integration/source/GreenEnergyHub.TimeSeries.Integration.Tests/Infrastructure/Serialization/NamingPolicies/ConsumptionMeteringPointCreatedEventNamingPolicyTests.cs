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
using GreenEnergyHub.TimeSeries.Integration.Infrastructure.Serialization.NamingPolicies;
using GreenEnergyHub.TimeSeries.Integration.Tests.Attributes;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.TimeSeries.Integration.Tests.Infrastructure.Serialization.NamingPolicies
{
    [UnitTest]
    public class ConsumptionMeteringPointCreatedEventNamingPolicyTests
    {
        [Theory]
        [InlineAutoMoqData("MeteringPointId", "metering_point_id")]
        [InlineAutoMoqData("MeteringPointType", "metering_point_type")]
        [InlineAutoMoqData("GridArea", "grid_area")]
        [InlineAutoMoqData("SettlementMethod", "settlement_method")]
        [InlineAutoMoqData("MeteringMethod", "metering_method")]
        [InlineAutoMoqData("Resolution", "resolution")]
        [InlineAutoMoqData("Product", "product")]
        [InlineAutoMoqData("ConnectionState", "connection_state")]
        [InlineAutoMoqData("Unit", "unit")]
        [InlineAutoMoqData("EffectiveDate", "effective_date")]
        [InlineAutoMoqData("MessageVersion", "MessageVersion")]
        [InlineAutoMoqData("MessageType", "MessageType")]
        [InlineAutoMoqData("Transaction", "Transaction")]
        public void ConvertName_KnownProperty_ReturnsCorrectPropertyName(
            string propertyName,
            string expected,
            [NotNull] ConsumptionMeteringPointCreatedEventNamingPolicy sut)
        {
            // Act
            var actual = sut.ConvertName(propertyName);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ConvertName_UnknownProperty_ThrowsException()
        {
            // Arrange
            var sut = new ConsumptionMeteringPointCreatedEventNamingPolicy();

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => sut.ConvertName("UnknownProperty"));
        }
    }
}
