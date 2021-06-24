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
        [InlineData(null, Product.Unknown)]
        public void Map_WhenGivenInput_MapsToCorrectEnum(string input, Product expected)
        {
            var actual = ProductMapper.Map(input);
            Assert.Equal(actual, expected);
        }
    }
}
