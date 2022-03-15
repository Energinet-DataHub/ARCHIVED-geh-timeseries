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
        .getOrCreate()
         
def test_my_job(spark):
    print(os.getcwd())
    
    raw_stream = spark \
    .readStream \
    .json("/workspaces/geh-timeseries/source/databricks/tests/test_data.json") \
    .load()
        
    query = raw_stream \
    .writeStream \
    .outputMode("complete") \
    .format("console") \
    .start()

    query.awaitTermination()        