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



@pytest.fixture(scope="session")
def spark_azurite(azurite):
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


time_series_json = """{"id":1,"first_name":"Saleem","last_name":"Arkcoll"}
{"id":2,"first_name":"Brandy","last_name":"Plascott"}
{"id":1000,"first_name":"Alic","last_name":"Furniss"}"""


def time_series_persister(spark: SparkSession):
    input_stream = (spark
                  .readStream
                  .schema(schema)
                  .json("/workspaces/geh-timeseries/source/databricks/tests/integration/test_data*.json"))

    #input_stream = spark.read.json(spark.sparkContext.parallelize(time_series_json))

    job = (input_stream
             #.writeStream
             #.trigger(processingTime='5 seconds')
            #  .foreachBatch(lambda df, id: (
            #      df.printSchema(),
            #      df.show(),

            #      (df
            #          .write
            #          .format("delta")
            #          .mode("append")
            #          .save("unprocessed_time_series"))
            #  ))
            #  .outputMode("append")
             #.format("console")
             #.start())
             #.writeStream
             .write
             #.format("delta")
             #.outputMode("complete")
             #.option("checkpointLocation", "/tmp/delta/eventsByCustomer/_checkpoints/streaming-agg")
             .save("./__delta/unprocessed_time_series")
             #.start("/tmp/delta/eventsByCustomer"))
    )

    return job
