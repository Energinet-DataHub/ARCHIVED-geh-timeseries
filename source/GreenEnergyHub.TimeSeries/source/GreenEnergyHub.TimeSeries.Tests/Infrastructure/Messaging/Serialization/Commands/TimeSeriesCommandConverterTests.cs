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

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using AutoFixture.Xunit2;
using GreenEnergyHub.Iso8601;
using GreenEnergyHub.TimeSeries.Domain.MarketDocument;
using GreenEnergyHub.TimeSeries.Domain.Notification;
using GreenEnergyHub.TimeSeries.Infrastructure.Messaging;
using GreenEnergyHub.TimeSeries.Infrastructure.Messaging.Serialization.Commands;
using GreenEnergyHub.TimeSeries.TestCore;
using Moq;
using NodaTime;
using NodaTime.Text;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.TimeSeries.Tests.Infrastructure.Messaging.Serialization.Commands
{
    [UnitTest]
    public class TimeSeriesCommandConverterTests
    {
        [Theory]
        [InlineAutoMoqData]
        public async Task ConvertAsync_WhenCalledWithValidHourlyTimeSeries_ReturnsParsedObject(
            [NotNull][Frozen] Mock<ICorrelationContext> context,
            [NotNull][Frozen] Mock<IIso8601Durations> iso8601Durations,
            [NotNull] TimeSeriesCommandConverter timeSeriesCommandConverter)
        {
            // Arrange
            var correlationId = Guid.NewGuid().ToString();
            context.Setup(c => c.CorrelationId).Returns(correlationId);

            SetObservationTime(iso8601Durations, "2021-06-27T22:00:00Z");

            var stream = GetEmbeddedResource("GreenEnergyHub.TimeSeries.Tests.TestFiles.Valid_Hourly_CIM_TimeSeries.xml");
            using var reader = XmlReader.Create(stream, new XmlReaderSettings { Async = true });

            // Act
            var result = (TimeSeriesCommand)await timeSeriesCommandConverter.ConvertAsync(reader).ConfigureAwait(false);

            // Assert
            Assert.Equal(correlationId, result.CorrelationId);
            // Document
            Assert.Equal("DocId_Valid_001", result.Document.Id);
            Assert.Equal(DocumentType.NotifyValidatedMeasureData, result.Document.Type);
            Assert.Equal(BusinessReasonCode.PeriodicFlexMetering, result.Document.BusinessReasonCode);
            Assert.Equal("8100000000030", result.Document.Sender.Id);
            Assert.Equal(MarketParticipantRole.MeteredDataResponsible, result.Document.Sender.BusinessProcessRole);
            Assert.Equal("5790001330552", result.Document.Recipient.Id);
            Assert.Equal(MarketParticipantRole.SystemOperator, result.Document.Recipient.BusinessProcessRole);
            Assert.Equal(InstantPattern.ExtendedIso.Parse("2021-06-21T10:23:40.149Z").Value, result.Document.CreatedDateTime);
            // Series
            Assert.Equal("tId_Valid_001", result.Series.Id);
            Assert.Equal(Product.EnergyActive, result.Series.Product);
            Assert.Equal("578032999778756222", result.Series.MeteringPointId);
            Assert.Equal(MeteringPointType.Consumption, result.Series.MeteringPointType);
            Assert.Equal(SettlementMethod.Flex, result.Series.SettlementMethod);
            Assert.Equal(InstantPattern.ExtendedIso.Parse("2021-06-21T10:23:40.150Z").Value, result.Series.RegistrationDateTime);
            Assert.Equal(MeasureUnit.KiloWattHour, result.Series.Unit);
            Assert.Equal(TimeSeriesResolution.Hour, result.Series.Resolution);
            Assert.Equal(InstantPattern.ExtendedIso.Parse("2021-06-27T22:00:00Z").Value, result.Series.StartDateTime);
            Assert.Equal(InstantPattern.ExtendedIso.Parse("2021-06-28T22:00:00Z").Value, result.Series.EndDateTime);
            // Points
            Assert.Equal(24, result.Series.Points.Count);
            Assert.Equal(1, result.Series.Points[0].Position);
            Assert.Equal(0.337m, result.Series.Points[0].Quantity);
            Assert.Equal(InstantPattern.ExtendedIso.Parse("2021-06-27T22:00:00Z").Value, result.Series.Points[0].ObservationDateTime);
            Assert.Equal(Quality.Measured, result.Series.Points[0].Quality);

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task ConvertAsync_WhenCalledWithValidTimeSeriesWithQuantityMissingFirstPosition_ReturnsParsedObject(
            [NotNull][Frozen] Mock<ICorrelationContext> context,
            [NotNull][Frozen] Mock<IIso8601Durations> iso8601Durations,
            [NotNull] TimeSeriesCommandConverter timeSeriesCommandConverter)
        {
            // Arrange
            var correlationId = Guid.NewGuid().ToString();
            context.Setup(c => c.CorrelationId).Returns(correlationId);

            SetObservationTime(iso8601Durations, "2021-06-27T22:00:00Z");

            var stream = GetEmbeddedResource("GreenEnergyHub.TimeSeries.Tests.TestFiles.Valid_Hourly_CIM_TimeSeries_WithQuantityMissingFirstPosition.xml");
            using var reader = XmlReader.Create(stream, new XmlReaderSettings { Async = true });

            // Act
            var result = (TimeSeriesCommand)await timeSeriesCommandConverter.ConvertAsync(reader).ConfigureAwait(false);

            // Assert
            Assert.Equal(Quality.QuantityMissing, result.Series.Points[0].Quality);
            Assert.Equal(Quality.Measured, result.Series.Points[1].Quality);

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task ConvertAsync_WhenCalledWithValidTimeSeriesWithoutQualityElements_ReturnsParsedObjectWithQualityMeasured(
            [NotNull][Frozen] Mock<ICorrelationContext> context,
            [NotNull][Frozen] Mock<IIso8601Durations> iso8601Durations,
            [NotNull] TimeSeriesCommandConverter timeSeriesCommandConverter)
        {
            // Arrange
            var correlationId = Guid.NewGuid().ToString();
            context.Setup(c => c.CorrelationId).Returns(correlationId);

            SetObservationTime(iso8601Durations, "2021-06-27T22:00:00Z");

            var stream = GetEmbeddedResource("GreenEnergyHub.TimeSeries.Tests.TestFiles.Valid_Hourly_CIM_TimeSeries_WithoutQualityElements.xml");
            using var reader = XmlReader.Create(stream, new XmlReaderSettings { Async = true });

            // Act
            var result = (TimeSeriesCommand)await timeSeriesCommandConverter.ConvertAsync(reader).ConfigureAwait(false);

            // Assert
            Assert.Equal(24, result.Series.Points.Count);
            Assert.Equal(Quality.Measured, result.Series.Points[0].Quality);

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task ConvertAsync_WhenCalledWithValidTimeSeriesContainingUnusedCimContent_ReturnsParsedObject(
            [NotNull][Frozen] Mock<ICorrelationContext> context,
            [NotNull][Frozen] Mock<IIso8601Durations> iso8601Durations,
            [NotNull] TimeSeriesCommandConverter timeSeriesCommandConverter)
        {
            // Arrange
            var correlationId = Guid.NewGuid().ToString();
            context.Setup(c => c.CorrelationId).Returns(correlationId);

            SetObservationTime(iso8601Durations, "2021-06-27T22:00:00Z");

            var stream = GetEmbeddedResource("GreenEnergyHub.TimeSeries.Tests.TestFiles.Valid_Hourly_CIM_TimeSeries_WithUnusedCimContent.xml");
            using var reader = XmlReader.Create(stream, new XmlReaderSettings { Async = true });

            // Act
            var result = (TimeSeriesCommand)await timeSeriesCommandConverter.ConvertAsync(reader).ConfigureAwait(false);

            // Assert
            Assert.Equal("DocId_Valid_002", result.Document.Id);
            Assert.Equal("tId_Valid_002", result.Series.Id);
            Assert.Equal(24, result.Series.Points.Count);

            await Task.CompletedTask.ConfigureAwait(false);
        }

        private static void SetObservationTime(Mock<IIso8601Durations> iso8601Durations, string isoDate)
        {
            var observationTime = InstantPattern.ExtendedIso.Parse(isoDate).Value;
            iso8601Durations.Setup(d => d.AddDuration(
                It.IsAny<Instant>(),
                It.IsAny<string>(),
                It.IsAny<int>())).Returns(observationTime);
        }

        private static async Task<byte[]> GetEmbeddedResourceAsBytes(string path)
        {
            var input = GetEmbeddedResource(path);

            var byteInput = new byte[input.Length];
            await input.ReadAsync(byteInput.AsMemory(0, (int)input.Length)).ConfigureAwait(false);
            return byteInput;
        }

        private static Stream GetEmbeddedResource(string path)
        {
            var assembly = Assembly.GetExecutingAssembly();
            return EmbeddedStreamHelper.GetInputStream(assembly, path);
        }
    }
}
