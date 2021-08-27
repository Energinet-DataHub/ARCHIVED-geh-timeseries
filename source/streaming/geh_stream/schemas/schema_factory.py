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
import copy
from pyspark.sql import DataFrame, SparkSession
from pyspark.sql.functions import col, from_json
from pyspark.sql.types import IntegerType, LongType, StringType, StructType, StructField, TimestampType, DecimalType, ArrayType
from .schema_names import SchemaNames


quantity_type = DecimalType(18, 3)


class SchemaFactory:
    # validFrom and validTo are not to be included in outputs from the time series point streaming process
    master_schema: StructType = StructType() \
        .add("meteringPointId", StringType(), False) \
        .add("validFrom", TimestampType(), False) \
        .add("validTo", TimestampType(), True) \
        .add("meteringPointType", StringType(), False) \
        .add("settlementMethod", IntegerType(), False)

    parsed_protobuf_schema: StructType = StructType() \
        .add("correlation_id", StringType(), True) \
        .add("document", StructType(StructType()
            .add("business_reason_code", StructType(StructType()
                .add("name", StringType(), True)
                .add("number", IntegerType(), True)
            ), True)
            .add("created_date_time", StructType(StructType()
                .add("nanos", IntegerType(), True)
                .add("seconds", LongType(), True)
            ), True)
            .add("id", StringType(), True)
            .add("request_date_time", StructType(StructType()
                .add("nanos", IntegerType(), True)
                .add("seconds", LongType(), True)
            ), True)
            .add("sender", StructType(StructType()
                .add("business_process_role", StructType(StructType()
                    .add("name", StringType(), True)
                    .add("number", IntegerType(), True)
                ), True)
                .add("id", StringType(), True)
            ), True)), True) \
        .add("series", StructType(StructType()
            .add("end_date_time", StructType(StructType()
                .add("nanos", IntegerType(), True)
                .add("seconds", LongType(), True)
            ), True)
            .add("id", StringType(), True)
            .add("metering_point_id", StringType(), True)
            .add("metering_point_type", StructType(StructType()
                .add("name", StringType(), True)
                .add("number", IntegerType(), True)
            ), True)
            .add("points", ArrayType(StructType()
                .add("observation_date_time", StructType(StructType()
                    .add("nanos", IntegerType(), True)
                    .add("seconds", LongType(), True)
                ), True)
                .add("position", IntegerType(), True)
                .add("quality", StructType(StructType()
                    .add("name", StringType(), True)
                    .add("number", IntegerType(), True)
                ), True)
                .add("quantity", StructType(StructType()
                    .add("nanos", IntegerType(), True)
                    .add("units", LongType(), True)
                ), True), True))
            .add("product", StructType(StructType()
                .add("name", StringType(), True)
                .add("number", IntegerType(), True)
            ), True)
            .add("registration_date_time", StructType(StructType()
                .add("nanos", IntegerType(), True)
                .add("seconds", LongType(), True)
            ), True)
            .add("resolution", StructType(StructType()
                .add("name", StringType(), True)
                .add("number", IntegerType(), True)
            ), True)
            .add("settlement_method", StructType(StructType()
                .add("name", StringType(), True)
                .add("number", IntegerType(), True)
            ), True)
            .add("start_date_time", StructType(StructType()
                .add("nanos", IntegerType(), True)
                .add("seconds", LongType(), True)
            ), True)
            .add("unit", StructType(StructType()
                .add("name", StringType(), True)
                .add("number", IntegerType(), True)
            ), True)), True)

    time_series_points_schema: StructType = StructType() \
        .add("document_id", StringType(), True) \
        .add("document_requestDateTime", TimestampType(), True) \
        .add("document_createdDateTime", TimestampType(), True) \
        .add("document_sender_id", StringType(), True) \
        .add("document_sender_businessProcessRole", IntegerType(), True) \
        .add("document_businessReasonCode", IntegerType(), True) \
        .add("series_id", StringType(), True) \
        .add("series_meteringPointId", StringType(), True) \
        .add("series_meteringPointType", IntegerType(), True) \
        .add("series_settlementMethod", IntegerType(), True) \
        .add("series_registrationDateTime", TimestampType(), True) \
        .add("series_product", IntegerType(), True) \
        .add("series_unit", IntegerType(), True) \
        .add("series_resolution", IntegerType(), True) \
        .add("series_startDateTime", TimestampType(), True) \
        .add("series_endDateTime", TimestampType(), True) \
        .add("series_point_position", IntegerType(), True) \
        .add("series_point_observationDateTime", TimestampType(), True) \
        .add("series_point_quantity", quantity_type, True) \
        .add("series_point_quality", IntegerType(), True) \
        .add("correlationId", StringType(), True)

    # For right now, this is the simplest solution for getting master/parsed data
    # This should be improved
    @staticmethod
    def get_instance(schema_name: SchemaNames):
        if schema_name is SchemaNames.Master:
            return SchemaFactory.master_schema
        elif schema_name is SchemaNames.ParsedProtobuf:
            return SchemaFactory.parsed_protobuf_schema
        elif schema_name is SchemaNames.TimeSeriesPoints:
            return SchemaFactory.time_series_points_schema
        else:
            return None
