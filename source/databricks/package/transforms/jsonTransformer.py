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

from package.schemas.eventhub_timeseries_schema import eventhub_timeseries_schema
from pyspark.sql.functions import from_json, explode, when, col, to_timestamp, expr
from pyspark.sql.dataframe import DataFrame
from package.codelists import Resolution
from package.codelists import Colname


class JsonTransformer():

    def TransformFromJsonToDataframe(self, source: DataFrame) -> DataFrame:
        structured = source.select(from_json(source.body, eventhub_timeseries_schema).alias('json'))
        flat = structured. \
            select(explode("json.Series")). \
            select("col.MeteringPointId", "col.TransactionId", "col.Period"). \
            select(
                col("MeteringPointId").alias(Colname.metering_point_id),
                col("TransactionId").alias(Colname.transaction_id),
                to_timestamp(col("Period.StartDateTime")).alias("StartDateTime"),
                col("Period.Resolution").alias("Resolution"),
                explode("Period.Points").alias("Period_Point")). \
            select("*",
                   col("Period_Point.Quantity").cast("decimal(18,3)").alias(Colname.quantity),
                   col("Period_Point.Quality").alias(Colname.quality),
                   "Period_Point.Position"). \
            drop("Period_Point")

        withResolutionInMinutes = flat.withColumn("ClockResolution",
                                                  when(col("Resolution") == Resolution.quarter, 15).
                                                  when(col("Resolution") == Resolution.hour, 60).
                                                  when(col("Resolution") == Resolution.day, 1440).
                                                  when(col("Resolution") == Resolution.month, 43800)).drop("Resolution")

        # TODO how do we handle month resolution ?
        timeToAdd = withResolutionInMinutes.withColumn("TimeToAdd", (col("Position") - 1) * col("ClockResolution")).drop("ClockResolution", "Position")
        withTime = timeToAdd.withColumn(Colname.time, expr("StartDateTime + make_interval(0, 0, 0, 0, 0, TimeToAdd, 0)")).drop("StartDateTime").drop("TimeToAdd")

        return withTime
