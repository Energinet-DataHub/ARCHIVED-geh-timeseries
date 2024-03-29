﻿// Copyright 2020 Energinet DataHub A/S
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
        public string ValidTimeSeries => GetDocumentAsString("TimeSeriesXMLDocuments.Valid_Hourly_CIM_TimeSeries.xml");

        public string InvalidTimeSeriesMissingId => GetDocumentAsString("TimeSeriesXMLDocuments.Invalid_Hourly_CIM_TimeSeries_missing_mRID.xml");

        public Stream InvalidTimeSeriesMissingIdAsStream => GetDocumentStream("TimeSeriesXMLDocuments.Invalid_Hourly_CIM_TimeSeries_missing_mRID.xml");

        public string ValidMultipleTimeSeriesMissingId => GetDocumentAsString("TimeSeriesXMLDocuments.Valid_Hourly_CIM_MultipleTimeSeries.xml");

        public Stream ValidMultipleTimeSeries => GetDocumentStream("TimeSeriesXMLDocuments.Valid_Hourly_CIM_MultipleTimeSeries.xml");

        public Stream ValidMultipleTimeSeriesMissingQuality => GetDocumentStream("TimeSeriesXMLDocuments.Valid_Hourly_CIM_MultipleTimeSeries_With_Missing_Quality.xml");

        public Stream ValidMultipleTimeSeriesMissingQuantity => GetDocumentStream("TimeSeriesXMLDocuments.Valid_Hourly_CIM_MultipleTimeSeries_With_Missing_Quantity.xml");

        public string TimeSeriesBundleJsonAsString => GetDocumentAsString("TimeSeriesBundleJson.Time_Series_Bundle.json");

        public string DatabricksClustersListResponse => GetDocumentAsString("DatabricksClustersListResponse.json");

        public string ValidMultipleTimeSeriesAsStringWithGuid(string baseFileName) => GetDocumentAsStringReplaceIdWithGuid("TimeSeriesXMLDocuments.Valid_Hourly_CIM_MultipleTimeSeries.xml", baseFileName, "C1876453");

        public string TimeSeriesBundleJsonAsStringWithGuid(string baseFileName) => GetDocumentAsStringReplaceIdWithGuid("TimeSeriesBundleJson.Time_Series_Bundle.json", baseFileName, "C1876453");

        private string GetDocumentAsStringReplaceIdWithGuid(string documentName, string baseFileName, string oldBaseFileName)
        {
            var document = GetDocumentAsString(documentName);

            return document.Replace(oldBaseFileName, baseFileName);
        }

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
            return assembly.GetManifestResourceStream($"{rootNamespace}.{documentName}")!;
        }
    }
}
