using System;
using System.Collections.Generic;
using System.Text;
using GreenEnergyHub.TimeSeries.Domain.Notification;
using GreenEnergyHub.TimeSeries.Infrastructure.Messaging.Serialization.Commands;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.TimeSeries.Tests.Infrastructure.Messaging.Serialization.Commands
{
    [UnitTest]
    public class MeasureUnitMapperTests
    {
        [Theory]
        [InlineData("KWH", MeasureUnit.KiloWattHour)]
        [InlineData("MWH", MeasureUnit.MegaWattHour)]
        [InlineData("KWT", MeasureUnit.KiloWatt)]
        [InlineData("MAW", MeasureUnit.MegaWattHour)]
        [InlineData("K3", MeasureUnit.KiloVarHour)]
        [InlineData("Z03", MeasureUnit.MegaVar)]
        [InlineData("Z14", MeasureUnit.Tariff)]
        [InlineData("TNE", MeasureUnit.Tonne)]
        [InlineData("", MeasureUnit.Unknown)]
        [InlineData("DoesNotExist", MeasureUnit.Unknown)]
        public void Map_WhenGivenInput_MapsToCorrectEnum(string unit, MeasureUnit expected)
        {
            var actual = MeasureUnitMapper.Map(unit);
            Assert.Equal(actual, expected);
        }
    }
}
