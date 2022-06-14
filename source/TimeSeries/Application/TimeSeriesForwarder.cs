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
using Energinet.DataHub.Core.JsonSerialization;
using Energinet.DataHub.TimeSeries.Application.Dtos;
using Energinet.DataHub.TimeSeries.Infrastructure.Blob;

namespace Energinet.DataHub.TimeSeries.Application
{
    public class TimeSeriesForwarder : ITimeSeriesForwarder
    {
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IBlobHandler _blobHandler;

        public TimeSeriesForwarder(
            IJsonSerializer jsonSerializer,
            IBlobHandler blobHandler)
        {
            _jsonSerializer = jsonSerializer;
            _blobHandler = blobHandler;
        }

        public async Task HandleAsync(TimeSeriesBundleDto timeSeriesBundle, string connectionString)
        {
            var timeSeriesBundleToJsonConverter = new TimeSeriesBundleToJsonConverter(_jsonSerializer);
            var json = timeSeriesBundleToJsonConverter.ConvertToJson(timeSeriesBundle);
            await _blobHandler.SaveAsync(timeSeriesBundle.Document.Id, json, connectionString, "timeseries-raw").ConfigureAwait(false);
        }
    }
}
