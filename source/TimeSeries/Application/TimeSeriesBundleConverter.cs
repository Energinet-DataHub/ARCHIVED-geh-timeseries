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
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Energinet.DataHub.TimeSeries.Application.Dtos;
using Energinet.DataHub.TimeSeries.Application.Dtos.Converted;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Energinet.DataHub.TimeSeries.Application;

public class TimeSeriesBundleConverter : ITimeSeriesBundleConverter
{
    public async Task ConvertAsync(TimeSeriesBundleDto timeSeriesBundle, Stream stream)
    {
        var timeSeriesJsonDtoList = timeSeriesBundle
            .Series
            .Select(series => Convert(timeSeriesBundle.Document, series))
            .ToList();

        var newLine = Encoding.UTF8.GetBytes(Environment.NewLine);

        // Options will be remove when SerializeAsync has been implemented in custom JsonSerializer
        var options = new JsonSerializerOptions();
        options.Converters.Add(NodaConverters.InstantConverter);
        options.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

        for (var index = 0; index < timeSeriesJsonDtoList.Count; index++)
        {
            if (index != 0)
            {
                // Places each json object on a new line to make it easier to consume in databricks
                await stream.WriteAsync(newLine).ConfigureAwait(false);
            }

            var item = timeSeriesJsonDtoList[index];

            // JsonSerializer.SerializeAsync will be replaced with custom implementation when ready
            await JsonSerializer.SerializeAsync(stream, item, options);
        }
    }

    private static TimeSeriesRawDto Convert(DocumentDto document, SeriesDto seriesDto)
    {
        return new TimeSeriesRawDto(
            document.Id,
            document.CreatedDateTime,
            new MarketParticipantRawDto(document.Sender.Id, document.Sender.BusinessProcessRole),
            new MarketParticipantRawDto(document.Receiver.Id, document.Receiver.BusinessProcessRole),
            document.BusinessReasonCode,
            seriesDto.TransactionId,
            seriesDto.GsrnNumber,
            seriesDto.MeteringPointType,
            seriesDto.RegistrationDateTime,
            seriesDto.Product,
            seriesDto.MeasureUnit,
            new PeriodRawDto(
                seriesDto.Period.Resolution,
                seriesDto.Period.StartDateTime,
                seriesDto.Period.EndDateTime,
                seriesDto.Period.Points
                    .Select(p => new PointRawDto(p.Position, p.Quality, p.Quantity))
                    .ToArray()));
    }
}
