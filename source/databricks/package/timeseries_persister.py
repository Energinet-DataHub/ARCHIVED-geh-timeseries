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
from package.codelists import Colname


def process_eventhub_item(df, epoch_id, time_series_unprocessed_path):
    """
    Store received time series partitioned by the time of receival.

    Time of receival is currently defined as the time the messages are enqueued
    on the EventHub.
    """
    df = (
        df.withColumn(Colname.registration_time, df.enqueuedTime)
        .withColumn(Colname.year, year(Colname.registration_time))
        .withColumn(Colname.month, month(Colname.registration_time))
        .withColumn(Colname.day, dayofmonth(Colname.registration_time))
        .withColumn(Colname.timeseries, df.body.cast(StringType()))
        .select(Colname.timeseries, Colname.year, Colname.month, Colname.day, Colname.registration_time)
    )

    (df
     .write.partitionBy(Colname.year, Colname.month, Colname.day)
     .format("delta")
     .mode("append")
     .save(time_series_unprocessed_path))


def timeseries_persister(streamingDf: DataFrame, checkpoint_path: str, timeseries_unprocessed_path: str):
    return (
        streamingDf.writeStream.option("checkpointLocation", checkpoint_path)
        .foreachBatch(
            lambda df, epochId: process_eventhub_item(
                df, epochId, timeseries_unprocessed_path
            )
        )
        .start()
    )
