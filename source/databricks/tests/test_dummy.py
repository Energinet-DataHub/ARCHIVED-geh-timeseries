import asyncio
import pytest
import os
import threading
import time
from pyspark import SparkConf
from pyspark.sql import SparkSession, DataFrame
from pyspark.sql.functions import col, lit, to_timestamp, explode
from pyspark.sql.types import StructType, StructField, StringType, ArrayType, \
    DecimalType, IntegerType, TimestampType, BooleanType, BinaryType, LongType

# Hej Lasse

@pytest.fixture()
def spark():
    # spark.hadoop.fs.* for Azurite storage
    spark_conf = SparkConf(loadDefaults=True) \
        .set("spark.sql.session.timeZone", "UTC")
        #.set("spark.hadoop.fs.defaultFS", "wasb://container@azurite") \
        #.set("spark.hadoop.fs.azure.storage.emulator.account.name", "azurite")
    return SparkSession \
        .builder \
        .config(conf=spark_conf) \
        .config("spark.sql.streaming.schemaInference", True) \
        .getOrCreate()


class Task(threading.Thread):
    def __init__(self, func):
        threading.Thread.__init__(self)
        self.func = func

    def run(self):
        """Target function of the thread class"""
        self.function()


@pytest.fixture(scope="session")
def azurite():
    import subprocess
    azurite_process = subprocess.Popen(args=["azurite-blob", "-l", ".azurite-files"], stdout=subprocess.PIPE)

    # import os
    # os.system("azurite-blob -l .azurite-files")
    yield
    azurite_process.terminate()
    print("############ Azurite has terminated")


"""
schema = StructType(
            [(StructField("id", IntegerType, true),
                 StructField("first_name", StringType, true),
                 StructField("last_name", StringType, true))])
"""

def spark_job(spark):
    raw_stream =(spark
            .readStream
            #.schema(schema)
            .json("/workspaces/geh-timeseries/source/databricks/tests/test_data*.json"))

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
    raw_stream =(spark
        .readStream
        #.schema(schema)
        .json("/workspaces/geh-timeseries/source/databricks/tests/test_data*.json"))

    query = (raw_stream
        .writeStream
        .outputMode("append")
        .format("console")
        .start())
    try:
        print("############# About to await termination")
        quey.awaitTermination()
    except asyncio.CancelledError:
        print("############# About to stop spark job")
        query.stop()
        raise


@pytest.mark.asyncio
async def test_cancel(spark, azurite):
    task = asyncio.create_task(job_task(spark))
    #await task
    await asyncio.sleep(10)
    task.cancel()
    print("Done")
