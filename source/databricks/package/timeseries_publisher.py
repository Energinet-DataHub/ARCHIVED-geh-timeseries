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
from pyspark.sql.functions import col, year, month, dayofmonth, when, lit, min, max
from pyspark.sql.types import BooleanType
from package.transformations import transform_unprocessed_time_series_to_points
from package.schemas import time_series_unprocessed_schema


def publish_timeseries_batch(
    unprocessed_time_series_df, epoch_id, time_series_points_path
):
    """
    Transform raw timeseries into timeseries with defined schema suited for aggregations.
    The table is partitioned by the time of the actual consumption/production/exchange.
    """

    (
        transform_unprocessed_time_series_to_points(unprocessed_time_series_df)
        .write.partitionBy("year", "month", "day")
        .mode("append")
        .format("parquet")
        .save(time_series_points_path)
    )


def timeseries_publisher(
    spark: SparkSession,
    time_series_unprocessed_path: str,
    time_series_checkpoint_path: str,
    time_series_points_path: str,
):

    return (
        spark.readStream.schema(time_series_unprocessed_schema)
        .format("parquet")
        .load(time_series_unprocessed_path)
        .writeStream.option("checkpointLocation", time_series_checkpoint_path)
        .foreachBatch(
            lambda df, epochId: publish_timeseries_batch(
                df, epochId, time_series_points_path
            )
        )
        .start()
    )
