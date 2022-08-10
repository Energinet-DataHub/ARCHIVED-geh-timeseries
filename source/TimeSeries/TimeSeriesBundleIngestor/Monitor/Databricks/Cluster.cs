using System.Text.Json.Serialization;

namespace Energinet.DataHub.TimeSeries.TimeSeriesBundleIngestor.Monitor.Databricks;

public class Cluster
{
    [JsonPropertyName("cluster_id")]
    public string? ClusterId { get; set; }

    [JsonPropertyName("state")]
    public string? State { get; set; }

    [JsonPropertyName("default_tags")]
    public DefaultTags? DefaultTags { get; set; }
}
