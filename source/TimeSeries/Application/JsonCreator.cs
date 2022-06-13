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
using System.Text.Json;
using Energinet.DataHub.TimeSeries.Application.Dtos;
using Energinet.DataHub.TimeSeries.Application.Enums;
using NodaTime;

namespace Energinet.DataHub.TimeSeries.Application;

public class JsonCreator
{
    public List<string> Create(TimeSeriesBundleDto timeSeriesBundle)
    {
        var timeSeriesJsonDtoList = new List<TimeSeriesJsonDto>();
        foreach (var series in timeSeriesBundle.Series)
        {
            timeSeriesJsonDtoList.Add(new TimeSeriesJsonDto
            {
                DocId = timeSeriesBundle.Document.Id,
                CreatedDateTime = timeSeriesBundle.Document.CreatedDateTime,
                Sender = timeSeriesBundle.Document.Sender,
                Receiver = timeSeriesBundle.Document.Receiver,
                BusinessReasonCode = timeSeriesBundle.Document.BusinessReasonCode,
                SeriesId = series.Id,
                TransactionId = series.TransactionId,
                MeteringPointId = series.MeteringPointId,
                MeteringPointType = series.MeteringPointType,
                RegistrationDateTime = series.RegistrationDateTime,
                Product = series.Product,
                MeasureUnit = series.MeasureUnit,
                Period = series.Period,
            });
        }

        var jsonFileList = new List<string>();
        foreach (var item in timeSeriesJsonDtoList)
        {
            jsonFileList.Add(JsonSerializer.Serialize(item));
        }

        return jsonFileList;
    }
}

public class TimeSeriesJsonDto
{
    public TimeSeriesJsonDto()
    {
        Sender = new MarketParticipantDto();
        Receiver = new MarketParticipantDto();

        // Cim deserialization will ensure a value is set.
        DocId = null!;

        Period = new PeriodDto();

        // Cim deserialization will ensure a value is set.
        SeriesId = null!;
        MeteringPointId = null!;
    }

    public string DocId { get; set; }

    public Instant CreatedDateTime { get; set; }

    public MarketParticipantDto Sender { get; set; }

    public MarketParticipantDto Receiver { get; set; }

    /// <summary>
    /// Process type in CIM XML
    /// </summary>
    public BusinessReasonCode BusinessReasonCode { get; set; }

    public string SeriesId { get; set; }

    public string? TransactionId { get; set; }

    public string MeteringPointId { get; set; }

    public MeteringPointType MeteringPointType { get; set; }

    public Instant RegistrationDateTime { get; set; }

    public string? Product { get; set; }

    public MeasureUnit MeasureUnit { get; set; }

    public PeriodDto Period { get; set; }
}
