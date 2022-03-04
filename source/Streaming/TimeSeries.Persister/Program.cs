using System;
using System.Linq;
using Microsoft.Spark.Sql;

namespace TimeSeries.Persister
{
    internal class Program
    {
        public static void Main()
        {
            Environment.SetEnvironmentVariable("EVENT_HUB_CONNECTION_STRING", "Endpoint=sb://evhns-timeseries-timeseries-u-001.servicebus.windows.net/;SharedAccessKeyName=listen;SharedAccessKey=lGW5ENgWGFeRhNNBhPkq4uAYrebkWhSTa0ZbvafB9cw=;EntityPath=received-timeseries");
            /////////////////////////////////////////////////////

            var spark = SparkSession.Builder().GetOrCreate();
            var timeseries_unprocessed_path = "TODO";

            var eventHubConnectionString = Environment.GetEnvironmentVariable("EVENT_HUB_CONNECTION_STRING");

            // TODO BJARKE: Extract hostname from connection string: Endpoint=sb://evhns-integrationstest-u-002.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=*******
            string bootstrapServers = "evhns-timeseries-timeseries-u-001.servicebus.windows.net:9093"; // 9093 is the port used to communicate with Event Hubs, see [troubleshooting guide](https://docs.microsoft.com/azure/event-hubs/troubleshooting-guide)
            string eh_sasl = $"org.apache.kafka.common.security.plain.PlainLoginModule required username=\"$ConnectionString\" password=\"{eventHubConnectionString}\";";
            Console.WriteLine(eh_sasl);

            var streamingDf = spark
                .ReadStream()
                .Format("kafka")
                .Option("kafka.bootstrap.servers", bootstrapServers)
                .Option("subscribe", "received-timeseries")
                .Option("kafka.sasl.mechanism", "PLAIN")
                .Option("kafka.security.protocol", "SASL_SSL")
                .Option("kafka.sasl.jaas.config", eh_sasl)
                .Option("kafka.request.timeout.ms", "60000")
                .Option("kafka.session.timeout.ms", "60000")
                //.Option("failOnDataLoss", "false")
                //.Option("checkpointLocation", "/tmp/kafka_cp.txt")
                .Load();

            //var checkpointPath = "/tmp/received_time_series_checkpoint";
            //var checkpointPath = $"abfss://{delta_lake_container_name}@{stdatalakesharedresu001}.dfs.core.windows.net/checkpoint"
            streamingDf
                .WriteStream()
                 //.Option("checkpointLocation", checkpointPath)
                .ForeachBatch((df, epochId) => ProcessEventhubItem(df, epochId, timeseries_unprocessed_path))
                .Start().AwaitTermination(60000);
        }

        public static void ProcessEventhubItem(DataFrame df, long epochId, string timeseriesUnprocessedPath)
        {
            Console.WriteLine($"Hello {epochId} {timeseriesUnprocessedPath}");
            if (df.Head(1).Any())
            {
                df.Show();
                var body = df.Select("value");
                body.Show();
            }

            // if (df == null) throw new ArgumentNullException(nameof(timeseriesUnprocessedPath));
            // df = df
            //     .WithColumn("year", Functions.Year(df["enqueuedTime"]))
            //     .WithColumn("month", Functions.Month(df["enqueuedTime"]))
            //     .WithColumn("day", Functions.DayOfMonth(df["enqueuedTime"]));
            // // df
            // //     .Write()
            // //     .PartitionBy("year", "month", "day")
            // //     .Format("delta")
            // //     .Mode(SaveMode.Append)
            // //     .Save(timeseries_unprocessed_path);
            // df.Show();
        }
    }
}
