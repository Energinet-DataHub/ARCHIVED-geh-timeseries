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

public class JsonCreator
{
    private readonly IJsonSerializer _jsonSerializer;

    public JsonCreator(IJsonSerializer jsonSerializer)
    {
        _jsonSerializer = jsonSerializer;
    }

    public string Create(TimeSeriesBundleDto timeSeriesBundle)
    {
        var timeSeriesJsonDtoList = timeSeriesBundle.Series.Select(series => new TimeSeriesJsonDto
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
            })
            .ToList();

        var jsonFileList = timeSeriesJsonDtoList.Select(timeSeriesJsonDto => _jsonSerializer.Serialize(timeSeriesJsonDto)).ToList();

        return string.Join(Environment.NewLine, jsonFileList);
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
