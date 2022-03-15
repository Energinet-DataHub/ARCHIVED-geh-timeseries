import pytest
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
        
def add_test_column(df: DataFrame) -> DataFrame:
    return df.withColumn("this is the fist column", lit("this is the first values"))

def test_add_test_column(spark):
    # Arrange
    columns = ["language","users_count"]
    data = [("Java", "20000"), ("Python", "100000"), ("Scala", "3000")]
    
    rdd = spark.sparkContext.parallelize(data)    
    dfFromRDD1 = rdd.toDF(columns)
    
    # Act

    print(dfFromRDD1.show())
