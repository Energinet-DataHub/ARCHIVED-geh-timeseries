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
    public class SettlementMethodMapperTests
    {
        [Theory]
        [InlineData("D01", SettlementMethod.Flex)]
        [InlineData("E02", SettlementMethod.NonProfiled)]
        [InlineData("E01", SettlementMethod.Profiled)]
        [InlineData("", SettlementMethod.Unknown)]
        [InlineData("DoesNotExist", SettlementMethod.Unknown)]
        [InlineData(null, SettlementMethod.Unknown)]
        public void Map_WhenGivenInput_ToCorrectEnum(string input, SettlementMethod expected)
        {
            var actual = SettlementMethodMapper.Map(input);
            Assert.Equal(actual, expected);
        }
    }
}
