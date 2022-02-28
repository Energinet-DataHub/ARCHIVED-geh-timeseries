using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Spark.Sql;
using Microsoft.Spark.Sql.Streaming;

namespace TimeSeriesPersister
{
    /// <summary>
    /// See https://github.com/dotnet/spark/blob/main/docs/getting-started/windows-instructions.md.
    ///
    /// For installation of Apache Spark on Windows see either of these:
    /// - https://phoenixnap.com/kb/install-spark-on-windows-10
    /// - https://towardsdatascience.com/installing-apache-pyspark-on-windows-10-f5f0c506bea1
    ///
    /// After installation try:
    /// - Start pyspark by executing the command "pyspark" in a command shell
    /// - The navigate to http://localhost:4040
    /// - Run the integration test
    /// - See what's stored in the Delta Lake using Azure Storage Explorer:
    ///   https://azure.microsoft.com/en-us/features/storage-explorer/#overview
    /// </summary>
    public static class Program
    {
        public static void Main()
        {
            // var spark = SparkSession.Builder().GetOrCreate();
            // var tablePath = Environment.GetEnvironmentVariable("TIME_SERIES_POINTS_TABLE_PATH");
            //
            // var df = spark.Read().Json("people.json");
            // df.Show();
            // df.Write().Mode(SaveMode.Append).Parquet(tablePath);
            var spark = SparkSession.Builder().GetOrCreate();

            var timeseries_unprocessed_path = "TODO";

            // https://docs.microsoft.com/en-us/dotnet/spark/how-to-guides/connect-to-event-hub
            var eventHubConnectionString = Environment.GetEnvironmentVariable("EVENT_HUB_CONNECTION_STRING");

            // TODO BJARKE: Extract hostname from connection string: Endpoint=sb://evhns-integrationstest-u-002.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=*******
            string bootstrapServers = "evhns-integrationstest-u-002.servicebus.windows.net:9093"; // 9093 is the port used to communicate with Event Hubs, see [troubleshooting guide](https://docs.microsoft.com/azure/event-hubs/troubleshooting-guide)
            string eh_sasl = $"org.apache.kafka.common.security.plain.PlainLoginModule required username=\"$ConnectionString\" password=\"{eventHubConnectionString}\";";

            var streamingDf = spark
                .ReadStream()
                .Format("kafka")
                .Option("kafka.bootstrap.servers", bootstrapServers)
                .Option("subscribe", "spark-test")
                .Option("kafka.sasl.mechanism", "PLAIN")
                .Option("kafka.security.protocol", "SASL_SSL")
                .Option("kafka.sasl.jaas.config", eh_sasl)
                .Option("kafka.request.timeout.ms", "60000")
                .Option("kafka.session.timeout.ms", "60000")
                .Option("failOnDataLoss", "false")
                .Load();

            var checkpointPath = "my_checkpoint";

            //var checkpointPath = $"abfss://{delta_lake_container_name}@{storage_account_name}.dfs.core.windows.net/checkpoint"
            streamingDf
                .WriteStream()
                .Option("checkpointLocation", checkpointPath)
                .ForeachBatch((df, epochId) => ProcessEventhubItem(df, epochId, timeseries_unprocessed_path))
                .Start();

            // streamingDf
            //     .WriteStream()
            //     .OutputMode(OutputMode.Append)
            //     .Format("console")
            //     .Start();
        }

        public static void ProcessEventhubItem(DataFrame df, long epochId, string timeseriesUnprocessedPath)
        {
            if (df == null) throw new ArgumentNullException(nameof(timeseriesUnprocessedPath));

            df = df
                .WithColumn("year", Functions.Year(df["enqueuedTime"]))
                .WithColumn("month", Functions.Month(df["enqueuedTime"]))
                .WithColumn("day", Functions.DayOfMonth(df["enqueuedTime"]));

            // df
            //     .Write()
            //     .PartitionBy("year", "month", "day")
            //     .Format("delta")
            //     .Mode(SaveMode.Append)
            //     .Save(timeseries_unprocessed_path);
            df.Show();
            Thread.Sleep(30000);
            Console.Write(epochId);
        }
    }
}
