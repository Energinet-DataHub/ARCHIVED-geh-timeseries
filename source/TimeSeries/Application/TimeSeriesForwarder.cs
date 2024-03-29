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

using System.Net;
using System.Threading.Tasks;
using Energinet.DataHub.TimeSeries.Application.Dtos;
using Energinet.DataHub.TimeSeries.Infrastructure.Blob;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.TimeSeries.Application
{
    public class TimeSeriesForwarder : ITimeSeriesForwarder
    {
        private readonly IRawTimeSeriesStorageClient _rawTimeSeriesStorageClient;
        private readonly ITimeSeriesBundleConverter _timeSeriesBundleConverter;
        private readonly TimeSeriesRawFolderOptions _timeSeriesRawFolderOptions;

        public TimeSeriesForwarder(
            ITimeSeriesBundleConverter timeSeriesBundleConverter,
            IRawTimeSeriesStorageClient rawTimeSeriesStorageClient,
            IOptions<TimeSeriesRawFolderOptions> options)
        {
            _timeSeriesBundleConverter = timeSeriesBundleConverter;
            _rawTimeSeriesStorageClient = rawTimeSeriesStorageClient;
            _timeSeriesRawFolderOptions = options.Value;
        }

        public async Task HandleAsync(TimeSeriesBundleDto timeSeriesBundle)
        {
            var folder = _timeSeriesRawFolderOptions.FolderName;

            // Prevent blob names with invalid format causing errors like the following by using HTTP url encoding:
            //    URISyntaxException: Relative path in absolute URI: actor=8200000007739-document=DocId2022-08-08T09:22:20.459Z.json
            // This problem may be solved when validation of RSM-012 messages is being added to the domain.
            var blobName = $"{folder}/actor={WebUtility.UrlEncode(timeSeriesBundle.Document.Sender.Id)}-document={WebUtility.UrlEncode(timeSeriesBundle.Document.Id)}.json";

            await using var outputStream = await _rawTimeSeriesStorageClient.OpenWriteAsync(blobName);
            await _timeSeriesBundleConverter.ConvertAsync(timeSeriesBundle, outputStream);
        }
    }
}
