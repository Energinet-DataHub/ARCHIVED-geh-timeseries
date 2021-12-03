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
from pyspark.sql.functions import year, month, dayofmonth, to_json, \
    struct, col, from_json, coalesce, lit
# from geh_stream.event_dispatch.meteringpoint_dispatcher import dispatcher
# from geh_stream.shared.data_exporter import export_to_csv
# from geh_stream.bus import message_registry
from .event_meta_data import EventMetaData


def __incomming_event_handler(df: DataFrame, epoch_id, spark, master_data_path):
    df.printSchema()
    df.show()

    json_schema = spark.read.json(df.rdd.map(lambda row: row.body)).schema
    # df.withColumn('json', from_json(col('json'), json_schema))

    df = df \
        .withColumn("deserialized", from_json("body", json_schema))

    df.printSchema()
    df.show()

    #     .withColumn("metering_point_id")
    # if len(df.head(1)) > 0:
    #     for row in df.rdd.collect():
    #         event_class = message_registry.get(row[EventMetaData.event_name])

    #         if event_class is not None:
    #             # deserialize from json with dataclasses_json
    #             try:
    #                 event = event_class.from_json(row["body"])
    #                 dispatcher(event)
    #             except Exception as e:
    #                 print("An exception occurred when trying to dispatch" + str(e))

    df = df \
        .select(col("deserialized.metering_point_id").alias("meteringPointId"),
                col("deserialized.metering_point_type").alias("meteringPointType"),
                col("deserialized.settlement_method").alias("settlementMethod"),
                col("deserialized.connection_state").alias("connectionState"),
                col("deserialized.effective_date").alias("validFrom")) \
        .withColumn("validTo", lit(None).cast("timestamp"))

    df.printSchema()
    df.show()

    df = df \
        .write \
        .format("delta") \
        .mode("append") \
        .save(master_data_path)


def build_masterdata(delta_lake_container_name: str, storage_account_name: str, events_delta_path, master_data_path: str):
    spark = SparkSession.builder.getOrCreate()
    inputDf = spark.readStream.format("delta").load(events_delta_path)
    checkpoint_path = f"abfss://{delta_lake_container_name}@{storage_account_name}.dfs.core.windows.net/masterdata_checkpoint"

    (inputDf
        .writeStream
        .option("checkpointLocation", checkpoint_path)
        .foreachBatch(lambda df, epochId: __incomming_event_handler(df, epochId, spark, master_data_path))
        .start()
        .awaitTermination())
    print("Master data builder started.")
