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
from decimal import Decimal, getcontext
from pyspark.sql import SparkSession, DataFrame
from pyspark.sql.functions import col, explode, udf
from pyspark.sql.types import IntegerType, StringType, StructType, StructField, TimestampType, DecimalType, ArrayType

from geh_stream.schemas import quantity_type
from .protobuf_message_parser import ProtobufMessageParser
from geh_stream.dataframelib import flatten_df


def get_time_series_point_stream(spark: SparkSession, input_eh_conf: dict) -> DataFrame:
    # Get raw data as they are represented by the eventhubs provider
    raw_stream = spark \
        .readStream \
        .format("eventhubs") \
        .options(**input_eh_conf) \
        .load()

    return __parse_stream(raw_stream)


def __parse_stream(raw_stream) -> DataFrame:
    print("Input stream schema:")
    raw_stream.printSchema()

    # Parse data from protobuf messages
    parsed_stream = ProtobufMessageParser.parse(raw_stream)

    temp_time_series_point_stream = __get_flattened_time_series_points(parsed_stream)

    time_series_point_stream = (temp_time_series_point_stream.select(
                                col("document_id").alias("document_id").cast(StringType()),
                                col("document_request_date_time_seconds").alias("document_requestDateTime").cast(TimestampType()),
                                col("document_created_date_time_seconds").alias("document_createdDateTime").cast(TimestampType()),
                                col("document_sender_id").alias("document_sender_id").cast(StringType()),
                                col("document_sender_business_process_role_number").alias("document_sender_businessProcessRole").cast(IntegerType()),
                                col("document_business_reason_code_number").alias("document_businessReasonCode").cast(IntegerType()),
                                col("series_id").alias("series_id").cast(StringType()),
                                col("series_metering_point_id").alias("series_meteringPointId").cast(StringType()),
                                col("series_metering_point_type_number").alias("series_meteringPointType").cast(IntegerType()),
                                col("series_settlement_method_number").alias("series_settlementMethod").cast(IntegerType()),
                                col("series_registration_date_time_seconds").alias("series_registrationDateTime").cast(TimestampType()),
                                col("series_product_number").alias("series_product").cast(IntegerType()),
                                col("series_unit_number").alias("series_unit").cast(IntegerType()),
                                col("series_resolution_number").alias("series_resolution").cast(IntegerType()),
                                col("series_start_date_time_seconds").alias("series_startDateTime").cast(TimestampType()),
                                col("series_end_date_time_seconds").alias("series_endDateTime").cast(TimestampType()),
                                col("series_point.position").alias("series_point_position").cast(IntegerType()),
                                col("series_point.observation_date_time.seconds").alias("series_point_observationDateTime").cast(TimestampType()),
                                to_quantity(
                                    col("series_point.quantity.units"),
                                    col("series_point.quantity.nanos")).alias("series_point_quantity"),
                                col("series_point.quality.number").alias("series_point_quality").cast(IntegerType()),
                                col("correlation_id").alias("correlationId").cast(StringType())))

    print("Time Series Point stream schema:")
    time_series_point_stream.printSchema()

    return time_series_point_stream


def __get_flattened_time_series_points(parsed_stream) -> DataFrame:
    "Get time series points in flattened structure"

    # Flatten structure because - in general - it's much easier to work with non-nested structures in Spark
    flattened_stream = flatten_df(parsed_stream)

    # Explode time series into points - again because it's much easier to work with and also has the benefits
    # that we can adjust names and types of the individual time series point properties
    return flattened_stream.select(col("*"), explode(col("series_points")).alias("series_point")).drop("series_points")


def __to_quantity(units, nanos):
    return Decimal(units or 0) + (Decimal(nanos or 0) / 10**9)


to_quantity = udf(__to_quantity, quantity_type)
