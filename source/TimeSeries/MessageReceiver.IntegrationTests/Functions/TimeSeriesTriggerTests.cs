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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using Energinet.DataHub.Core.FunctionApp.TestCommon.EventHub.ListenerMock;
using Energinet.DataHub.TimeSeries.MessageReceiver.IntegrationTests.Fixtures;
using Energinet.DataHub.TimeSeries.TestCore.Assets;
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
            var content = _testDocuments.InvalidTimeSeriesMissingId;
            using var request = await CreateTimeSeriesHttpRequest(true, content).ConfigureAwait(false);
            var response = await Fixture.HostManager.HttpClient.SendAsync(request).ConfigureAwait(false);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task When_RequestIsReceived_Then_RequestAndResponseAreLogged()
        {
            // Arrange
            const string expectedHttpDataResponseType = "response";
            const string expectedHttpDataRequestType = "request";
            var content = _testDocuments.ValidTimeSeries;

            // Act
            using var request = await CreateTimeSeriesHttpRequest(true, content).ConfigureAwait(false);

            // Assert
            var blobItems = Fixture.LogContainerClient.GetBlobs().TakeLast(2).ToArray();
            var actualHttpDataRequestType = blobItems[0].Metadata["httpdatatype"];
            var actualHttpDataResponse = blobItems[1].Metadata["httpdatatype"];
            actualHttpDataRequestType.Should().Be(expectedHttpDataRequestType);
            actualHttpDataResponse.Should().Be(expectedHttpDataResponseType);
        }

        [Fact]
        public async Task When_FunctionExecuted_Then_MessageSentToEventHub()
        {
            var content = _testDocuments.ValidTimeSeries;
            using var whenAllEvent = await Fixture.EventHubListener
                .WhenAny()
                .VerifyCountAsync(1).ConfigureAwait(false);
            using var request = await CreateTimeSeriesHttpRequest(true, content).ConfigureAwait(false);
            await Fixture.HostManager.HttpClient.SendAsync(request).ConfigureAwait(false);
            var allReceived = whenAllEvent.Wait(TimeSpan.FromSeconds(5));
            allReceived.Should().BeTrue();
        }

        private async Task<HttpRequestMessage> CreateTimeSeriesHttpRequest(bool includeJwtToken, string content)
        {
            const string requestUri = "api/" + TimeSeriesFunctionNames.TimeSeriesBundleIngestor;
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
