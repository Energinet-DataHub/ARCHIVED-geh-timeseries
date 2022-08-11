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

using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Energinet.DataHub.TimeSeries.TimeSeriesBundleIngestor;
using Energinet.DataHub.TimeSeries.TimeSeriesBundleIngestor.Monitor.Databricks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.TimeSeries.UnitTests;

[UnitTest]
public class DatabricksHealthCheckTests
{
    [Theory]
    [InlineData("RUNNING", "RUNNING", HealthStatus.Healthy)]
    [InlineData("TERMINATED", "TERMINATED", HealthStatus.Unhealthy)]
    [InlineData("RUNNING", "TERMINATED", HealthStatus.Unhealthy)]
    public async Task GivenTwoDatabricksTaskRuns_Return_CorrectHealthStatus(string state1, string state2, HealthStatus healthStatus)
    {
        var loggerMock = new Mock<ILogger<DatabricksHealthCheck>>();
        var databricksClientMock = new Mock<IDatabricksHealthCheckClient>();
        databricksClientMock.Setup(x => x.GetClustersAsync()).ReturnsAsync(new Root
        {
            Clusters = new List<Cluster>
            {
                new()
                {
                    State = state1,
                    ClusterId = "0803-094100-sgoe7sd9",
                    DefaultTags = new DefaultTags
                    {
                        JobId = "412860718888051",
                        ClusterName = "job-412860718888051-run-167631",
                        RunName = "persister_streaming_job_CD76C9E4-8025-4C22-AB48-8EB32A23E668",
                    },
                },
                new()
                {
                    State = state2,
                    ClusterId = "0803-094100-sgoe7sd9",
                    DefaultTags = new DefaultTags
                    {
                        JobId = "412860718888051",
                        ClusterName = "job-412860718888051-run-167631",
                        RunName = "publisher_streaming_job_CD76C9E4-8025-4C22-AB48-8EB32A23E668",
                    },
                },
            },
        });

        var sut = new DatabricksHealthCheck(new[] { "persister_streaming_job", "publisher_streaming_job" }, databricksClientMock.Object);

        var result = await sut.CheckHealthAsync(new HealthCheckContext());
        Assert.Equal(healthStatus, result.Status);
    }

    [Fact]
    public async Task GivenDatabricksHasBadToken_Return_UnHealthy()
    {
        var loggerMock = new Mock<ILogger<DatabricksHealthCheck>>();
        var databricksClientMock = new Mock<IDatabricksHealthCheckClient>();
        databricksClientMock.Setup(x => x.GetClustersAsync()).Throws(new HttpRequestException());

        var sut = new DatabricksHealthCheck(new string[] { }, databricksClientMock.Object);

        var result = await sut.CheckHealthAsync(new HealthCheckContext());
        Assert.Equal(HealthStatus.Unhealthy, result.Status);
    }
}
