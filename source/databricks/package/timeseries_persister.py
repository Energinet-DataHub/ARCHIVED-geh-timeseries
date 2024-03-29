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

from pyspark.sql import SparkSession, DataFrame
from pyspark.sql.types import StringType, StructType
from pyspark.sql.functions import year, month, dayofmonth, current_timestamp


def process_raw_timeseries(df, epoch_id, time_series_unprocessed_path):
    """
    Store received time series partitioned by the time of receival.

    Time of receival is currently defined as the time the messages are enqueued
    on the Blobstorage.
    """

    df = (
        df.withColumn("storedTime", current_timestamp())
        .withColumn("year", year("storedTime"))
        .withColumn("month", month("storedTime"))
        .withColumn("day", dayofmonth("storedTime"))
    )

    (
        df.write.partitionBy("year", "month", "day")
        .format("parquet")
        .mode("append")
        .save(time_series_unprocessed_path)
    )


def timeseries_persister(
    streamingDf: DataFrame, checkpoint_path: str, timeseries_unprocessed_path: str
):
    return (
        streamingDf.writeStream.option("checkpointLocation", checkpoint_path)
        .foreachBatch(
            lambda df, epochId: process_raw_timeseries(
                df, epochId, timeseries_unprocessed_path
            )
        )
        .start()
    )
