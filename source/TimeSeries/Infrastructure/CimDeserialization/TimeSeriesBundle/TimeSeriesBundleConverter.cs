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

using System.Threading.Tasks;
using Energinet.DataHub.Core.SchemaValidation;
using Energinet.DataHub.TimeSeries.Application.Dtos;
using Energinet.DataHub.TimeSeries.Infrastructure.Cim.MarketDocument;

namespace Energinet.DataHub.TimeSeries.Infrastructure.CimDeserialization.TimeSeriesBundle
{
    public class TimeSeriesBundleConverter
    {
        public async Task<TimeSeriesBundleDto> ConvertAsync(SchemaValidatingReader reader)
        {
            var timeSeriesBundle = new TimeSeriesBundleDto();
            timeSeriesBundle.Document = await ParseDocumentAsync(reader).ConfigureAwait(false);

            // var message = await ConvertSpecializedContentAsync(reader, document).ConfigureAwait(false);
            return timeSeriesBundle;
        }

        // protected override Task<IInboundMessage> ConvertSpecializedContentAsync(SchemaValidatingReader reader, DocumentDto documentDto);
        private static async Task<DocumentDto> ParseDocumentAsync(SchemaValidatingReader reader)
        {
            var document = new DocumentDto()
            {
                Sender = new MarketParticipantDto(),
                Receiver = new MarketParticipantDto(),
            };

            await ParseFieldsAsync(reader, document).ConfigureAwait(false);

            return document;
        }

        private static async Task ParseFieldsAsync(SchemaValidatingReader reader, DocumentDto documentDto)
        {
            var hasReadRoot = false;

            while (await reader.AdvanceAsync().ConfigureAwait(false))
            {
                if (!hasReadRoot)
                {
                    hasReadRoot = true;
                }
                else if (reader.Is(CimMarketDocumentConstants.Id))
                {
                    var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                    documentDto.Id = content;
                }
                else if (reader.Is(CimMarketDocumentConstants.BusinessReasonCode))
                {
                    var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                    documentDto.BusinessReasonCode = BusinessReasonCodeMapper.Map(content);
                }
                else if (reader.Is(CimMarketDocumentConstants.SenderId))
                {
                    var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                    documentDto.Sender.Id = content;
                }
                else if (reader.Is(CimMarketDocumentConstants.SenderBusinessProcessRole))
                {
                    var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                    documentDto.Sender.BusinessProcessRole = MarketParticipantRoleMapper.Map(content);
                }
                else if (reader.Is(CimMarketDocumentConstants.RecipientId))
                {
                    var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                    documentDto.Receiver.Id = content;
                }
                else if (reader.Is(CimMarketDocumentConstants.RecipientBusinessProcessRole))
                {
                    var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                    documentDto.Receiver.BusinessProcessRole = MarketParticipantRoleMapper.Map(content);
                }
                else if (reader.Is(CimMarketDocumentConstants.CreatedDateTime))
                {
                    documentDto.CreatedDateTime = await reader.ReadValueAsNodaTimeAsync().ConfigureAwait(false);
                }
                else if (reader.IsElement())
                {
                    // CIM does not have the payload in a separate element,
                    // so we have to assume that the first unknown element
                    // is the start of the specialized documentDto
                    break;
                }
            }
        }
    }
}
