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
from package.transforms import JsonTransformer
from package.codelists import Colname
from package.schemas import time_series_schema
from delta.tables import DeltaTable
from package.table_creator import create_delta_table_if_empty


# Transform raw timeseries from eventhub into timeseries with defined schema suited for aggregations
def publish_timeseries_batch(df, epoch_id, timeseries_processed_path):
    jsonStringDataframe = df.select(Colname.timeseries)
    jsonTransformer = JsonTransformer()

    (jsonTransformer
     .TransformFromJsonToDataframe(jsonStringDataframe)
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
         col(Colname.registration_date_time)
     )
     .write
     .partitionBy(
         Colname.year,
         Colname.month,
         Colname.day)
     .format("delta")
     .mode("append")
     .save(timeseries_processed_path))


def timeseries_publisher(spark: SparkSession, timeseries_unprocessed_path: str, checkpoint_path: str, timeseries_processed_path: str):
    create_delta_table_if_empty(spark, timeseries_processed_path, time_series_schema, [Colname.year, Colname.month, Colname.day])

    return (spark
            .readStream
            .format("delta")
            .load(timeseries_unprocessed_path)
            .writeStream
            .option("checkpointLocation", checkpoint_path)
            .foreachBatch(lambda df, epochId: publish_timeseries_batch(df, epochId, timeseries_processed_path))
            .start())
