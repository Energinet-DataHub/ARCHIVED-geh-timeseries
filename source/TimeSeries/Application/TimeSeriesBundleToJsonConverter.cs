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
using System.Collections.Generic;
using System.Linq;
using Energinet.DataHub.Core.JsonSerialization;
using Energinet.DataHub.TimeSeries.Application.Dtos;
using Energinet.DataHub.TimeSeries.Application.Enums;
using NodaTime;

namespace Energinet.DataHub.TimeSeries.Application;

public class TimeSeriesBundleToJsonConverter
{
    private readonly IJsonSerializer _jsonSerializer;

    public TimeSeriesBundleToJsonConverter(IJsonSerializer jsonSerializer)
    {
        _jsonSerializer = jsonSerializer;
    }

    public string ConvertToJson(TimeSeriesBundleDto timeSeriesBundle)
    {
        var timeSeriesJsonDtoList = timeSeriesBundle.Series.Select(series => new
            {
                DocumentId = timeSeriesBundle.Document.Id,
                timeSeriesBundle.Document.CreatedDateTime,
                timeSeriesBundle.Document.Sender,
                timeSeriesBundle.Document.Receiver,
                timeSeriesBundle.Document.BusinessReasonCode,
                SeriesId = series.Id,
                series.TransactionId,
                series.MeteringPointId,
                series.MeteringPointType,
                series.RegistrationDateTime,
                series.Product,
                series.MeasureUnit,
                series.Period,
            })
            .ToList();

        var jsonStrings = timeSeriesJsonDtoList.Select(timeSeriesJsonDto => _jsonSerializer.Serialize(timeSeriesJsonDto)).ToList();

        return string.Join(Environment.NewLine, jsonStrings);
    }
}
