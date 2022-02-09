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
using System.Text.Json;
using GreenEnergyHub.TimeSeries.Integration.Domain;
using GreenEnergyHub.TimeSeries.Integration.Infrastructure.Serialization.Converters;
using GreenEnergyHub.TimeSeries.Integration.Tests.Attributes;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.TimeSeries.Integration.Tests.Infrastructure.Serialization.Converters
{
    [UnitTest]
    public static class SettlementMethodConverterTests
    {
        [Theory]
        [InlineAutoMoqData(@"""D01""", SettlementMethod.Flex)]
        [InlineAutoMoqData(@"""E01""", SettlementMethod.Profiled)]
        [InlineAutoMoqData(@"""E02""", SettlementMethod.NonProfiled)]
        public static void Read_ValidStrings_ReturnsCorrectType(
            string json,
            SettlementMethod expected,
            [NotNull] JsonSerializerOptions options,
            SettlementMethodConverter sut)
        {
            // Arrange
            options.Converters.Add(sut);

            // Act
            var actual = JsonSerializer.Deserialize<SettlementMethod>(json, options);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public static void Read_UnknownString_ThrowsException()
        {
            // Arrange
            const string json = @"""Unknown""";
            var options = new JsonSerializerOptions();
            var sut = new SettlementMethodConverter();
            options.Converters.Add(sut);

            // Act
            Assert.Throws<ArgumentException>(() => JsonSerializer.Deserialize<SettlementMethod>(json, options));
        }

        [Theory]
        [InlineAutoMoqData(@"""D01""", SettlementMethod.Flex)]
        [InlineAutoMoqData(@"""E01""", SettlementMethod.Profiled)]
        [InlineAutoMoqData(@"""E02""", SettlementMethod.NonProfiled)]
        public static void Write_ValidValue_ReturnsCorrectString(
            string expected,
            SettlementMethod settlementMethod,
            [NotNull] JsonSerializerOptions options,
            SettlementMethodConverter sut)
        {
            // Arrange
            options.Converters.Add(sut);

            // Act
            var actual = JsonSerializer.Serialize(settlementMethod, options);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public static void Write_UnknownValue_ThrowsException()
        {
            // Arrange
            const SettlementMethod settlementMethod = (SettlementMethod)999;
            var options = new JsonSerializerOptions();
            var sut = new SettlementMethodConverter();
            options.Converters.Add(sut);

            // Act
            Assert.Throws<ArgumentOutOfRangeException>(() => JsonSerializer.Serialize(settlementMethod, options));
        }
    }
}
