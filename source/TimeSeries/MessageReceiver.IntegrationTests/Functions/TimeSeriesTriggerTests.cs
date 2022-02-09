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
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using Energinet.DataHub.TimeSeries.MessageReceiver.IntegrationTests.Assets;
using Energinet.DataHub.TimeSeries.MessageReceiver.IntegrationTests.Fixtures;
using FluentAssertions;
using Microsoft.Identity.Client;
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
        public async Task When_RequestReceivedWithNoJwtToken_Then_UnauthorizedResponseReturned()
        {
            var content = _testDocuments.ValidTimeSeries;
            using var request = await CreateTimeSeriesHttpRequest(false, content).ConfigureAwait(false);
            var response = await Fixture.HostManager.HttpClient.SendAsync(request).ConfigureAwait(false);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task When_RequestReceivedWithJwtToken_Then_AcceptedResponseReturned()
        {
            var content = _testDocuments.ValidTimeSeries;
            using var request = await CreateTimeSeriesHttpRequest(true, content).ConfigureAwait(false);
            var response = await Fixture.HostManager.HttpClient.SendAsync(request).ConfigureAwait(false);
            response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        }

        [Fact]
        public async Task When_RequestReceivedWithJwtTokenAndSchemaInvalid_Then_BadRequestResponseReturned()
        {
            var content = _testDocuments.InvalidTimeSeries;
            using var request = await CreateTimeSeriesHttpRequest(true, content).ConfigureAwait(false);
            var response = await Fixture.HostManager.HttpClient.SendAsync(request).ConfigureAwait(false);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        private async Task<HttpRequestMessage> CreateTimeSeriesHttpRequest(bool includeJwtToken, string content)
        {
            const string requestUri = "api/" + TimeSeriesFunctionNames.TimeSeriesIngestion;
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

            if (includeJwtToken)
            {
                var confidentialClientApp = CreateConfidentialClientApp();
                var result = await confidentialClientApp
                    .AcquireTokenForClient(Fixture.AuthorizationConfiguration.BackendAppScope).ExecuteAsync()
                    .ConfigureAwait(false);
                request.Headers.Add("Authorization", $"Bearer {result.AccessToken}");
            }

            request.Content = new StringContent(content);
            return request;
        }

        private IConfidentialClientApplication CreateConfidentialClientApp()
        {
            var (teamClientId, teamClientSecret) = Fixture.AuthorizationConfiguration.ClientCredentialsSettings;

            var confidentialClientApp = ConfidentialClientApplicationBuilder
                .Create(teamClientId)
                .WithClientSecret(teamClientSecret)
                .WithAuthority(new Uri($"https://login.microsoftonline.com/{Fixture.AuthorizationConfiguration.B2cTenantId}"))
                .Build();

            return confidentialClientApp;
        }
    }
}
