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


async def job_task(job):
    try:
        job.awaitTermination()
    except asyncio.CancelledError:
        job.stop()
        raise


@pytest.mark.asyncio
async def test_time_series_persister(time_series_persister):
    task = asyncio.create_task(time_series_persister)
    await task
    #await asyncio.sleep(30)
    #task.cancel()
    print("Done")
