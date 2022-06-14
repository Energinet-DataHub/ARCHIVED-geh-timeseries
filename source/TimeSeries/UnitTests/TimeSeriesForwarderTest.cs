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

using System.Collections.Generic;
using System.Threading.Tasks;
using Energinet.DataHub.Core.JsonSerialization;
using Energinet.DataHub.TimeSeries.Application;
using Energinet.DataHub.TimeSeries.Application.Dtos;
using Energinet.DataHub.TimeSeries.Application.Enums;
using Energinet.DataHub.TimeSeries.Infrastructure.Blob;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.TimeSeries.UnitTests;

public class TimeSeriesForwarderTest
{
    private readonly TimeSeriesForwarder _timeSeriesForwarder;

    public TimeSeriesForwarderTest()
    {
        IJsonSerializer jsonSerializer = new JsonSerializer();
        IBlobHandler blobHandler = new BlobHandler();
        _timeSeriesForwarder = new TimeSeriesForwarder(jsonSerializer, blobHandler);
    }

    [Fact]
    public async Task TestHandleAsync()
    {
        var testData = new TimeSeriesBundleDto
        {
            Document = new DocumentDto
            {
                Id = "1",
                CreatedDateTime = Instant.FromUtc(2022, 6, 13, 12, 0),
                Sender = new MarketParticipantDto { Id = "1", BusinessProcessRole = MarketParticipantRole.Unknown },
                Receiver = new MarketParticipantDto { Id = "2", BusinessProcessRole = MarketParticipantRole.Unknown },
                BusinessReasonCode = BusinessReasonCode.Unknown,
            },
            Series = new List<SeriesDto>
            {
                new()
                {
                    Id = "1",
                    TransactionId = "1",
                    MeteringPointId = "1",
                    MeteringPointType = MeteringPointType.Production,
                    RegistrationDateTime = Instant.FromUtc(2022, 6, 13, 12, 0),
                    Product = "1",
                    MeasureUnit = MeasureUnit.Unknown,
                    Period = new PeriodDto
                    {
                        Resolution = Resolution.Hour,
                        StartDateTime = Instant.FromUtc(2022, 6, 13, 12, 0),
                        EndDateTime = Instant.FromUtc(2022, 6, 13, 12, 0),
                        Points = new List<PointDto>
                        {
                            new() { Quantity = new decimal(1.1), Quality = Quality.Estimated, Position = 1, },
                            new() { Quantity = new decimal(1.1), Quality = Quality.Estimated, Position = 1, },
                        },
                    },
                },
                new()
                {
                    Id = "1",
                    TransactionId = "1",
                    MeteringPointId = "1",
                    MeteringPointType = MeteringPointType.Production,
                    RegistrationDateTime = Instant.FromUtc(2022, 6, 13, 12, 0),
                    Product = "1",
                    MeasureUnit = MeasureUnit.Unknown,
                    Period = new PeriodDto
                    {
                        Resolution = Resolution.Hour,
                        StartDateTime = Instant.FromUtc(2022, 6, 13, 12, 0),
                        EndDateTime = Instant.FromUtc(2022, 6, 13, 12, 0),
                        Points = new List<PointDto>
                        {
                            new() { Quantity = new decimal(1.1), Quality = Quality.Estimated, Position = 1, },
                            new() { Quantity = new decimal(1.1), Quality = Quality.Estimated, Position = 1, },
                        },
                    },
                },
            },
        };
        await _timeSeriesForwarder.HandleAsync(testData, "UseDevelopmentStorage=true");
    }
}
