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
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using Energinet.DataHub.TimeSeries.MessageReceiver.IntegrationTests.Assets;
using Energinet.DataHub.TimeSeries.MessageReceiver.IntegrationTests.Fixtures;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.TimeSeries.MessageReceiver.IntegrationTests.Functions
{
    [Collection(nameof(TimeSeriesFunctionAppCollectionFixture))]
    public class TimeSeriesTriggerTests : FunctionAppTestBase<TimeSeriesFunctionAppFixture>
    {
        private readonly TestDocuments _testDocuments;

        public TimeSeriesTriggerTests(TimeSeriesFunctionAppFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture, testOutputHelper)
        {
            _testDocuments = new TestDocuments();
        }

        [Fact]
        public async Task When_TimeSeriesTriggerFired_Then_OkResponseReturned()
        {
            using var request = CreateValidTimeSeriesHttpRequest();
            var response = await Fixture.HostManager.HttpClient.SendAsync(request).ConfigureAwait(false);
            response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        }

        private HttpRequestMessage CreateValidTimeSeriesHttpRequest()
        {
            var xmlString = _testDocuments.ValidTimeSeries;
            const string requestUri = "api/" + TimeSeriesFunctionNames.TimeSeriesIngestion;
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Content = new StringContent(xmlString);
            return request;
        }
    }
}
