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
from pyspark.sql.types import IntegerType, DecimalType
from package.codelists import Resolution


def transform_unprocessed_time_series_to_points(source: DataFrame) -> DataFrame:
    df = (
        source.select(col("*"), explode("Period.Points").alias("Points"))
        .withColumn(
            "RegistrationDateTime",
            when(
                col("RegistrationDateTime").isNull(),
                col("CreatedDateTime"),
            ).otherwise(col("RegistrationDateTime")),
        )
        .withColumn(
            "Points",
            col("Points").withField(
                "Quantity", col("Points.Quantity").cast(DecimalType(18, 3))
            ),
        )
        .withColumn("storedTime", current_timestamp())
        .withColumn(
            "Factor",
            when(
                col("Period.Resolution") == Resolution.quarter.value,
                (col("Points.Position") - 1)
                * 15,  # To add 15, 30 or 45 to the minut interval
            ).otherwise(
                col("Points.Position") - 1
            ),  # Position - 1 to make the value set start from zero
        )  # Factor is used in the next step to add to the interval
        .withColumn(  # make_interval( [years [, months [, weeks [, days [, hours [, mins [, secs] ] ] ] ] ] ] )
            "time",
            when(
                col("Period.Resolution") == Resolution.quarter.value,
                expr("Period.StartDateTime + make_interval(0, 0, 0, 0, 0, Factor, 0)"),
            )
            .when(
                col("Period.Resolution") == Resolution.hour.value,
                expr("Period.StartDateTime + make_interval(0, 0, 0, 0, Factor, 0, 0)"),
            )
            .when(
                col("Period.Resolution") == Resolution.day.value,
                expr("Period.StartDateTime + make_interval(0, 0, 0, Factor, 0, 0, 0)"),
            )
            .when(
                col("Period.Resolution") == Resolution.month.value,
                expr("Period.StartDateTime + make_interval(0, Factor, 0, 0, 0, 0, 0)"),
            ),
        )  # time is the time of observation and what we wil partition on, with year, month, day
        .withColumn(
            "year",
            year(col("time")),
        )
        .withColumn(
            "month",
            month(col("time")),
        )
        .withColumn(
            "day",
            dayofmonth(col("time")),
        )
        .select(
            col("GsrnNumber"),
            col("TransactionId"),
            col("Points.Quantity"),
            col("Points.Quality"),
            col("Period.Resolution"),
            col("RegistrationDateTime"),
            col("storedTime"),
            col("time"),
            col("year"),
            col("month"),
            col("day"),
        )
    )

    return df
