# Copyright 2020 Energinet DataHub A/S
#
# Licensed under the Apache License, Version 2.0 (the "License2");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
from pyspark.sql import SparkSession
from pyspark.sql.functions import from_json, explode, when, col, to_timestamp, expr
from pyspark.sql.types import StringType, StructType, StructField, ArrayType
from package.schemas.eventhub_timeseries_schema import eventhub_timeseries_schema
from package.codelists import Resolution


def transform(df, epoch_id, timeseries_processed_path):
    jsonDataFrame = df.select(df.body.cast(StringType()).alias("body"))
    structured = jsonDataFrame.select(from_json(jsonDataFrame.body, eventhub_timeseries_schema).alias('json'))
    flat = structured. \
        select(explode("json.Series")). \
        select("col.MeteringPointId", "col.TransactionId", "col.Period"). \
        select(col("MeteringPointId"), col("TransactionId"), to_timestamp(col("Period.StartDateTime")).alias("StartDateTime"), col("Period.Resolution").alias("Resolution"), explode("Period.Points").alias("Period_Point")). \
        select("*", "Period_Point.Quantity", "Period_Point.Quality", "Period_Point.Position"). \
        drop("Period_Point")
    withResolutionInMinutes = flat.withColumn("ClockResolution", when(col("Resolution") == Resolution.quarter, 15).when(col("Resolution") == Resolution.hour, 60).when(col("Resolution") == Resolution.day, 1440).when(col("Resolution") == Resolution.month, 43800)).drop("Resolution")
    # TODO how do we handle month resolution ?
    timeToAdd = withResolutionInMinutes.withColumn("TimeToAdd", (col("Position") - 1) * col("ClockResolution")).drop("ClockResolution", "Position")
    withTime = timeToAdd.withColumn("Time", expr("StartDateTime + make_interval(0, 0, 0, 0, 0, TimeToAdd, 0)")).drop("StartDateTime").drop("TimeToAdd")

    withTime.write \
            .partitionBy("MeteringPointId") \
            .format("delta") \
            .mode("append") \
            .save(timeseries_processed_path)


def timeseries_transformer(delta_lake_container_name: str, storage_account_name: str, timeseries_unprocessed_path: str, timeseries_processed_path: str):

    source_path = timeseries_unprocessed_path
    spark = SparkSession.builder.getOrCreate()

    read_df = spark.readStream.format("delta").load(source_path)

    checkpoint_path = f"abfss://{delta_lake_container_name}@{storage_account_name}.dfs.core.windows.net/checkpoint-timeseries-transformer"

    read_df. \
        writeStream. \
        option("checkpointLocation", checkpoint_path). \
        foreachBatch(lambda df, epochId: transform(df, epochId, timeseries_processed_path)). \
        start()
