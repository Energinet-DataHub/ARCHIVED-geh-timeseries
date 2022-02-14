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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.Core.Schemas;
using Energinet.DataHub.Core.SchemaValidation;
using Energinet.DataHub.TimeSeries.Application.Dtos;
using Energinet.DataHub.TimeSeries.Infrastructure.Cim.MarketDocument;
using NodaTime;

namespace Energinet.DataHub.TimeSeries.Infrastructure.CimDeserialization.TimeSeriesBundle
{
    public class TimeSeriesBundleDtoValidatingDeserializer : ITimeSeriesBundleDtoValidatingDeserializer
    {
        public async Task<TimeSeriesBundleDtoResult> ValidateAndDeserializeAsync(Stream reqBody)
        {
            var reader = new SchemaValidatingReader(reqBody, Schemas.CimXml.MeasureNotifyValidatedMeasureData);
            var timeSeriesBundle = await ConvertAsync(reader).ConfigureAwait(false);
            return new TimeSeriesBundleDtoResult
            {
                HasErrors = reader.HasErrors,
                Errors = reader.Errors.ToList(),
                TimeSeriesBundleDto = timeSeriesBundle,
            };
        }

        private static async Task<TimeSeriesBundleDto> ConvertAsync(SchemaValidatingReader reader)
        {
            var timeSeriesBundle = new TimeSeriesBundleDto();
            timeSeriesBundle.Document = await ParseDocumentAsync(reader).ConfigureAwait(false);
            timeSeriesBundle.Series = await ParseSeriesAsync(reader).ConfigureAwait(false);

            return timeSeriesBundle;
        }

        private static async Task<IEnumerable<SeriesDto>> ParseSeriesAsync(SchemaValidatingReader reader)
        {
            return await ParseSeriesFieldsAsync(reader).ConfigureAwait(false);
        }

        private static async Task<IEnumerable<SeriesDto>> ParseSeriesFieldsAsync(SchemaValidatingReader reader)
        {
            var series = new List<SeriesDto>();

            var hasReadRoot = false;

            while (await reader.AdvanceAsync().ConfigureAwait(false))
            {
                if (!hasReadRoot)
                {
                    hasReadRoot = true;
                }
                else if (reader.CurrentNodeName == CimMarketDocumentConstants.Series &&
                         reader.CurrentNodeType == NodeType.StartElement)
                {
                    var seriesEntry = new SeriesDto();

                    while (await reader.AdvanceAsync().ConfigureAwait(false))
                    {
                        if (reader.CurrentNodeName == CimMarketDocumentConstants.Id && reader.CanReadValue)
                        {
                            var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                            seriesEntry.Id = content;
                        }
                        else if (reader.CurrentNodeName ==
                            CimMarketDocumentConstants.OriginalTransactionIdReferenceSeriesId && reader.CanReadValue)
                        {
                            var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                            seriesEntry.TransactionId = content;
                        }
                        else if (reader.CurrentNodeName ==
                                 CimMarketDocumentConstants.MeteringPointId && reader.CanReadValue)
                        {
                            var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                            seriesEntry.MeteringPointId = content;
                        }
                        else if (reader.CurrentNodeName ==
                                 CimMarketDocumentConstants.MeteringPointType && reader.CanReadValue)
                        {
                            var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                            seriesEntry.MeteringPointType = MeteringPointTypeMapper.Map(content);
                        }
                        else if (reader.CurrentNodeName ==
                                 CimMarketDocumentConstants.RegistrationDateTime && reader.CanReadValue)
                        {
                            var content = await reader.ReadValueAsNodaTimeAsync().ConfigureAwait(false);
                            seriesEntry.RegistrationDateTime = content;
                        }
                        else if (reader.CurrentNodeName ==
                                 CimMarketDocumentConstants.Product && reader.CanReadValue)
                        {
                            var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                            seriesEntry.Product = content;
                        }
                        else if (reader.CurrentNodeName ==
                                 CimMarketDocumentConstants.MeasureUnit && reader.CanReadValue)
                        {
                            var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                            seriesEntry.MeasureUnit = MeasureUnitMapper.Map(content);
                        }
                        else if (reader.CurrentNodeName == CimMarketDocumentConstants.Period &&
                                 reader.CurrentNodeType == NodeType.StartElement)
                        {
                            seriesEntry.Period = await ParsePeriodAsync(reader).ConfigureAwait(false);
                            series.Add(seriesEntry);
                            seriesEntry = new SeriesDto();
                        }
                    }
                }
            }

            return series;
        }

        private static async Task<PeriodDto> ParsePeriodAsync(SchemaValidatingReader reader)
        {
            var period = new PeriodDto();
            var points = new List<PointDto>();

            while (await reader.AdvanceAsync().ConfigureAwait(false))
            {
                if (reader.CurrentNodeName == CimMarketDocumentConstants.Resolution && reader.CanReadValue)
                {
                    var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                    period.Resolution = ResolutionMapper.Map(content);
                }
                else if (reader.CurrentNodeName == CimMarketDocumentConstants.TimeIntervalStart && reader.CanReadValue)
                {
                    var content = await reader
                        .ReadValueAsStringAsync()
                        .ConfigureAwait(false);

                    period.StartDateTime = Instant.FromDateTimeOffset(DateTimeOffset.Parse(content));
                }
                else if (reader.CurrentNodeName == CimMarketDocumentConstants.TimeIntervalEnd && reader.CanReadValue)
                {
                    var content = await reader
                        .ReadValueAsStringAsync()
                        .ConfigureAwait(false);

                    period.EndDateTime = Instant.FromDateTimeOffset(DateTimeOffset.Parse(content));
                }
                else if (reader.CurrentNodeName == CimMarketDocumentConstants.Point && reader.CurrentNodeType == NodeType.StartElement)
                {
                    points = await ParsePointsAsync(reader).ConfigureAwait(false);
                }
                else if (reader.CurrentNodeName == CimMarketDocumentConstants.Series &&
                         reader.CurrentNodeType == NodeType.EndElement)
                {
                    period.Points = points;
                    break;
                }
            }

            return period;
        }

        private static async Task<List<PointDto>> ParsePointsAsync(SchemaValidatingReader reader)
        {
            var points = new List<PointDto>();
            var point = new PointDto();

            while (await reader.AdvanceAsync().ConfigureAwait(false))
            {
                if (reader.CurrentNodeName == CimMarketDocumentConstants.Position && reader.CanReadValue)
                {
                    var content = await reader.ReadValueAsIntAsync().ConfigureAwait(false);
                    point.Position = content;
                }
                else if (reader.CurrentNodeName == CimMarketDocumentConstants.Quantity && reader.CanReadValue)
                {
                    var content = await reader.ReadValueAsDecimalAsync().ConfigureAwait(false);
                    point.Quantity = content;
                }
                else if (reader.CurrentNodeName == CimMarketDocumentConstants.Quality && reader.CanReadValue)
                {
                    var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                    point.Quality = QualityMapper.Map(content);
                }
                else if (reader.CurrentNodeName == CimMarketDocumentConstants.Point && reader.CurrentNodeType == NodeType.EndElement)
                {
                    points.Add(point);
                    point = new PointDto();
                }
                else if
                    (reader.CurrentNodeName == CimMarketDocumentConstants.Period && reader.CurrentNodeType ==
                     NodeType.EndElement)
                {
                    break;
                }
            }

            return points;
        }

        private static async Task<DocumentDto> ParseDocumentAsync(SchemaValidatingReader reader)
        {
            var document = new DocumentDto()
            {
                Sender = new MarketParticipantDto(),
                Receiver = new MarketParticipantDto(),
            };

            await ParseDocumentFieldsAsync(reader, document).ConfigureAwait(false);

            return document;
        }

        private static async Task ParseDocumentFieldsAsync(SchemaValidatingReader reader, DocumentDto documentDto)
        {
            var hasReadRoot = false;

            while (await reader.AdvanceAsync().ConfigureAwait(false))
            {
                if (!hasReadRoot)
                {
                    hasReadRoot = true;
                }
                else if (reader.CurrentNodeName == CimMarketDocumentConstants.Id && reader.CanReadValue)
                {
                    var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                    documentDto.Id = content;
                }
                else if (reader.CurrentNodeName == CimMarketDocumentConstants.BusinessReasonCode && reader.CanReadValue)
                {
                    var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                    documentDto.BusinessReasonCode = BusinessReasonCodeMapper.Map(content);
                }
                else if (reader.CurrentNodeName == CimMarketDocumentConstants.SenderId && reader.CanReadValue)
                {
                    var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                    documentDto.Sender.Id = content;
                }
                else if (reader.CurrentNodeName == CimMarketDocumentConstants.SenderBusinessProcessRole && reader.CanReadValue)
                {
                    var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                    documentDto.Sender.BusinessProcessRole = MarketParticipantRoleMapper.Map(content);
                }
                else if (reader.CurrentNodeName == CimMarketDocumentConstants.RecipientId && reader.CanReadValue)
                {
                    var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                    documentDto.Receiver.Id = content;
                }
                else if (reader.CurrentNodeName == CimMarketDocumentConstants.RecipientBusinessProcessRole && reader.CanReadValue)
                {
                    var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                    documentDto.Receiver.BusinessProcessRole = MarketParticipantRoleMapper.Map(content);
                }
                else if (reader.CurrentNodeName == CimMarketDocumentConstants.CreatedDateTime && reader.CanReadValue)
                {
                    documentDto.CreatedDateTime = await reader.ReadValueAsNodaTimeAsync().ConfigureAwait(false);
                    break;
                }
            }
        }
    }
}
