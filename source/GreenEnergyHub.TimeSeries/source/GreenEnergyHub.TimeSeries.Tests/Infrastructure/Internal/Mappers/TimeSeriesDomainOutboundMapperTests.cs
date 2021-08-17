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
using GreenEnergyHub.TimeSeries.Core.DateTime;
using GreenEnergyHub.TimeSeries.Core.Enumeration;
using GreenEnergyHub.TimeSeries.Domain.MarketDocument;
using GreenEnergyHub.TimeSeries.Infrastructure.Contracts.Internal.Mappers;
using GreenEnergyHub.TimeSeries.TestCore;
using GreenEnergyHub.TimeSeries.TestCore.Protobuf;
using NodaTime;
using Xunit;
using Xunit.Categories;
using domain = GreenEnergyHub.TimeSeries.Domain.Notification;
using proto = GreenEnergyHub.TimeSeries.Contracts.Internal;

namespace GreenEnergyHub.TimeSeries.Tests.Infrastructure.Internal.Mappers
{
    [UnitTest]
    public class TimeSeriesDomainOutboundMapperTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void Convert_WhenCalled_ShouldMapToProtobufWithCorrectValues(
            [NotNull] domain.TimeSeriesCommand expectedTimeSeriesCommand,
            [NotNull] TimeSeriesCommandOutboundMapper sut)
        {
            FixPossiblyInvalidValues(expectedTimeSeriesCommand);
            var actualProtobufMessage = (proto.TimeSeriesCommand)sut.Convert(expectedTimeSeriesCommand);

            // TODO: The assert currently doesn't support the DecimalValue type. All the point type workaround here should be moved in an upcoming pull request
            ProtobufAssert.OutgoingContractIsSubset(
                expectedTimeSeriesCommand,
                actualProtobufMessage,
                new[] { "Series.Points[0].Quantity", "Series.Points[1].Quantity", "Series.Points[2].Quantity" });
            for (var i = 0; i < actualProtobufMessage.Series.Points.Count; i++)
            {
                var domainPoint = expectedTimeSeriesCommand.Series.Points[i];
                var protoPoint = actualProtobufMessage.Series.Points[i];
                Assert.Equal(domainPoint.Position, protoPoint.Position);
                Assert.Equal(domainPoint.Quality, protoPoint.Quality.Cast<domain.Quality>());
                Assert.True(domainPoint.Quantity == protoPoint.Quantity);
                Assert.Equal(domainPoint.ObservationDateTime.TruncateToSeconds(), protoPoint.ObservationDateTime.ToInstant().TruncateToSeconds());
            }
        }

        private static void FixPossiblyInvalidValues([NotNull] domain.TimeSeriesCommand timeSeriesCommand)
        {
            FixPossiblyInvalidInstants(timeSeriesCommand);
            FixPossiblyInvalidEnums(timeSeriesCommand);
        }

        private static void FixPossiblyInvalidInstants([NotNull] domain.TimeSeriesCommand timeSeriesCommand)
        {
            timeSeriesCommand.Document.CreatedDateTime =
                SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromDays(11)).TruncateToSeconds();
            timeSeriesCommand.Document.RequestDateTime =
                SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromDays(10)).TruncateToSeconds();
            timeSeriesCommand.Series.RegistrationDateTime =
                SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromDays(12)).TruncateToSeconds();
            timeSeriesCommand.Series.StartDateTime =
                SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromDays(42)).TruncateToSeconds();
            timeSeriesCommand.Series.EndDateTime =
                SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromDays(12)).TruncateToSeconds();

            foreach (var point in timeSeriesCommand.Series.Points)
            {
                point.ObservationDateTime = SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromDays(10));
            }
        }

        private static void FixPossiblyInvalidEnums(domain.TimeSeriesCommand timeSeriesCommand)
        {
            timeSeriesCommand.Document.Recipient.BusinessProcessRole = MarketParticipantRole.GridAccessProvider;
            timeSeriesCommand.Series.Points.ForEach(p => p.Quality = domain.Quality.Measured);
        }
    }
}
