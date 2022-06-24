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

from select import select
from package.schemas.eventhub_timeseries_schema import eventhub_timeseries_schema
from pyspark.sql.functions import from_json, explode, when, col, to_timestamp, expr, year, month, dayofmonth, lit
from pyspark.sql.dataframe import DataFrame
from package.codelists import Resolution
from package.codelists import Colname


def transform_unprocessed_time_series_to_points(source: DataFrame) -> DataFrame:
    set_time_func = when(col("Resolution") == Resolution.quarter, expr("StartDateTime + make_interval(0, 0, 0, 0, 0, TimeToAdd, 0)")) \
        .when(col("Resolution") == Resolution.hour, expr("StartDateTime + make_interval(0, 0, 0, 0, TimeToAdd, 0, 0)")) \
        .when(col("Resolution") == Resolution.day, expr("StartDateTime + make_interval(0, 0, 0, TimeToAdd, 0, 0, 0)")) \
        .when(col("Resolution") == Resolution.month, expr("StartDateTime + make_interval(0, TimeToAdd, 0, 0, 0, 0, 0)"))

    df = (source.select(
          col("*"),
          explode("Period.Points").alias("Points")
          )
          .select(
              "MeteringPointId",
              "TransactionId",
              col("Points.quantity").alias("quantity"),
              col("Points.quality").alias("quality"),
              col("Points.position").alias("position"),
              col("Period.resolution").alias("resolution"),
              col("Period.StartDateTime").alias("StartDateTime"),
              "RegistrationDateTime"
          )
          .withColumn("TimeToAdd",
                      when(col("resolution") == Resolution.quarter, (col("position") - 1) * 15)
                      .otherwise(col("position") - 1))
          .withColumn("time", set_time_func)
          .withColumn(Colname.year, year(col(Colname.time)))
          .withColumn(Colname.month, month(col(Colname.time)))
          .withColumn(Colname.day, dayofmonth(col(Colname.time)))
          .drop("position" "StartDateTime", "TimeToAdd"))

    return df
