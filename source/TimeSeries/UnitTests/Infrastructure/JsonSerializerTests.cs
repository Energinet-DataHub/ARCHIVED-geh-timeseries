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
