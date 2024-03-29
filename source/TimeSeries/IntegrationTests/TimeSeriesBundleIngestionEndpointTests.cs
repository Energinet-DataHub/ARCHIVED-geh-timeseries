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

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using Energinet.DataHub.TimeSeries.IntegrationTests.Fixtures;
using Energinet.DataHub.TimeSeries.TestCore.Assets;
using Energinet.DataHub.TimeSeries.TimeSeriesBundleIngestor;
using FluentAssertions;
using Microsoft.Identity.Client;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace Energinet.DataHub.TimeSeries.IntegrationTests
{
    [IntegrationTest]
    [Collection(nameof(TimeSeriesFunctionAppCollectionFixture))]
    public class TimeSeriesBundleIngestionEndpoint : FunctionAppTestBase<TimeSeriesFunctionAppFixture>
    {
        private readonly TestDocuments _testDocuments;

        public TimeSeriesBundleIngestionEndpoint(TimeSeriesFunctionAppFixture fixture, ITestOutputHelper testOutputHelper)
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
        public async Task When_RequestReceivedWithValidJwtToken_Then_JsonStreamUploadedToBlobStorage()
        {
            // Arrange
            var baseFileName = Guid.NewGuid().ToString();
            var senderMarketParticipantId = "5799999933317";
            var blobName = $"timeseries-raw/actor={senderMarketParticipantId}-document={baseFileName}.json";
            var expected = _testDocuments.TimeSeriesBundleJsonAsStringWithGuid(baseFileName);
            var content = _testDocuments.ValidMultipleTimeSeriesAsStringWithGuid(baseFileName);
            using var request = await CreateTimeSeriesHttpRequest(true, content).ConfigureAwait(false);

            // Act
            await Fixture.HostManager.HttpClient.SendAsync(request).ConfigureAwait(false);

            // Assert
            var response = await Fixture.TimeSeriesContainerClient.GetBlobClient(blobName).DownloadAsync();
            var actual = await new StreamReader(response.Value.Content).ReadToEndAsync();
            Assert.Equal(expected, actual, ignoreLineEndingDifferences: true);
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
        [SuppressMessage("Usage", "VSTHRD103:Call async methods when in an async method", Justification = "TODO: Fix analyzer warning")]
        public async Task When_RequestIsReceived_Then_RequestAndResponseAreLogged()
        {
            // Arrange
            const string expectedHttpDataResponseType = "response";
            const string expectedHttpDataRequestType = "request";
            var content = _testDocuments.ValidTimeSeries;
            var blobItemsBeforeRequest = Fixture.LogContainerClient.GetBlobs().ToArray();

            // Act
            using var request = await CreateTimeSeriesHttpRequest(true, content).ConfigureAwait(false);
            using var response = await Fixture.HostManager.HttpClient.SendAsync(request).ConfigureAwait(false);

            // Assert
            var blobItemsAfterRequest = Fixture.LogContainerClient.GetBlobs().OrderBy(e => e.Properties.CreatedOn).ToArray();
            var twoNewest = blobItemsAfterRequest.TakeLast(2).ToArray();

            Assert.True(blobItemsAfterRequest.Length - blobItemsBeforeRequest.Length == 2);
            twoNewest.Should().Contain(x => x.Metadata["httpdatatype"] == expectedHttpDataRequestType);
            twoNewest.Should().Contain(x => x.Metadata["httpdatatype"] == expectedHttpDataResponseType);
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
