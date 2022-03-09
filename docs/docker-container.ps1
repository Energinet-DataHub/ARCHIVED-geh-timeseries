docker run -d --name dotnet-spark -p 5567:5567 -v $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath('..\source\Streaming\TimeSeries.Persister\bin\Debug') -e DOTNET_SPARK_BACKEND_IP_ADDRESS="0.0.0.0" -e SPARK_SUBMIT_PACKAGES="org.apache.spark:spark-sql-kafka-0-10_2.12:3.2.1","io.delta:delta-core_2.12:1.1.0","org.apache.hadoop:hadoop-azure:3.3.2","org.apache.hadoop:hadoop-common:3.3.2" johevemi/dotnet-spark:2.1.0-3.2.1