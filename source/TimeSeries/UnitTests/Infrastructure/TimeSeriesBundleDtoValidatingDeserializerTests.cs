﻿// Copyright 2020 Energinet DataHub A/S
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
using Energinet.DataHub.TimeSeries.Infrastructure.CimDeserialization.TimeSeriesBundle;
using Energinet.DataHub.TimeSeries.TestCore.Assets;
using Energinet.DataHub.TimeSeries.TestCore.Attributes;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.TimeSeries.UnitTests.Infrastructure
{
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
            var document = _testDocuments.ValidMultipleTimeSeriesMissingIdAsStream;

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
            var document = _testDocuments.ValidMultipleTimeSeriesMissingIdAsStream;

            // Act
            var result = await sut.ValidateAndDeserializeAsync(document).ConfigureAwait(false);

            // Assert
            result.TimeSeriesBundleDto.Series.First().Should()
                .NotBeEquivalentTo(result.TimeSeriesBundleDto.Series.Last());
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task
            ValidateAndDeserialize_WhenCalledWithTimeSeriesWithQualityNotPresent_ReturnsParsedObjectWithQualitySetToAsProvided(
                TimeSeriesBundleDtoValidatingDeserializer sut)
        {
            // Arrange
            var document = _testDocuments.ValidMultipleTimeSeriesMissingIdAsStream;

            // Act
            var result = await sut.ValidateAndDeserializeAsync(document).ConfigureAwait(false);

            // Assert
            result.Should().BeNull();
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task
            ValidateAndDeserialize_WhenCalledWithTimeSeriesWithQuantityNotPresent_ReturnsParsedObjectWithQuantitySetToNull(
                TimeSeriesBundleDtoValidatingDeserializer sut)
        {
            // Arrange
            var document = _testDocuments.ValidMultipleTimeSeriesMissingIdAsStream;

            // Act
            var result = await sut.ValidateAndDeserializeAsync(document).ConfigureAwait(false);

            // Assert
            result.Should().BeNull();
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task
            ValidateAndDeserialize_WhenCalledWithTimeSeriesInvalidScema_ReturnsParsedObjectErrorsSet(
                TimeSeriesBundleDtoValidatingDeserializer sut)
        {
            // Arrange
            var document = _testDocuments.ValidMultipleTimeSeriesMissingIdAsStream;

            // Act
            var result = await sut.ValidateAndDeserializeAsync(document).ConfigureAwait(false);

            // Assert
            result.Should().BeNull();
        }
    }
}