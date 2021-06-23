using GreenEnergyHub.TimeSeries.Domain.Notification;
using GreenEnergyHub.TimeSeries.Infrastructure.Messaging.Serialization.Commands;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.TimeSeries.Tests.Infrastructure.Messaging.Serialization.Commands
{
    [UnitTest]
    public class QuantityQualityMapperTests
    {
        [Theory]
        [InlineData("D01", QuantityQuality.Calculated)]
        [InlineData("56", QuantityQuality.Estimated)]
        [InlineData("E01", QuantityQuality.Measured)]
        [InlineData("D99", QuantityQuality.QuantityMissing)]
        [InlineData("36", QuantityQuality.Revised)]
        [InlineData("", QuantityQuality.Unknown)]
        [InlineData("DoesNotExist", QuantityQuality.Unknown)]
        public void Map_WhenGivenInput_MapsToCorrectEnum(string input, QuantityQuality expected)
        {
            var actual = QuantityQualityMapper.Map(input);
            Assert.Equal(actual, expected);
        }
    }
}
