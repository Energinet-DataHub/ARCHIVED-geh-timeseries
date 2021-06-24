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
        [InlineData(null, MeteringPointType.Unknown)]
        public void Map_WhenGivenInput_MapsToCorrectEnum(string input, MeteringPointType expected)
        {
            var actual = MeteringPointTypeMapper.Map(input);
            Assert.Equal(actual, expected);
        }
    }
}
