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
from pyspark.sql.types import StructType, StructField, StringType, ArrayType, \
    DecimalType, IntegerType, TimestampType, BooleanType, BinaryType, LongType


class Task(threading.Thread):
    def __init__(self, func):
        threading.Thread.__init__(self)
        self.func = func

    def run(self):
        """Target function of the thread class"""
        self.function()


"""
schema = StructType(
            [(StructField("id", IntegerType, true),
                 StructField("first_name", StringType, true),
                 StructField("last_name", StringType, true))])
"""


def spark_job(spark):
    raw_stream = (spark
                  .readStream
                  # .schema(schema)
                  .json("/workspaces/geh-timeseries/source/databricks/tests/integration/test_data*.json"))

    query = (raw_stream
             .writeStream
             .outputMode("append")
             .format("console")
             .start())

    # query.awaitTermination()
    return query


@pytest.fixture
def my_job_function(spark: SparkSession, azurite):
    return lambda: spark_job(spark)


"""
def test_spark(my_job_function):
    my_job_function().awaitTermination()
    print("############ Never get here")
"""

"""
def test_my_job(my_job_function):
    task = Task(my_job_function)
    task.start()
    time.sleep(5)
    task.join()
    my_job.awaitTermination()
"""


async def job_task(spark):
    raw_stream = (spark
                  .readStream
                  # .schema(schema)
                  .json("/workspaces/geh-timeseries/source/databricks/tests/integration/test_data*.json"))

    query = (raw_stream
             .writeStream
             .outputMode("append")
             .format("console")
             .start())
    try:
        print("############# About to await termination")
        query.awaitTermination()
    except asyncio.CancelledError:
        print("############# About to stop spark job")
        query.stop()
        raise


@pytest.mark.asyncio
async def test_cancel(spark, azurite):
    task = asyncio.create_task(job_task(spark))
    # await task
    await asyncio.sleep(30)
    task.cancel()
    print("Done")
