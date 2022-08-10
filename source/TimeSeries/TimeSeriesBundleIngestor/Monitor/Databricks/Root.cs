using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Energinet.DataHub.TimeSeries.TimeSeriesBundleIngestor.Monitor.Databricks;

public class Root
{
    [JsonPropertyName("clusters")]
    public List<Cluster>? Clusters { get; set; }
}
