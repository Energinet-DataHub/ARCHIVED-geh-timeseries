using System.Threading.Tasks;
using Energinet.DataHub.Core.SchemaValidation;
using Energinet.DataHub.TimeSeries.Application.Dtos;

namespace Energinet.DataHub.TimeSeries.Infrastructure.CimDeserialization.TimeSeriesBundle
{
    public class TimeSeriesBundleConverter
    {
        public async Task<TimeSeriesBundleDto> ConvertAsync(SchemaValidatingReader reader)
        {
            throw new System.NotImplementedException();
        }

        private readonly IClock _clock;

        protected DocumentConverter(IClock clock)
        {
            _clock = clock;
        }

        public async Task<IInboundMessage> ConvertAsync(SchemaValidatingReader reader)
        {
            var document = await ParseDocumentAsync(reader).ConfigureAwait(false);

            var message = await ConvertSpecializedContentAsync(reader, document).ConfigureAwait(false);

            return message;
        }

        protected abstract Task<IInboundMessage> ConvertSpecializedContentAsync(SchemaValidatingReader reader, DocumentDto document);

        private static async Task ParseFieldsAsync(SchemaValidatingReader reader, DocumentDto document)
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
                    document.Id = content;
                }
                else if (reader.Is(CimMarketDocumentConstants.Type))
                {
                    var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                    document.Type = DocumentTypeMapper.Map(content);
                }
                else if (reader.Is(CimMarketDocumentConstants.BusinessReasonCode))
                {
                    var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                    document.BusinessReasonCode = BusinessReasonCodeMapper.Map(content);
                }
                else if (reader.Is(CimMarketDocumentConstants.IndustryClassification))
                {
                    var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                    document.IndustryClassification = IndustryClassificationMapper.Map(content);
                }
                else if (reader.Is(CimMarketDocumentConstants.SenderId))
                {
                    var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                    document.Sender.Id = content;
                }
                else if (reader.Is(CimMarketDocumentConstants.SenderBusinessProcessRole))
                {
                    var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                    document.Sender.BusinessProcessRole = MarketParticipantRoleMapper.Map(content);
                }
                else if (reader.Is(CimMarketDocumentConstants.RecipientId))
                {
                    var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                    document.Recipient.Id = content;
                }
                else if (reader.Is(CimMarketDocumentConstants.RecipientBusinessProcessRole))
                {
                    var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                    document.Recipient.BusinessProcessRole = MarketParticipantRoleMapper.Map(content);
                }
                else if (reader.Is(CimMarketDocumentConstants.CreatedDateTime))
                {
                    document.CreatedDateTime = await reader.ReadValueAsNodaTimeAsync().ConfigureAwait(false);
                }
                else if (reader.IsElement())
                {
                    // CIM does not have the payload in a separate element,
                    // so we have to assume that the first unknown element
                    // is the start of the specialized document
                    break;
                }
            }
        }

        private async Task<DocumentDto> ParseDocumentAsync(SchemaValidatingReader reader)
        {
            var document = new DocumentDto()
            {
                Sender = new MarketParticipantDto(),
                Recipient = new MarketParticipantDto(),
                RequestDate = _clock.GetCurrentInstant(),
            };

            await ParseFieldsAsync(reader, document).ConfigureAwait(false);

            return document;
        }
    }
}
