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
using System.Threading.Tasks;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using Energinet.DataHub.TimeSeries.MessageReceiver.IntegrationTests.Fixtures;
using Energinet.DataHub.TimeSeries.MessageReceiver.IntegrationTests.TestHelpers;
using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace Energinet.DataHub.TimeSeries.MessageReceiver.IntegrationTests
{
    [IntegrationTest]
    [Collection(nameof(TimeSeriesFunctionAppCollectionFixture))]
    public class HealthCheckTests : FunctionAppTestBase<TimeSeriesFunctionAppFixture>
    {
        public HealthCheckTests(TimeSeriesFunctionAppFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture, testOutputHelper)
        {
        }

        [Fact]
        public async Task When_RequestLivenessStatus_Then_ResponseIsOkAndHealthy()
        {
            // Arrange
            var requestMessage = HttpRequestGenerator.CreateHttpGetRequest("api/monitor/live");

            // Act
            var actualResponse = await Fixture.HostManager.HttpClient.SendAsync(requestMessage.Request).ConfigureAwait(false);

            // Assert
            actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var actualContent = await actualResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            actualContent.Should().Be(Enum.GetName(typeof(HealthStatus), HealthStatus.Healthy));
        }

        [Fact]
        public async Task When_RequestReadinessStatus_Then_ResponseIsOkAndHealthy()
        {
            // Arrange
            var requestMessage = HttpRequestGenerator.CreateHttpGetRequest("api/monitor/ready");

            // Act
            var actualResponse = await Fixture.HostManager.HttpClient.SendAsync(requestMessage.Request).ConfigureAwait(false);

            // Assert
            actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var actualContent = await actualResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            actualContent.Should().Be(Enum.GetName(typeof(HealthStatus), HealthStatus.Healthy));
        }
    }
}
