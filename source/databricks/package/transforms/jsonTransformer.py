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
from pyspark.sql.functions import (
    from_json,
    explode,
    when,
    col,
    to_timestamp,
    expr,
    year,
    month,
    dayofmonth,
    lit,
    current_timestamp,
)
from pyspark.sql.dataframe import DataFrame
from package.codelists import Resolution
from package.codelists import Colname


def transform_unprocessed_time_series_to_points(source: DataFrame) -> DataFrame:
    set_time_func = (
        when(
            col("Resolution") == Resolution.quarter,
            expr(
                f"{Colname.start_datetime} + make_interval(0, 0, 0, 0, 0, TimeToAdd, 0)"
            ),
        )
        .when(
            col("Resolution") == Resolution.hour,
            expr(
                f"{Colname.start_datetime} + make_interval(0, 0, 0, 0, TimeToAdd, 0, 0)"
            ),
        )
        .when(
            col("Resolution") == Resolution.day,
            expr(
                f"{Colname.start_datetime} + make_interval(0, 0, 0, TimeToAdd, 0, 0, 0)"
            ),
        )
        .when(
            col("Resolution") == Resolution.month,
            expr(
                f"{Colname.start_datetime} + make_interval(0, TimeToAdd, 0, 0, 0, 0, 0)"
            ),
        )
    )

    df = (
        source.select(col("*"), explode("Period.Points").alias("Points"))
        .select(
            col("MeteringPointId").alias(Colname.metering_point_id),
            col("TransactionId").alias(Colname.transaction_id),
            col("Points.quantity").alias(Colname.quantity),
            col("Points.quality").alias(Colname.quality),
            col("Points.position").alias(Colname.position),
            col("Period.resolution").alias(Colname.resolution),
            col("Period.StartDateTime").alias(Colname.start_datetime),
            col("RegistrationDateTime").alias(Colname.registration_date_time),
        )
        .withColumn(
            Colname.registration_date_time,
            when(
                col(Colname.registration_date_time) == "", col(Colname.start_datetime)
            ).otherwise(col(Colname.registration_date_time)),
        )
        .withColumn(
            "TimeToAdd",
            when(
                col("Resolution") == Resolution.quarter, (col("Position") - 1) * 15
            ).otherwise(col("Position") - 1),
        )
        .withColumn("storedTime", current_timestamp())
        .withColumn(Colname.time, set_time_func)
        .withColumn(Colname.year, year(col(Colname.time)))
        .withColumn(Colname.month, month(col(Colname.time)))
        .withColumn(Colname.day, dayofmonth(col(Colname.time)))
        .drop("Position" "StartDateTime", "TimeToAdd")
    )

    return df
