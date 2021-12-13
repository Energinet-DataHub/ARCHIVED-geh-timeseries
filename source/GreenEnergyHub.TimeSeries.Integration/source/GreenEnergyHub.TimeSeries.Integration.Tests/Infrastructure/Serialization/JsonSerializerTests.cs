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
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using GreenEnergyHub.TimeSeries.Integration.Application.IntegrationEvents.MeteringPoints;
using GreenEnergyHub.TimeSeries.Integration.Domain;
using GreenEnergyHub.TimeSeries.Integration.Infrastructure.Serialization;
using GreenEnergyHub.TimeSeries.Integration.IntegrationEventListener.Common;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.TimeSeries.Integration.Tests.Infrastructure.Serialization
{
    [UnitTest]
    public class JsonSerializerTests
    {
        [Fact]
        public void SerializeString_ValidString_ReturnsConvertedValues()
        {
            // Arrange
            var sut = new JsonSerializer();
            var message = new ConsumptionMeteringPointCreatedEvent(
                MeteringPointId: "MeteringPointId",
                MeteringPointType: MeteringPointType.Consumption,
                GridArea: "GridArea",
                SettlementMethod: SettlementMethod.Flex,
                MeteringMethod: MeteringMethod.Physical,
                Resolution: Resolution.Hourly,
                Product: Product.EnergyActive,
                ConnectionState: ConnectionState.New,
                Unit: Unit.Kwh,
                EffectiveDate: Instant.FromUnixTimeSeconds(1000));

            // Act
            var actual = sut.Serialize(message);
            var jsonDictionary = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(actual);

            // Assert
            Assert.NotNull(jsonDictionary);
            Assert.Equal("MeteringPointId", jsonDictionary["metering_point_id"].ToString());
            Assert.Equal("E17", jsonDictionary["metering_point_type"].ToString());
            Assert.Equal("GridArea", jsonDictionary["grid_area"].ToString());
            Assert.Equal("D01", jsonDictionary["settlement_method"].ToString());
            Assert.Equal("D01", jsonDictionary["metering_method"].ToString());
            Assert.Equal("PT1H", jsonDictionary["resolution"].ToString());
            Assert.Equal("8716867000030", jsonDictionary["product"].ToString());
            Assert.Equal("D03", jsonDictionary["connection_state"].ToString());
            Assert.Equal("KWH", jsonDictionary["unit"].ToString());
            Assert.Equal("1970-01-01T00:16:40Z", jsonDictionary["effective_date"].ToString());
        }

        [Fact]
        public void SerializeString_StringIsNull_ThrowsException()
        {
            var sut = new JsonSerializer();
            Assert.Throws<ArgumentNullException>(() => sut.Serialize((string)null));
        }

        [Fact]
        public void DeserializeString_ValidJson_ReturnsCorrectValues()
        {
            // Arrange
            const string jsonString = "{\"MessageVersion\": 1, \"MessageType\": \"ConsumptionMeteringPointCreated\", \"EventIdentification\": \"1234\", \"OperationTimestamp\": \"2021-10-01T10:00:00Z\", \"OperationCorrelationId\": \"5678\"}";
            var expected = new EventMetadata(1, "ConsumptionMeteringPointCreated", "1234", Instant.FromDateTimeUtc(new DateTime(2021, 10, 01, 10, 0, 0).ToUniversalTime()), "5678");
            var sut = new JsonSerializer();

            // Act
            var actual = sut.Deserialize<EventMetadata>(jsonString);

            // Assert
            Assert.NotNull(actual);
            Assert.Equal(expected.MessageVersion, actual.MessageVersion);
            Assert.Equal(expected.MessageType, actual.MessageType);
        }

        [Fact]
        public void Deserialize_JsonStringIsNull_ThrowsException()
        {
            var sut = new JsonSerializer();
            Assert.Throws<ArgumentNullException>(() => sut.Deserialize<EventMetadata>(null!));
        }

        [Fact]
        public void Deserialize_CustomType_ReturnsCorrectTypeAndValues()
        {
            // Arrange
            const string jsonString = "{\"MessageVersion\": 1, \"MessageType\": \"ConsumptionMeteringPointCreated\", \"EventIdentification\": \"1234\", \"OperationTimestamp\": \"2021-10-01T10:00:00Z\", \"OperationCorrelationId\": \"5678\"}";
            var expected = new EventMetadata(1, "ConsumptionMeteringPointCreated", "1234", Instant.FromDateTimeUtc(new DateTime(2021, 10, 01, 10, 0, 0).ToUniversalTime()), "5678");
            var sut = new JsonSerializer();

            // Act
            var actualObject = sut.Deserialize(jsonString, typeof(EventMetadata));

            // Assert
            Assert.IsType<EventMetadata>(actualObject);

            var actual = actualObject as EventMetadata;
            Assert.NotNull(actual);
            Assert.Equal(expected.MessageVersion, actual.MessageVersion);
            Assert.Equal(expected.MessageType, actual.MessageType);
        }

        [Fact]
        public void Deserialize_CustomObjectIsNull_ThrowsException()
        {
            var sut = new JsonSerializer();
            Assert.Throws<ArgumentNullException>(() => sut.Deserialize(null!, typeof(EventMetadata)));
        }

        [Fact]
        public async Task DeserializeAsync_ValidStream_ReturnsCorrectValues()
        {
            const string jsonString = "{\"MessageVersion\": 1, \"MessageType\": \"ConsumptionMeteringPointCreated\", \"EventIdentification\": \"1234\", \"OperationTimestamp\": \"2021-10-01T10:00:00Z\", \"OperationCorrelationId\": \"5678\"}";
            var expected = new EventMetadata(1, "ConsumptionMeteringPointCreated", "1234", Instant.FromDateTimeUtc(new DateTime(2021, 10, 01, 10, 0, 0).ToUniversalTime()), "5678");
            await using var jsonStream = new MemoryStream(Encoding.UTF8.GetBytes(s: jsonString));
            var sut = new JsonSerializer();

            // Act
            var actualObject = await sut.DeserializeAsync(jsonStream, typeof(EventMetadata));

            // Assert
            Assert.IsType<EventMetadata>(actualObject);

            var actual = actualObject as EventMetadata;
            Assert.NotNull(actual);
            Assert.Equal(expected.MessageVersion, actual.MessageVersion);
            Assert.Equal(expected.MessageType, actual.MessageType);
        }

        [Fact]
        public async Task DeserializeAsync_StreamIsNull_ThrowsException()
        {
            var sut = new JsonSerializer();
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await sut.DeserializeAsync(null!, typeof(EventMetadata)));
        }
    }
}
