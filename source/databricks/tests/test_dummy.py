import pytest
import os
from pyspark import SparkConf
from pyspark.sql import SparkSession, DataFrame
from pyspark.sql.functions import col, lit, to_timestamp, explode
from pyspark.sql.types import StructType, StructField, StringType, ArrayType, \
    DecimalType, IntegerType, TimestampType, BooleanType, BinaryType, LongType


@pytest.fixture()
def spark():
    spark_conf = SparkConf(loadDefaults=True) \
        .set("spark.sql.session.timeZone", "UTC")
    return SparkSession \
        .builder \
        .config(conf=spark_conf) \
        .config("spark.sql.streaming.schemaInference", True) \
        .getOrCreate()


"""
schema = StructType(
            [(StructField("id", IntegerType, true),
                 StructField("first_name", StringType, true),
                 StructField("last_name", StringType, true))])
"""
@pytest.fixture
def my_job(spark: SparkSession):
    print(os.getcwd())

    raw_stream =(spark
            .readStream
            #.schema(schema)
            .json("/workspaces/geh-timeseries/source/databricks/tests/test_data*.json"))

    query = (raw_stream
        .writeStream
        .outputMode("append")
        .format("console")
        .start())

    #query.awaitTermination()
    return query


def test_my_job(my_job):
    my_job.awaitTermination()
