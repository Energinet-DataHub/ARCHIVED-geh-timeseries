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
from package.transforms import transform_unprocessed_time_series_to_points
from package.codelists import Colname
from package.schemas import time_series_points_schema
from delta.tables import DeltaTable
from package.table_creator import create_delta_table_if_empty


def publish_timeseries_batch(unprocessed_time_series_df, epoch_id, time_series_points_path):
    """
    Transform raw timeseries from eventhub into timeseries with defined schema suited for aggregations.
    The table is partitioned by the time of the actual consumption/production/exchange.
    """

    (transform_unprocessed_time_series_to_points(unprocessed_time_series_df)
     .select(
         col(Colname.metering_point_id),
         col(Colname.transaction_id),
         col(Colname.quantity),
         col(Colname.quality),
         col(Colname.time),
         col(Colname.resolution),
         col(Colname.year),
         col(Colname.month),
         col(Colname.day),
         col(Colname.registration_date_time))
     .write
     .partitionBy(
         Colname.year,
         Colname.month,
         Colname.day)
     .format("delta")
     .mode("append")
     .save(time_series_points_path))


def timeseries_publisher(spark: SparkSession, time_series_unprocessed_path: str, time_series_checkpoint_path: str, time_series_points_path: str):
    create_delta_table_if_empty(spark, time_series_points_path, time_series_points_schema, [Colname.year, Colname.month, Colname.day])

    return (spark
            .readStream
            .format("delta")
            .load(time_series_unprocessed_path)
            .writeStream
            .option("checkpointLocation", time_series_checkpoint_path)
            .foreachBatch(lambda df, epochId: publish_timeseries_batch(df, epochId, time_series_points_path))
            .start())
