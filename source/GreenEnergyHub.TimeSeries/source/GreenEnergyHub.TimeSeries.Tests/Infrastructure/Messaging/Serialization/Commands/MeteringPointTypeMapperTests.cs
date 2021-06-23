using GreenEnergyHub.TimeSeries.Domain.Notification;
using GreenEnergyHub.TimeSeries.Infrastructure.Messaging.Serialization.Commands;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.TimeSeries.Tests.Infrastructure.Messaging.Serialization.Commands
{
    [UnitTest]
    public class MeteringPointTypeMapperTests
    {
        [Theory]
        [InlineData("E17", MeteringPointType.Consumption)]
        [InlineData("E18", MeteringPointType.Production)]
        [InlineData("E20", MeteringPointType.Exchange)]
        [InlineData("D01", MeteringPointType.VeProduction)]
        [InlineData("D02", MeteringPointType.Analysis)]
        [InlineData("D04", MeteringPointType.SurplusProductionGroup)]
        [InlineData("D05", MeteringPointType.NetProduction)]
        [InlineData("D06", MeteringPointType.SupplyToGrid)]
        [InlineData("D07", MeteringPointType.ConsumptionFromGrid)]
        [InlineData("D08", MeteringPointType.WholesaleService)]
        [InlineData("D09", MeteringPointType.OwnProduction)]
        [InlineData("D10", MeteringPointType.NetFromGrid)]
        [InlineData("D11", MeteringPointType.NetToGrid)]
        [InlineData("D12", MeteringPointType.TotalConsumption)]
        [InlineData("D13", MeteringPointType.GridLossCorrection)]
        [InlineData("D14", MeteringPointType.ElectricalHeating)]
        [InlineData("D15", MeteringPointType.NetConsumption)]
        [InlineData("D17", MeteringPointType.OtherConsumption)]
        [InlineData("D18", MeteringPointType.OtherProduction)]
        [InlineData("D20", MeteringPointType.ExchangeReactiveEnergy)]
        [InlineData("D99", MeteringPointType.InternalUse)]
        [InlineData("", MeteringPointType.Unknown)]
        [InlineData("DoesNotExist", MeteringPointType.Unknown)]
        public void Map_WhenGivenInput_MapsToCorrectEnum(string input, MeteringPointType expected)
        {
            var actual = MeteringPointTypeMapper.Map(input);
            Assert.Equal(actual, expected);
        }
    }
}
