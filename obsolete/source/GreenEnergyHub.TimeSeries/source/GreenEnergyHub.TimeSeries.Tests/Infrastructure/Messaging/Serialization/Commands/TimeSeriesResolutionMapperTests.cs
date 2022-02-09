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
    public class TimeSeriesResolutionMapperTests
    {
        [Theory]
        [InlineData("P1D", TimeSeriesResolution.Day)]
        [InlineData("PT1H", TimeSeriesResolution.Hour)]
        [InlineData("P1M", TimeSeriesResolution.Month)]
        [InlineData("PT15M", TimeSeriesResolution.QuarterOfHour)]
        [InlineData("", TimeSeriesResolution.Unknown)]
        [InlineData("DoesNotExist", TimeSeriesResolution.Unknown)]
        [InlineData(null, TimeSeriesResolution.Unknown)]
        public void Map_WhenGivenStringInput_MapsToCorrectEnum(string input, TimeSeriesResolution expected)
        {
            var actual = TimeSeriesResolutionMapper.Map(input);
            Assert.Equal(actual, expected);
        }

        [Theory]
        [InlineData(TimeSeriesResolution.Day, "P1D")]
        [InlineData(TimeSeriesResolution.Hour, "PT1H")]
        [InlineData(TimeSeriesResolution.Month, "P1M")]
        [InlineData(TimeSeriesResolution.QuarterOfHour, "PT15M")]
        [InlineData(TimeSeriesResolution.Unknown, "")]
        public void Map_WhenGivenEnumInput_MapsToCorrectString(TimeSeriesResolution input, string expected)
        {
            var actual = TimeSeriesResolutionMapper.Map(input);
            Assert.Equal(actual, expected);
        }
    }
}
