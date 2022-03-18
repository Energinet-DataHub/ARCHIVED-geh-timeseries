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

import pytest
import subprocess

from pyspark import SparkConf
from pyspark.sql import SparkSession
from pyspark.sql.types import StructType, StructField, StringType, ArrayType, \
    DecimalType, IntegerType, TimestampType, BooleanType, BinaryType, LongType


@pytest.fixture(scope="session")
def azurite():
    """Fixture for starting Azurite blob service"""
    azurite_process = subprocess.Popen(
        args=["azurite-blob", "-l", ".azurite-files"], stdout=subprocess.PIPE
    )

    # Terminate Azurite service at end of test session
    yield
    azurite_process.terminate()


@pytest.fixture()
def spark(azurite):
    # spark.hadoop.fs.* for Azurite storage
    spark_conf = (SparkConf(loadDefaults=True)
        .set("spark.sql.session.timeZone", "UTC")
        .set("spark.hadoop.fs.defaultFS", "wasb://container@azurite")
        .set("spark.hadoop.fs.azure.storage.emulator.account.name", "azurite"))
    return (SparkSession
        .builder
        .config(conf=spark_conf)
        #.config("spark.hadoop.fs.defaultFS", "wasb://container@azurite")
        #.config("spark.hadoop.fs.azure.storage.emulator.account.name", "azurite")
        #.config("spark.sql.streaming.schemaInference", True)
        .getOrCreate())


schema = StructType(
    [StructField("id", IntegerType(), True),
        StructField("first_name", StringType(), True),
        StructField("last_name", StringType(), True)])


@pytest.fixture
def time_series_persister(spark):
    input_stream = (spark
                  .readStream
                  .schema(schema)
                  .json("/workspaces/geh-timeseries/source/databricks/tests/integration/test_data*.json"))

    job = (input_stream
             #.writeStream
             #.trigger(processingTime='5 seconds')
             .foreachBatch(lambda df, id: (
                 df.printSchema(),
                 df.show(),

                 (df
                     .write
                     .format("delta")
                     .mode("append")
                     .save("unprocessed_time_series"))
             ))
             .outputMode("append")
             #.format("console")
             #.start())
             #.writeStream
             #.format("delta")
             #.outputMode("complete")
             .option("checkpointLocation", "/tmp/delta/eventsByCustomer/_checkpoints/streaming-agg")
             .start("/tmp/delta/eventsByCustomer"))

    return job
