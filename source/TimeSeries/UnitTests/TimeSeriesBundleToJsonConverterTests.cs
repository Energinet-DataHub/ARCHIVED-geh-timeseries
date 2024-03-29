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

using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Energinet.DataHub.TimeSeries.Application;
using Energinet.DataHub.TimeSeries.Application.Dtos;
using Energinet.DataHub.TimeSeries.Application.Enums;
using Energinet.DataHub.TimeSeries.TestCore.Assets;
using FluentAssertions;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.TimeSeries.UnitTests;

[UnitTest]
public class TimeSeriesBundleToJsonConverterTests
{
    private readonly TestDocuments _testDocuments;
    private readonly TimeSeriesBundleConverter _timeSeriesBundleConverter;

    public TimeSeriesBundleToJsonConverterTests()
    {
        _testDocuments = new TestDocuments();
        _timeSeriesBundleConverter = new TimeSeriesBundleConverter();
    }

    [Fact]
    public async Task TimeSeriesBundleDto_ConvertToJsonAsync_TimeSeriesBundleJsonString()
    {
        // Arrange
        var testTimeSeriesBundleDto = CreateTestTimeSeriesBundleDto();
        var expected = _testDocuments.TimeSeriesBundleJsonAsString;
        var stream = new MemoryStream();

        // Act
        await _timeSeriesBundleConverter.ConvertAsync(testTimeSeriesBundleDto, stream);
        var actual = Encoding.UTF8.GetString(stream.ToArray());

        // Assert
        Assert.Equal(expected, actual, ignoreLineEndingDifferences: true);
    }

    private TimeSeriesBundleDto CreateTestTimeSeriesBundleDto()
    {
        return new TimeSeriesBundleDto
        {
            Document = new DocumentDto
            {
                Id = "C1876453",
                CreatedDateTime = Instant.FromUtc(2022, 12, 17, 9, 30, 47),
                Sender = new MarketParticipantDto { Id = "5799999933317", BusinessProcessRole = MarketParticipantRole.MeteredDataResponsible },
                Receiver = new MarketParticipantDto { Id = "5790001330552", BusinessProcessRole = MarketParticipantRole.MeteredDataAdministrator },
                BusinessReasonCode = BusinessReasonCode.PeriodicMetering,
            },
            Series = new List<SeriesDto>
            {
                new()
                {
                    TransactionId = "C123456",
                    GsrnNumber = "579999993331812345",
                    MeteringPointType = MeteringPointType.Consumption,
                    RegistrationDateTime = Instant.FromUtc(2022, 12, 17, 7, 30),
                    Product = "8716867000030",
                    MeasureUnit = MeasureUnit.KiloWattHour,
                    Period = new PeriodDto
                    {
                        Resolution = Resolution.Hour,
                        StartDateTime = Instant.FromUtc(2022, 8, 15, 22, 0),
                        EndDateTime = Instant.FromUtc(2022, 8, 16, 4, 0),
                        Points = new List<PointDto>
                        {
                            new() { Quantity = "242", Quality = Quality.Estimated, Position = 1, },
                            new() { Quantity = "242", Quality = Quality.Measured, Position = 2, },
                            new() { Quantity = "222", Quality = Quality.Measured, Position = 3, },
                            new() { Quantity = "202", Quality = Quality.Measured, Position = 4, },
                            new() { Quantity = null, Quality = Quality.Missing, Position = 5, },
                        },
                    },
                },
                new()
                {
                    TransactionId = "C789123",
                    GsrnNumber = "579999997778885555",
                    MeteringPointType = MeteringPointType.Production,
                    RegistrationDateTime = Instant.FromUtc(2022, 12, 18, 07, 30),
                    Product = "8716867000030",
                    MeasureUnit = MeasureUnit.KiloWattHour,
                    Period = new PeriodDto
                    {
                        Resolution = Resolution.Hour,
                        StartDateTime = Instant.FromUtc(2022, 8, 16, 22, 0),
                        EndDateTime = Instant.FromUtc(2022, 8, 17, 1, 0),
                        Points = new List<PointDto>
                        {
                            new() { Quantity = "10.123", Quality = Quality.Estimated, Position = 1, },
                            new() { Quantity = "12", Quality = Quality.Measured, Position = 2, },
                            new() { Quantity = "756", Quality = Quality.Measured, Position = 3, },
                        },
                    },
                },
            },
        };
    }
}
