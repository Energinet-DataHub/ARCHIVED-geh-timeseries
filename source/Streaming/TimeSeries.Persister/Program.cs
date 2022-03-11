using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Spark.Sql;
using Microsoft.Spark.Sql.Types;

namespace TimeSeries.Persister
{
    internal class Program
    {
        public static void Main()
        {
            // Event hub stuff
            var eventHubsNamespace = "eh6173";
            var eventHubsInstance = "jvm34";
            var eventHubConnectionStringWithSharedAccessKeyAndEntityPath = "Endpoint=sb://eh6173.servicebus.windows.net/;SharedAccessKeyName=manage;SharedAccessKey=0c3dyQi5iVMYm9WzRdYnUO4U34lC8hASAKPT8ImZV58=;EntityPath=jvm34";

            // Storage stuff
            var storageAccountKey = "z6cSayj6VWI6JIcaJoPZdYv25oDyNcLfKQUeRs5wAX2Hm886E5ca7cH+sPaOw8L6pxo0UzhmeZJn+ASt8UxTZw==";
            var storageAccountName = "jvmsstore234";
            var delta_lake_container_name = "delta";
            var blobName = "test";

            Environment.SetEnvironmentVariable("EVENT_HUB_CONNECTION_STRING", eventHubConnectionStringWithSharedAccessKeyAndEntityPath);
            var eventHubConnectionString = Environment.GetEnvironmentVariable("EVENT_HUB_CONNECTION_STRING");
            string bootstrapServers = $"{eventHubsNamespace}.servicebus.windows.net:9093"; // 9093 is the port used to communicate with Event Hubs, see [troubleshooting guide](https://docs.microsoft.com/azure/event-hubs/troubleshooting-guide)
            string eh_sasl = $"org.apache.kafka.common.security.plain.PlainLoginModule required username=\"$ConnectionString\" password=\"{eventHubConnectionString}\";";

            var path = $"abfss://{delta_lake_container_name}@{storageAccountName}.dfs.core.windows.net/{blobName}";
            var spark = SparkSession.Builder().Config($"fs.azure.account.key.{storageAccountName}.dfs.core.windows.net", storageAccountKey).GetOrCreate();

            Console.WriteLine(eh_sasl);

            var streamingDf = spark
                .ReadStream()
                .Format("kafka")
                .Option("kafka.bootstrap.servers", bootstrapServers)
                .Option("subscribe", eventHubsInstance)
                .Option("kafka.sasl.mechanism", "PLAIN")
                .Option("kafka.security.protocol", "SASL_SSL")
                .Option("kafka.sasl.jaas.config", eh_sasl)
                .Option("kafka.request.timeout.ms", "60000")
                .Option("kafka.session.timeout.ms", "60000")
                //.Option("failOnDataLoss", "false")
                //.Option("checkpointLocation", "/tmp/kafka_cp.txt")
                .Load()
                .WriteStream()
                .ForeachBatch((df, id) =>
                {
                    Console.WriteLine($"batch id: {id}");
                    df.Show();
                    if (df.Head(1).Any())
                    {
                        df = df
                        .WithColumn("year", Functions.Year(df["timestamp"]))
                        .WithColumn("month", Functions.Month(df["timestamp"]))
                        .WithColumn("day", Functions.DayOfMonth(df["timestamp"]));
                        df
                        .Write()
                        .PartitionBy("year", "month", "day")
                        .Format("delta")
                        .Mode(SaveMode.Append)
                        .Save(path); //"/tmp/delta-table"
                    }
                })
                .Start();

            streamingDf.AwaitTermination();
        }

        public static void ProcessEventhubItem(DataFrame df, long epochId)
        {
            Console.WriteLine($"Hello {epochId}");
            df
                .Write()
                .Format("delta")
                .Mode(SaveMode.Append)
                .Save($"abfss://delta@jvmsstore234.dfs.core.windows.net/delta");
            df.Show();
        }

        private static T Deserialize<T>(byte[] param)
        {
            using (var ms = new MemoryStream(param))
            {
                IFormatter br = new BinaryFormatter();
                return (T)br.Deserialize(ms);
            }
        }
    }
}
