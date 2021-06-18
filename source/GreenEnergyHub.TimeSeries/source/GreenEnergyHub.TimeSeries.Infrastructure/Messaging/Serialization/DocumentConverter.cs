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

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Xml;
using GreenEnergyHub.Messaging.Transport;
using GreenEnergyHub.TimeSeries.Domain.Common;

namespace GreenEnergyHub.TimeSeries.Infrastructure.Messaging.Serialization
{
    public abstract class DocumentConverter
    {
        public async Task<IInboundMessage> ConvertAsync([NotNull] XmlReader reader)
        {
            var document = await ParseDocumentAsync(reader).ConfigureAwait(false);

            var message = await ConvertSpecializedContentAsync(reader, document).ConfigureAwait(false);

            return await Task.FromResult(message).ConfigureAwait(false);
        }

        protected abstract Task<IInboundMessage> ConvertSpecializedContentAsync(XmlReader reader, Document document);

        protected abstract string GetNameSpace();

        private static bool RootElementNotFound(XmlReader reader, string rootElement, string rootNamespace)
        {
            return reader.NodeType != XmlNodeType.Element
                   && rootElement.Length == 0
                   && rootNamespace.Length == 0;
        }

        private static bool IfRootElementIsNotAssigned(string rootElement, string rootNamespace)
        {
            return rootElement.Length == 0 && rootNamespace.Length == 0;
        }

        private static async Task ParseFieldsAsync(XmlReader reader, Document document)
        {
            string rootElement = string.Empty;
            string ns = string.Empty;

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                if (RootElementNotFound(reader, rootElement, ns))
                {
                    continue;
                }

                if (IfRootElementIsNotAssigned(rootElement, ns))
                {
                    rootElement = reader.LocalName;
                    ns = reader.NamespaceURI;
                }
                else if (reader.Is("mRID", ns))
                {
                    var content = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                    document.Id = content;
                }
                else if (reader.IsElement())
                {
                    break;
                }
            }
        }

        private async Task<Document> ParseDocumentAsync(XmlReader reader)
        {
            var document = new Document();

            var ns = GetNameSpace();
            // var rootElement = "NotifyValidatedMeasureData_MarketDocument";
            // reader.ReadToFollowing(rootElement, ns);
            await ParseFieldsAsync(reader, document).ConfigureAwait(false);

            return await Task.FromResult(document).ConfigureAwait(false);
        }
    }
}
