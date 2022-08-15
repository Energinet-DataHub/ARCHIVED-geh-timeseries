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
from package.schemas.time_series_unprocessed import (
    TimeSeriesUnprocessedColname as TSUColname,
)
from package.schemas.time_series_points import (
    TimeSeriesPointsColname as TSPColname,
)

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
from pyspark.sql import DataFrame
from package.codelists import Resolution
from package.codelists import Colname


def transform_unprocessed_time_series_to_points(source: DataFrame) -> DataFrame:
    set_time_func = (
        when(
            col(TSUColname.Resolution) == Resolution.quarter,
            expr(
                f"{TSUColname.StartDateTime} + make_interval(0, 0, 0, 0, 0, TimeToAdd, 0)"
            ),
        )
        .when(
            col(TSUColname.Resolution) == Resolution.hour,
            expr(
                f"{TSUColname.StartDateTime} + make_interval(0, 0, 0, 0, TimeToAdd, 0, 0)"
            ),
        )
        .when(
            col(TSUColname.Resolution) == Resolution.day,
            expr(
                f"{TSUColname.StartDateTime} + make_interval(0, 0, 0, TimeToAdd, 0, 0, 0)"
            ),
        )
        .when(
            col(TSUColname.Resolution) == Resolution.month,
            expr(
                f"{TSUColname.StartDateTime} + make_interval(0, TimeToAdd, 0, 0, 0, 0, 0)"
            ),
        )
    )

    df = (
        source.select(
            col("*"), explode(TSUColname.Period_Points).alias(TSUColname.Points)
        )
        .select(
            col(TSUColname.MeteringPointId),
            col(TSUColname.TransactionId),
            col(TSUColname.Points_Quantity).alias(TSUColname.Quantity),
            col(TSUColname.Points_Quality).alias(TSUColname.Quality),
            col(TSUColname.Points_Position).alias(TSUColname.Position),
            col(TSUColname.Period_Resolution).alias(TSUColname.Resolution),
            col(TSUColname.Period_StartDateTime).alias(TSUColname.StartDateTime),
            col(TSUColname.RegistrationDateTime),
            col(TSUColname.CreatedDateTime),
        )
        .withColumn(
            TSUColname.RegistrationDateTime,
            when(
                col(TSUColname.RegistrationDateTime).isNull(),
                col(TSUColname.CreatedDateTime),
            ).otherwise(col(TSUColname.RegistrationDateTime)),
        )
        .withColumn(
            "TimeToAdd",
            when(
                col(TSUColname.Resolution) == Resolution.quarter,
                (col(TSUColname.Position) - 1) * 15,
            ).otherwise(col(TSUColname.Position) - 1),
        )
        .withColumn(TSPColname.StoredTime, current_timestamp())
        .withColumn(TSPColname.Time, set_time_func)
        .withColumn(TSPColname.Year, year(col(Colname.time)))
        .withColumn(TSPColname.Month, month(col(Colname.time)))
        .withColumn(TSPColname.Day, dayofmonth(col(Colname.time)))
        .drop("TimeToAdd")
    )

    return df
