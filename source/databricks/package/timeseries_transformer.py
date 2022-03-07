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
from pyspark.sql import SparkSession
from pyspark.sql.types import StringType, StructType, StructField, ArrayType
from package.transforms import JsonTransformer
from package.codelists import Colname


def transform(df, epoch_id, timeseries_processed_path):
    jsonStringDataframe = df.select(df.body.cast(StringType()).alias("body"))
    withTime = JsonTransformer.TransformFromJsonToDataframe(jsonStringDataframe)

    withTime.write \
            .partitionBy(
                Colname.year, 
                Colname.month, 
                Colname.day) \
            .format("delta") \
            .mode("append") \
            .save(timeseries_processed_path)


def timeseries_transformer(delta_lake_container_name: str, storage_account_name: str, timeseries_unprocessed_path: str, timeseries_processed_path: str):

    source_path = timeseries_unprocessed_path
    spark = SparkSession.builder.getOrCreate()

    read_df = spark.readStream.format("delta").load(source_path)

    checkpoint_path = f"abfss://{delta_lake_container_name}@{storage_account_name}.dfs.core.windows.net/checkpoint-timeseries-transformer"

    read_df. \
        writeStream. \
        option("checkpointLocation", checkpoint_path). \
        foreachBatch(lambda df, epochId: transform(df, epochId, timeseries_processed_path)). \
        start()
