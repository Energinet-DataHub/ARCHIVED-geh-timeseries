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

namespace Energinet.DataHub.TimeSeries.TestCore.Assets
{
    public class TestDocuments
    {
        public string ValidTimeSeries => GetDocumentAsString("Valid_Hourly_CIM_TimeSeries.xml");

        public string InvalidTimeSeriesMissingId => GetDocumentAsString("Invalid_Hourly_CIM_TimeSeries_missing_mRID.xml");

        public string ValidMultipleTimeSeriesMissingId => GetDocumentAsString("Valid_Hourly_CIM_MultipleTimeSeries.xml");

        public Stream ValidMultipleTimeSeries => GetDocumentStream("Valid_Hourly_CIM_MultipleTimeSeries.xml");

        public Stream InValidMultipleTimeSeriesMissingQuality => GetDocumentStream("Valid_Hourly_CIM_MultipleTimeSeries_With_Missing_Quality.xml");

        public Stream InValidMultipleTimeSeriesMissingQuantity => GetDocumentStream("Valid_Hourly_CIM_MultipleTimeSeries_With_Missing_Quantity.xml");

        private string GetDocumentAsString(string documentName)
        {
            var stream = GetDocumentStream(documentName);
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        private Stream GetDocumentStream(string documentName)
        {
            var rootNamespace = GetType().Namespace;
            var assembly = GetType().Assembly;
            return assembly.GetManifestResourceStream($"{rootNamespace}.{documentName}") !;
        }
    }
}
