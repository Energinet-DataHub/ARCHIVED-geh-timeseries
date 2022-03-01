# FROM 3rdman/dotnet-spark:interactive-latest
FROM 3rdman/dotnet-spark

ENV SPARK_SUBMIT_PACKAGES="org.apache.spark:spark-sql-kafka-0-10_2.12:3.2.1"

RUN apt-get update; \
    apt-get install -y apt-transport-https && \
    apt-get update && \
    apt-get install -y dotnet-sdk-6.0