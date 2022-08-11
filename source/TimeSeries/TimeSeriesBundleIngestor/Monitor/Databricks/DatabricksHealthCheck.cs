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
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.TimeSeries.TimeSeriesBundleIngestor.Monitor.Databricks;

public class DatabricksHealthCheck : IHealthCheck
{
    private readonly IDatabricksHealthCheckClient _healthCheckClient;
    private readonly string[] _jobNames;
    private readonly ILogger<DatabricksHealthCheck> _logger;

    public DatabricksHealthCheck(string[] jobNames, IDatabricksHealthCheckClient healthCheckClient, ILogger<DatabricksHealthCheck> logger)
    {
        _jobNames = jobNames;
        _healthCheckClient = healthCheckClient;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        Root? response;

        try
        {
            response = await _healthCheckClient.GetClustersAsync();
        }
        catch (HttpRequestException e)
        {
            return HealthCheckResult.Unhealthy("Was not able to get a successfully response from Databricks API StatusCode: " + e.StatusCode + " - " + e.Message);
        }

        if (response?.Clusters == null)
            return HealthCheckResult.Unhealthy("Failed to get clusters from Databricks API");

        var notRunningJobs = new List<string>();
        var runningJobs = new List<string>();

        foreach (var jobName in _jobNames)
        {
            var jobsWithRunningState = response.Clusters.Where(x => x.State == "RUNNING" && x.DefaultTags?.RunName != null && x.DefaultTags.RunName.StartsWith(jobName)).ToList();

            if (jobsWithRunningState.Any())
            {
                runningJobs.Add(jobName);
            }
            else
            {
                notRunningJobs.Add(jobName);
            }
        }

        if (!notRunningJobs.Any())
        {
            return HealthCheckResult.Healthy($@"Running jobs {string.Join(", ", runningJobs)}");
        }

        return HealthCheckResult.Unhealthy($@"Not running jobs {string.Join(", ", notRunningJobs)} and running jobs {string.Join(", ", runningJobs)}");
    }
}
