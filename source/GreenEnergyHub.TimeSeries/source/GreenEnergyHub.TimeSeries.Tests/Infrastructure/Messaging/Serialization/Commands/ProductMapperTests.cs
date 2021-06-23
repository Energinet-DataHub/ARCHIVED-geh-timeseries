using GreenEnergyHub.TimeSeries.Domain.Notification;
using GreenEnergyHub.TimeSeries.Infrastructure.Messaging.Serialization.Commands;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.TimeSeries.Tests.Infrastructure.Messaging.Serialization.Commands
{
    [UnitTest]
    public class ProductMapperTests
    {
        [Theory]
        [InlineData("8716867000030", Product.EnergyActive)]
        [InlineData("8716867000047", Product.EnergyReactive)]
        [InlineData("5790001330606", Product.FuelQuantity)]
        [InlineData("8716867000016", Product.PowerActive)]
        [InlineData("8716867000023", Product.PowerReactive)]
        [InlineData("5790001330590", Product.Tariff)]
        [InlineData("", Product.Unknown)]
        [InlineData("DoesNotExist", Product.Unknown)]
        public void Map_WhenGivenInput_MapsToCorrectEnum(string input, Product expected)
        {
            var actual = ProductMapper.Map(input);
            Assert.Equal(actual, expected);
        }
    }
}
