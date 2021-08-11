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
from pyspark.sql import SparkSession, DataFrame
from pyspark.sql.functions import col, explode, udf
from pyspark.sql.types import IntegerType, StringType, StructType, StructField, TimestampType, DecimalType, ArrayType

from geh_stream.schemas import SchemaFactory, quantity_type, SchemaNames
from .protobuf_message_parser import ProtobufMessageParser
from geh_stream.dataframelib import flatten_df


def get_time_series_point_stream(spark: SparkSession, input_eh_conf: dict) -> DataFrame:
    # Get raw data as they are represented by the eventhubs provider
    raw_stream = spark \
        .readStream \
        .format("eventhubs") \
        .options(**input_eh_conf) \
        .load()

    print("Input stream schema:")
    raw_stream.printSchema()

    # Parse data from protobuf messages
    message_schema: StructType = SchemaFactory.get_instance(SchemaNames.MessageBody)
    parsed_stream = ProtobufMessageParser.parse(raw_stream, message_schema)  # TODO: Schema is unused

    # Flatten structure because - in general - it's much easier to work with non-nested structures in Spark
    flattened_stream = flatten_df(parsed_stream)

    # Explode time series into points - again because it's much easier to work with an also has the benefits
    # that we can adjust names and types of the individual time series point properties
    temp_time_series_point_stream = flattened_stream.select(col("*"), explode(col("series_points")).alias("series_point")).drop("series_points")

    # Adjust points to match schema SchemaFactory.message_body_schema
    # TODO: Unit test that we end up with the expected schema
    time_series_point_stream = (temp_time_series_point_stream.select(
                                col("document_id").alias("document_id").cast(StringType()),
                                # TODO: Is there a prettier way to handle timestamp conversion - one that also expands to other (custom) types?
                                col("document_request_date_time_seconds").alias("document_requestDateTime").cast(TimestampType()),  # Ignoring the nano part
                                col("document_created_date_time_seconds").alias("document_createdDateTime").cast(TimestampType()),  # Ignoring the nano part
                                col("document_sender_id").alias("document_sender_id").cast(StringType()),
                                col("document_sender_business_proces_role").alias("document_sender_businessProcessRole").cast(IntegerType()),
                                col("document_recipient_id").alias("document_recipient_id").cast(StringType()),
                                col("document_recipient_business_proces_role").alias("document_recipient_businessProcessRole").cast(IntegerType()),
                                col("document_business_reason_code").alias("document_businessReasonCode").cast(IntegerType()),
                                col("series_id").alias("series_id").cast(StringType()),
                                col("series_metering_point_id").alias("series_meteringPointId").cast(StringType()),
                                col("series_metering_point_type").alias("series_meteringPointType").cast(IntegerType()),
                                col("series_settlement_method").alias("series_settlementMethod").cast(IntegerType()),
                                col("series_registration_date_time_seconds").alias("series_registrationDateTime").cast(TimestampType()),  # Ignoring the nano part
                                col("series_product").alias("series_product").cast(IntegerType()),
                                col("series_measure_unit").alias("series_unit").cast(IntegerType()),
                                col("series_resolution").alias("series_resolution").cast(IntegerType()),
                                col("series_start_date_time_seconds").alias("series_startDateTime").cast(TimestampType()),  # Ignoring the nano part
                                col("series_end_date_time_seconds").alias("series_endDateTime").cast(TimestampType()),  # Ignoring the nano part
                                col("series_point.position").alias("series_point_position").cast(IntegerType()),
                                col("series_point.observation_date_time.seconds").alias("series_point_observationDateTime").cast(TimestampType()),  # Ignoring the nano part
                                to_quantity(col("series_point.quantity.units"), col("series_point.quantity.nanos")).alias("series_point_quantity"),
                                col("series_point.quality").alias("series_point_quality").cast(IntegerType()),
                                col("correlation_id").alias("correlationId").cast(StringType())))

    print("Time Series Point stream schema:")
    time_series_point_stream.printSchema()

    return time_series_point_stream


# TODO: Move to lib
from decimal import Decimal, getcontext


def __to_quantity(units, nanos):
    return units + Decimal(nanos) / 1E-9


to_quantity = udf(__to_quantity, quantity_type)
