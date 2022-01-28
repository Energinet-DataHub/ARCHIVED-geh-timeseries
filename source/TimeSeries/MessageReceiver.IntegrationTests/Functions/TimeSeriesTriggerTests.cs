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
            using var request = await CreateValidTimeSeriesHttpRequest().ConfigureAwait(false);
            var response = await Fixture.HostManager.HttpClient.SendAsync(request).ConfigureAwait(false);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        private async Task<HttpRequestMessage> CreateValidTimeSeriesHttpRequest()
        {
            var xmlString = _testDocuments.ValidTimeSeries;
            const string requestUri = "api/" + TimeSeriesFunctionNames.TimeSeriesIngestion;
            var confidentialClientApp = CreateConfidentialClientApp();
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

            var result = await confidentialClientApp.AcquireTokenForClient(Fixture.AuthorizationConfiguration.BackendAppScope).ExecuteAsync().ConfigureAwait(false);
            request.Headers.Add("Authorization", $"Bearer {result.AccessToken}");
            request.Content = new StringContent(xmlString);
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
