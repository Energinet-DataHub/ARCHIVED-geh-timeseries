using GreenEnergyHub.TimeSeries.Domain.Notification;
using GreenEnergyHub.TimeSeries.Infrastructure.Messaging.Serialization.Commands;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.TimeSeries.Tests.Infrastructure.Messaging.Serialization.Commands
{
    [UnitTest]
    public class TimeSeriesResolutionMapperTests
    {
        [Theory]
        [InlineData("P1D", TimeSeriesResolution.Day)]
        [InlineData("PT1H", TimeSeriesResolution.Hour)]
        [InlineData("P1M", TimeSeriesResolution.Month)]
        [InlineData("PT15M", TimeSeriesResolution.QuarterOfHour)]
        [InlineData("", TimeSeriesResolution.Unknown)]
        [InlineData("DoesNotExist", TimeSeriesResolution.Unknown)]
        public void Map_WhenGivenInput_MapsToCorrectEnum(string input, TimeSeriesResolution expected)
        {
            var actual = TimeSeriesResolutionMapper.Map(input);
            Assert.Equal(actual, expected);
        }
    }
}
