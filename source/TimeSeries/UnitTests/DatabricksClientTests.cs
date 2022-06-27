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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.TimeSeries.TestCore.Assets;
using Energinet.DataHub.TimeSeries.TimeSeriesBundleIngestor;
using Moq;
using Moq.Protected;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.TimeSeries.UnitTests;

[UnitTest]
public class DatabricksClientTests
{
    private readonly TestDocuments _testDocuments;

    public DatabricksClientTests()
    {
        _testDocuments = new TestDocuments();
    }

    [Fact]
    public async Task SuccessfulGetClustersRequest_ParseSuccessful()
    {
        var httpClient = MockHttpClient(HttpStatusCode.OK);

        var client = new DatabricksClient(httpClient);
        var result = await client.GetClustersAsync();

        Assert.NotNull(result);
        Assert.Equal(32, result!.Clusters!.Count);

        var cluster = result.Clusters.Last();

        Assert.Equal("TERMINATED", cluster.State);
        Assert.Equal("0801-195819-8boihzib", cluster.ClusterId);
        Assert.Equal("job-266905437853558-run-155586", cluster.DefaultTags!.ClusterName);
        Assert.Equal("266905437853558", cluster.DefaultTags!.JobId);
        Assert.Equal("unique_job_af6d694e-5619-6586-c65d-a3ef9ae4869a", cluster.DefaultTags!.RunName);
    }

    [Fact]
    public async Task WrongToken_ThrowException()
    {
        var httpClient = MockHttpClient(HttpStatusCode.Forbidden);
        var client = new DatabricksClient(httpClient);
        await Assert.ThrowsAsync<HttpRequestException>(() => client.GetClustersAsync());
    }

    private HttpClient MockHttpClient(HttpStatusCode httpStatusCode)
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            // Setup the PROTECTED method to mock
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            // prepare the expected response of the mocked http call
            .ReturnsAsync(new HttpResponseMessage()
            {
                StatusCode = httpStatusCode,
                Content = new StringContent(_testDocuments.DatabricksClustersListResponse, Encoding.UTF8, "application/json"),
            })
            .Verifiable();
        // use real http client with mocked handler here
        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://unittest"),
        };
        return httpClient;
    }
}
