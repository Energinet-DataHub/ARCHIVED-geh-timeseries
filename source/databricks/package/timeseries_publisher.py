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
from pyspark.sql.functions import col, year, month, dayofmonth, when, lit, min, max
from pyspark.sql.types import BooleanType
from package.transforms import JsonTransformer
from package.codelists import Colname
from package.schemas import time_series_schema
from delta.tables import DeltaTable
# TODO: Fix "from package.table_creator" when PR gate breaks on test errors
from package import create_delta_table_if_empty


# Transform raw timeseries from eventhub into timeseries with defined schema suited for aggregations
def publish_timeseries_batch(df, epoch_id, timeseries_processed_path):
    if len(df.head(1)) > 0:
        spark = SparkSession.builder.getOrCreate()
        jsonStringDataframe = df.select(Colname.timeseries)
        jsonTransformer = JsonTransformer()
        withTime = jsonTransformer.TransformFromJsonToDataframe(jsonStringDataframe)

        # Set new column names on incomming dataframe
        df_to_merge = withTime.select(
            col(Colname.metering_point_id).alias('update_metering_point_id'),
            col(Colname.transaction_id).alias('update_transaction_id'),
            col(Colname.quantity).alias('update_quantity'),
            col(Colname.quality).alias('update_quality'),
            col(Colname.time).alias('update_time'),
            col(Colname.resolution).alias('update_resolution'),
            col(Colname.year).alias('update_year'),
            col(Colname.month).alias('update_month'),
            col(Colname.day).alias('update_day'),
            col(Colname.registration_date_time).alias('update_registration_date_time')
        )
        # Determine min and max year, month and day from incomming dataframe
        min_max_df = df_to_merge \
            .groupBy() \
            .agg(
                min("update_time").alias("min_time"),
                max("update_time").alias("max_time")) \
            .withColumn("min_year", year("min_time")) \
            .withColumn("min_month", month("min_time")) \
            .withColumn("min_day", dayofmonth("min_time")) \
            .withColumn("max_year", year("max_time")) \
            .withColumn("max_month", month("max_time")) \
            .withColumn("max_day", dayofmonth("max_time"))
        row = min_max_df.first()

        create_delta_table_if_empty(timeseries_processed_path, time_series_schema, [Colname.year, Colname.month, Colname.day])

        # Fetch existing processed timeseries within min and max year, month and day
        existing_df = spark \
            .read \
            .format("delta") \
            .load(timeseries_processed_path) \
            .where(f"""(year >= {row['min_year']} AND month >= {row['min_month']} AND day >= {row['min_day']})
                AND (year <= {row['max_year']} AND month <= {row['max_month']} AND day <= {row['max_day']})""")
        # Left join incomming dataframe with existing data on metering point id and time
        determine_df = df_to_merge.join(existing_df, (
            df_to_merge["update_metering_point_id"] == existing_df[Colname.metering_point_id])
            & (df_to_merge["update_time"] == existing_df[Colname.time]),
            "left")
        # Determine if incomming data should be updated based on condition that checks that incomming data registration datetime is greater or equal to existing data
        determine_df = determine_df.withColumn('should_update',
                                               (when(col(Colname.registration_date_time).isNotNull(),
                                                     col(Colname.registration_date_time) <= col('update_registration_date_time'))
                                                .otherwise(lit(False))).cast(BooleanType()))
        # determine_df.display()
        # Determine if incomming data should be inserted based on condition that "should_update" is False and there is no existing metering point in timeseries_processed table for the given time
        to_insert = determine_df \
            .filter(col('should_update') == 'False') \
            .filter(col(Colname.metering_point_id).isNull()) \
            .select(
                col('update_metering_point_id').alias(Colname.metering_point_id),
                col('update_transaction_id').alias(Colname.transaction_id),
                col('update_quantity').alias(Colname.quantity),
                col('update_quality').alias(Colname.quality),
                col('update_time').alias(Colname.time),
                col('update_resolution').alias(Colname.resolution),
                col('update_year').alias(Colname.year),
                col('update_month').alias(Colname.month),
                col('update_day').alias(Colname.day),
                col('update_registration_date_time').alias(Colname.registration_date_time)
            )
        # Filter out data that should be updated based on "should_update" column
        to_update = determine_df.filter(col('should_update') == 'True')
        # Insert data into timeseries_processed table
        to_insert \
            .write \
            .partitionBy(
                Colname.year,
                Colname.month,
                Colname.day) \
            .format("delta") \
            .mode("append") \
            .save(timeseries_processed_path)
        processed_table = DeltaTable.forPath(spark, timeseries_processed_path)
        # Update existing data with incomming data
        processed_table \
            .alias('source') \
            .merge(
                to_update.alias('updates'),
                f'source.{Colname.metering_point_id} = updates.{Colname.metering_point_id} AND source.{Colname.time} = updates.{Colname.time}'
            ) \
            .whenMatchedUpdate(set={
                Colname.transaction_id: "updates.update_transaction_id",
                Colname.quantity: "updates.update_quantity",
                Colname.quality: "updates.update_quality",
                Colname.resolution: "updates.update_resolution",
                Colname.registration_date_time: "updates.update_registration_date_time"
            }
            ) \
            .execute()


def timeseries_publisher(delta_lake_container_name: str, storage_account_name: str, timeseries_unprocessed_path: str, timeseries_processed_path: str):

    source_path = timeseries_unprocessed_path
    spark = SparkSession.builder.getOrCreate()

    read_df = spark.readStream.format("delta").load(source_path)

    checkpoint_path = f"abfss://{delta_lake_container_name}@{storage_account_name}.dfs.core.windows.net/checkpoint-timeseries-publisher"

    read_df. \
        writeStream. \
        option("checkpointLocation", checkpoint_path). \
        foreachBatch(lambda df, epochId: publish_timeseries_batch(df, epochId, timeseries_processed_path)). \
        start()
