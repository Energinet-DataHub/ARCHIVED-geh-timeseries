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
using System.Globalization;
using Energinet.DataHub.TimeSeries.Infrastructure.Serialization;
using FluentAssertions;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.TimeSeries.UnitTests.Infrastructure
{
    public class JsonSerializerTests
    {
        [Fact]
        public void When_InstantIsSerialized_Then_DateTimeIsPresentInJsonObject()
        {
            // Arrange
            var dateTime = new DateTime(2022, 01, 03).ToUniversalTime();
            var instant = Instant.FromDateTimeUtc(dateTime);
            var expectedDateTime = $"\"{dateTime.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture)}\"";

            var sut = new JsonSerializer();

            // Act
            var jsonObject = sut.Serialize(instant);

            // Assert
            jsonObject.Should().BeEquivalentTo(expectedDateTime);
        }
    }
}
