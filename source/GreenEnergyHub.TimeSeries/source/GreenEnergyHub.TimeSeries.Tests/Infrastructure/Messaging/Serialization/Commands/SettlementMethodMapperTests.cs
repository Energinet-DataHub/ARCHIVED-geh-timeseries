using GreenEnergyHub.TimeSeries.Domain.Notification;
using GreenEnergyHub.TimeSeries.Infrastructure.Messaging.Serialization.Commands;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.TimeSeries.Tests.Infrastructure.Messaging.Serialization.Commands
{
    [UnitTest]
    public class SettlementMethodMapperTests
    {
        [Theory]
        [InlineData("D01", SettlementMethod.Flex)]
        [InlineData("E02", SettlementMethod.NonProfiled)]
        [InlineData("E01", SettlementMethod.Profiled)]
        [InlineData("", SettlementMethod.Unknown)]
        [InlineData("DoesNotExist", SettlementMethod.Unknown)]
        public void Map_WhenGivenInput_ToCorrectEnum(string input, SettlementMethod expected)
        {
            var actual = SettlementMethodMapper.Map(input);
            Assert.Equal(actual, expected);
        }
    }
}
