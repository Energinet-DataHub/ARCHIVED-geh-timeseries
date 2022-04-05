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

namespace Energinet.DataHub.TimeSeries.MessageReceiver.IntegrationTests
{
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
