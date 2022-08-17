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

from pyspark.sql.functions import (
    explode,
    when,
    col,
    year,
    month,
    dayofmonth,
    current_timestamp,
    lit,
    expr,
)
from pyspark.sql import DataFrame
from pyspark.sql.types import IntegerType
from package.codelists import Resolution


def transform_unprocessed_time_series_to_points(source: DataFrame) -> DataFrame:
    df = (
        source.select(col("*"), explode("Period.Points").alias("Points"))
        .select(
            col("MeteringPointId"),
            col("TransactionId"),
            col("Points.Quantity").alias("Quantity"),
            col("Points.Quality").alias("Quality"),
            col("Points.Position").alias("Position"),
            col("Period.Resolution").alias("Resolution"),
            col("Period.StartDateTime").alias("StartDateTime"),
            col("RegistrationDateTime"),
            col("CreatedDateTime"),
        )
        .withColumn(
            "RegistrationDateTime",
            when(
                col("RegistrationDateTime").isNull(),
                col("CreatedDateTime"),
            ).otherwise(col("RegistrationDateTime")),
        )
        .withColumn("storedTime", current_timestamp())
        .withColumn(
            "Factor",
            when(
                col("Resolution") == Resolution.quarter,
                (col("Position") - 1) * 15,  # To add 15, 30 or 45 to the minut interval
            ).otherwise(
                col("Position") - 1
            ),  # Position - 1 to make the value set start from zero
        )  # Factor is used in set_time_func to add to the interval
        .withColumn(  # make_interval( [years [, months [, weeks [, days [, hours [, mins [, secs] ] ] ] ] ] ] )
            "time",
            when(
                col("Resolution") == Resolution.quarter,
                expr("StartDateTime + make_interval(0, 0, 0, 0, 0, Factor, 0)"),
            )
            .when(
                col("Resolution") == Resolution.hour,
                expr("StartDateTime + make_interval(0, 0, 0, 0, Factor, 0, 0)"),
            )
            .when(
                col("Resolution") == Resolution.day,
                expr("StartDateTime + make_interval(0, 0, 0, Factor, 0, 0, 0)"),
            )
            .when(
                col("Resolution") == Resolution.month,
                expr("StartDateTime + make_interval(0, Factor, 0, 0, 0, 0, 0)"),
            ),
        )  # time is the time of observation and what we wil partition on, with year, month, day
        .withColumn(
            "year",
            when(col("time").isNotNull(), year(col("time"))).otherwise(lit(None)),
        )
        .withColumn(
            "month",
            when(col("time").isNotNull(), month(col("time"))).otherwise(lit(None)),
        )
        .withColumn(
            "day",
            when(col("time").isNotNull(), dayofmonth(col("time"))).otherwise(lit(None)),
        )
        .drop("StartDateTime", "Positions", "Factor")
    )

    return df
