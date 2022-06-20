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

using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Energinet.DataHub.Core.JsonSerialization;
using Energinet.DataHub.TimeSeries.Application.Dtos;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Energinet.DataHub.TimeSeries.Application;

public class TimeSeriesBundleToJsonConverter : ITimeSeriesBundleToJsonConverter
{
    private readonly IJsonSerializer _jsonSerializer;

    public TimeSeriesBundleToJsonConverter(IJsonSerializer jsonSerializer)
    {
        _jsonSerializer = jsonSerializer;
    }

    public async Task ConvertAsync(TimeSeriesBundleDto timeSeriesBundle, Stream stream)
    {
        var timeSeriesJsonDtoList = timeSeriesBundle.Series.Select(series => new
            {
                DocumentId = timeSeriesBundle.Document.Id,
                timeSeriesBundle.Document.CreatedDateTime,
                timeSeriesBundle.Document.Sender,
                timeSeriesBundle.Document.Receiver,
                timeSeriesBundle.Document.BusinessReasonCode,
                SeriesId = series.Id,
                TransactionId = (string?)series.TransactionId,
                series.MeteringPointId,
                series.MeteringPointType,
                series.RegistrationDateTime,
                Product = (string?)series.Product,
                series.MeasureUnit,
                series.Period,
            })
            .ToList();

        var newLine = Encoding.UTF8.GetBytes("\n");

        var options = new JsonSerializerOptions();
        options.Converters.Add(NodaConverters.InstantConverter);
        options.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
        for (var index = 0; index < timeSeriesJsonDtoList.Count; index++)
        {
            if (index != 0)
            {
                await stream.WriteAsync(newLine).ConfigureAwait(false);
            }

            var item = timeSeriesJsonDtoList[index];
            await JsonSerializer.SerializeAsync(stream, item, options);
        }
    }
}
