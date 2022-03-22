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
from pyspark.sql.types import StringType
from pyspark.sql.functions import year, month, dayofmonth


def process_eventhub_item(df, epoch_id, time_series_unprocessed_path):
    """
    epoch_id is required in function signature, but not used
    """

    if len(df.head(1)) == 0:
        return

    df = df.withColumn("year", year(df.enqueuedTime)) \
        .withColumn("month", month(df.enqueuedTime)) \
        .withColumn("day", dayofmonth(df.enqueuedTime))

    df.write \
        .partitionBy("year", "month", "day") \
        .format("delta") \
        .mode("append") \
        .save(time_series_unprocessed_path)


def timeseries_persister(streamingDf: DataFrame, checkpoint_path: str, timeseries_unprocessed_path: str):
    return (streamingDf
            .writeStream
            .option("checkpointLocation", checkpoint_path)
            .foreachBatch(
                lambda df,
                epochId: process_eventhub_item(df, epochId, timeseries_unprocessed_path))
            .start())
