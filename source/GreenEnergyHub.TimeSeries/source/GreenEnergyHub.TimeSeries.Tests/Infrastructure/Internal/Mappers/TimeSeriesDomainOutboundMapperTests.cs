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

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Energinet.DataHub.TimeSeries.InternalContracts;
using FluentAssertions;
using GreenEnergyHub.TestHelpers.FluentAssertionsExtensions;
using GreenEnergyHub.TimeSeries.Core.DateTime;
using GreenEnergyHub.TimeSeries.Domain.Notification;
using GreenEnergyHub.TimeSeries.Infrastructure.Internal.Mappers;
using GreenEnergyHub.TimeSeries.TestCore;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.TimeSeries.Tests.Infrastructure.Internal.Mappers
{
    [UnitTest]
    public class TimeSeriesDomainOutboundMapperTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void Convert_WhenCalled_ShouldMapToProtobufWithCorrectValues([NotNull] TimeSeriesCommand timeSeriesCommand)
        {
            // Arrange
            UpdateInstantsToValidTimes(timeSeriesCommand);
            var mapper = new TimeSeriesCommandOutboundMapper();

            // Act
            var converted = (TimeSeriesCommandContract)mapper.Convert(timeSeriesCommand);

            // Assert
            var timeSeriesDocument = timeSeriesCommand.Document;
            var timeSeriesSeries = timeSeriesCommand.Series;
            var convertedDocument = converted.Document;

            Assert.Equal(
                timeSeriesDocument.CreatedDateTime.ToTimestamp().TruncateToSeconds(),
                converted.Document.CreatedDateTime);

            convertedDocument.Id.Should().BeEquivalentTo(timeSeriesDocument.Id);
            convertedDocument.Sender.Id.Should().BeEquivalentTo(timeSeriesDocument.Sender.Id);
            converted.Series.Id.Should().Be(timeSeriesSeries.Id);
            converted.Series.Points.First().Position.Should().Be(timeSeriesSeries.Points.First().Position);

            convertedDocument.Should().NotContainNullsOrEmptyEnumerables();
            timeSeriesSeries.Should().NotContainNullsOrEmptyEnumerables();
            timeSeriesDocument.Should().NotContainNullsOrEmptyEnumerables();
        }

        private static void UpdateInstantsToValidTimes([NotNull] TimeSeriesCommand timeSeriesCommand)
        {
            timeSeriesCommand.Document.CreatedDateTime = SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromDays(11));
            timeSeriesCommand.Document.RequestDateTime = SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromDays(10));
            timeSeriesCommand.Series.RegistrationDateTime = SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromDays(12));
            timeSeriesCommand.Series.StartDateTime = SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromDays(42));
            timeSeriesCommand.Series.EndDateTime = SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromDays(12));

            foreach (var point in timeSeriesCommand.Series.Points)
            {
                point.ObservationDateTime = SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromDays(10));
            }
        }
    }
}
