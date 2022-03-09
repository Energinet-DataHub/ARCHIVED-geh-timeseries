# FROM 3rdman/dotnet-spark:interactive-latest
FROM johevemi/dotnet-spark:2.1.0-3.2.1

ENV SPARK_SUBMIT_PACKAGES="org.apache.spark:spark-sql-kafka-0-10_2.12:3.2.1","io.delta:delta-core_2.12:1.1.0","org.apache.hadoop:hadoop-azure:3.3.2","org.apache.hadoop:hadoop-common:3.3.2"

RUN apt-get update; \
    apt-get install -y apt-transport-https && \
    apt-get update && \
    apt-get install -y dotnet-sdk-6.0