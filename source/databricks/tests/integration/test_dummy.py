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

import asyncio
import pytest
import threading
import time
from pyspark.sql import SparkSession, DataFrame
from pyspark.sql.functions import col, lit, to_timestamp, explode
from pyspark.sql.types import (
    StructType,
    StructField,
    StringType,
    ArrayType,
    DecimalType,
    IntegerType,
    TimestampType,
    BooleanType,
    BinaryType,
    LongType,
)


def spark_job(spark):
    raw_stream = (
        spark.readStream
        # .schema(schema)
        .json(
            "/workspaces/geh-timeseries/source/databricks/tests/integration/test_data*.json"
        )
    )

    query = raw_stream.writeStream.outputMode("append").format("console").start()

    # query.awaitTermination()
    return query


@pytest.fixture
def my_job_function(spark: SparkSession, azurite):
    return lambda: spark_job(spark)


# Code from https://stackoverflow.com/questions/45717433/stop-structured-streaming-query-gracefully
# Helper method to stop a streaming query
def stop_stream_query(query, wait_time):
    """Stop a running streaming query"""
    while query.isActive:
        msg = query.status["message"]
        data_avail = query.status["isDataAvailable"]
        trigger_active = query.status["isTriggerActive"]
        if not data_avail and not trigger_active and msg != "Initializing sources":
            print("Stopping query...")
            query.stop()
        time.sleep(0.5)

    # Okay wait for the stop to happen
    print("Awaiting termination...")
    query.awaitTermination(wait_time)


async def job_task(job):
    try:
        job.awaitTermination()
    except asyncio.CancelledError:
        stop_stream_query(job, 5000)
        # raise


schema = StructType(
    [
        StructField("id", IntegerType(), True),
        StructField("first_name", StringType(), True),
        StructField("last_name", StringType(), True),
    ]
)


path = "/workspaces/geh-timeseries/source/databricks/tests/integration/__delta__/unprocessed_time_series"


def time_series_persister(spark: SparkSession):
    input_stream = spark.readStream.schema(schema).json(
        "/workspaces/geh-timeseries/source/databricks/tests/integration/test_data*.json"
    )

    job = (
        input_stream.writeStream.trigger(processingTime="1 seconds")
        .foreachBatch(
            lambda df, id: (
                df.printSchema(),
                df.show(),
                (df.write.format("delta").mode("append").save(path)),
            )
        )
        .outputMode("append")
        # .format("console")
        .start()
        # .writeStream
        # .write
        # .format("delta")
        # .outputMode("complete")
        # .option("checkpointLocation", f"{path}/_checkpoints/streaming-agg")
        # .start(path)
        # .start("/tmp/delta/eventsByCustomer"))
    )

    return job


def read_job(spark: SparkSession):
    data = None
    try:
        print("### Starting reading from delta lake...")
        data = spark.read.format("delta").load(path)
        data.show()
    except:
        print("############################# OH NO")
    print("### Done reading from delta lake")
    return data


@pytest.mark.asyncio
async def test_time_series_persister(spark_fs):
    job = time_series_persister(spark_fs)
    task = asyncio.create_task(job_task(job))
    # await task
    for x in range(20000):
        print(f"### Loop {x}")
        # await asyncio.sleep(1)
        data = read_job(spark_fs)
        if data != None and data.count() > 0:
            print("Yeah! Data found.")
            # data.show()
            task.cancel()
            return

    task.cancel()
    assert False, "No data was stored in Delta table"
