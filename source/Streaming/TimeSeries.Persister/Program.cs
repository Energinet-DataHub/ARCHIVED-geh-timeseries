using System;
using System.Collections.Generic;
using Microsoft.Spark.Sql;
using Microsoft.Spark.Sql.Types;

namespace TimeSeries.Persister
{
    internal class Program
    {
        public static void Main()
        {
            string BOOTSTRAP_SERVERS = "hostname:9093"; // 9093 is the port used to communicate with Event Hubs, see [troubleshooting guide](https://docs.microsoft.com/azure/event-hubs/troubleshooting-guide)
            string EH_SASL = "org.apache.kafka.common.security.plain.PlainLoginModule required username=\"$ConnectionString\" password=\"<CONNECTION_STRING>\";"; // Connection string obtained from Step 1

            SparkSession spark = SparkSession
    .Builder()
    .AppName("Connect Event Hub")
    .GetOrCreate();

            DataFrame df = spark
                .ReadStream()
                .Format("kafka")
                .Option("kafka.bootstrap.servers", BOOTSTRAP_SERVERS)
                .Option("subscribe", "spark-test")
                .Option("kafka.sasl.mechanism", "PLAIN")
                .Option("kafka.security.protocol", "SASL_SSL")
                .Option("kafka.sasl.jaas.config", EH_SASL)
                .Option("kafka.request.timeout.ms", "60000")
                .Option("kafka.session.timeout.ms", "60000")
                .Option("failOnDataLoss", "false")
                .Load();

            DataFrame dfWrite = df
                .WriteStream()
                .OutputMode("append")
                .Format("console")
                .Start();
        }
    }
}
