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
)
from pyspark.sql import DataFrame
from pyspark.sql.types import IntegerType


def transform_unprocessed_time_series_to_points(source: DataFrame) -> DataFrame:
    df = (
        source.select(col("*"), explode("Period.Points").alias("Points"))
        .select(
            col("MeteringPointId"),
            col("TransactionId"),
            col("Points.Quantity").alias("Quantity"),
            col("Points.Quality").alias("Quality"),
            col("Period.Resolution").alias("Resolution"),
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
            "year",
            when(col("storedTime").isNotNull(), year(col("storedTime"))).otherwise(
                lit(None)
            ),
        )
        .withColumn(
            "month",
            when(col("storedTime").isNotNull(), month(col("storedTime"))).otherwise(
                lit(None)
            ),
        )
        .withColumn(
            "day",
            when(
                col("storedTime").isNotNull(), dayofmonth(col("storedTime"))
            ).otherwise(lit(None)),
        )
    )

    return df
