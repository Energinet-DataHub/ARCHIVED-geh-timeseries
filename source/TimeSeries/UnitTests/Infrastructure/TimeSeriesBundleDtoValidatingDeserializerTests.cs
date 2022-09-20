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

using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.TimeSeries.Application.CimDeserialization.TimeSeriesBundle;
using Energinet.DataHub.TimeSeries.Application.Dtos;
using Energinet.DataHub.TimeSeries.Application.Enums;
using Energinet.DataHub.TimeSeries.TestCore.Assets;
using FluentAssertions;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.TimeSeries.UnitTests.Infrastructure
{
    [UnitTest]
    public class TimeSeriesBundleDtoValidatingDeserializerTests
    {
        private readonly TestDocuments _testDocuments;

        public TimeSeriesBundleDtoValidatingDeserializerTests()
        {
            _testDocuments = new TestDocuments();
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task
            ValidateAndDeserialize_WhenCalledWithValidCimXmlMessageWithMultipleSeries_ReturnsParsedObjectWithMultipleSeries(
                TimeSeriesBundleDtoValidatingDeserializer sut)
        {
            // Arrange
            var document = _testDocuments.ValidMultipleTimeSeries;

            // Act
            var result = await sut.ValidateAndDeserializeAsync(document).ConfigureAwait(false);

            // Assert
            result.TimeSeriesBundleDto.Series.Should().HaveCountGreaterThan(1);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task
            ValidateAndDeserialize_WhenCalledWithValidCimXmlMessageWithMultipleSeries_ReturnsParsedObjectWithPointsNotOverwrittenByLastSeriesPoints(
                TimeSeriesBundleDtoValidatingDeserializer sut)
        {
            // Arrange
            var document = _testDocuments.ValidMultipleTimeSeries;

            // Act
            var result = await sut.ValidateAndDeserializeAsync(document).ConfigureAwait(false);

            // Assert
            result.TimeSeriesBundleDto.Series.First().Should()
                .NotBeEquivalentTo(result.TimeSeriesBundleDto.Series.Last());
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task
            ValidateAndDeserialize_WhenCalledWithTimeSeriesWithQualityNotPresent_ReturnsParsedObjectWithQualitySetToMeasured(
                TimeSeriesBundleDtoValidatingDeserializer sut)
        {
            // Arrange
            var document = _testDocuments.ValidMultipleTimeSeriesMissingQuality;

            // Act
            var result = await sut.ValidateAndDeserializeAsync(document).ConfigureAwait(false);

            // Assert
            var allPoints = result.TimeSeriesBundleDto.Series
                .Select(x => x.Period).Select(y => y.Points);

            foreach (var points in allPoints)
            {
                foreach (var point in points)
                {
                    point.Quality.Should().Be(Quality.Measured);
                }
            }
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task
            ValidateAndDeserialize_WhenCalledWithTimeSeriesWithoutQuantity_ReturnsParsedObjectWithQuantitySetToNull(
                TimeSeriesBundleDtoValidatingDeserializer sut)
        {
            // Arrange
            var document = _testDocuments.ValidMultipleTimeSeriesMissingQuantity;

            // Act
            var result = await sut.ValidateAndDeserializeAsync(document).ConfigureAwait(false);

            // Assert
            result.HasErrors.Should().BeFalse();

            // Assert
            var allPoints = result.TimeSeriesBundleDto.Series
                .Select(x => x.Period).Select(y => y.Points);

            foreach (var points in allPoints)
            {
                foreach (var point in points)
                {
                    point.Quantity.Should().BeNull();
                }
            }
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task
            ValidateAndDeserialize_WhenCalledWithTimeSeriesInvalidSchema_ReturnsParsedObjectErrorsSet(
                TimeSeriesBundleDtoValidatingDeserializer sut)
        {
            // Arrange
            var document = _testDocuments.InvalidTimeSeriesMissingIdAsStream;

            // Act
            var result = await sut.ValidateAndDeserializeAsync(document).ConfigureAwait(false);

            // Assert
            result.Errors.Should().HaveCountGreaterThan(0);
            result.HasErrors.Should().BeTrue();
        }
    }
}
