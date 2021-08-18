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
from pyspark.sql.types import IntegerType, StringType, StructType, StructField, TimestampType, DecimalType, ArrayType
from .schema_names import SchemaNames


# See NOTE on usage
def make_all_nullable(schema):
    schema.nullable = True
    if isinstance(schema, StructField):
        make_all_nullable(schema.dataType)
    if isinstance(schema, ArrayType):
        make_all_nullable(schema.elementType)
    if isinstance(schema, StructType):
        for f in schema.fields:
            make_all_nullable(f)


quantity_type = DecimalType(18, 3)


class SchemaFactory:
    # validFrom and validTo are not to be included in outputs from the time series point streaming process
    master_schema: StructType = StructType() \
        .add("meteringPointId", StringType(), False) \
        .add("validFrom", TimestampType(), False) \
        .add("validTo", TimestampType(), True) \
        .add("meteringPointType", StringType(), False) \
        .add("settlementMethod", IntegerType(), False)

    # TODO: This doesn't seem to be in use as errors in the schema doesn't seem to break anything
    parquet_schema: StructType = StructType() \
        .add("document_id", StringType(), False) \
        .add("document_createdDateTime", TimestampType(), False) \
        .add("document_sender_id", StringType(), False) \
        .add("document_businessReasonCode", StringType(), False) \
        .add("document_sender_businessProcessRole", StringType(), False) \
        .add("series_id", StringType(), False) \
        .add("series_meteringPointId", StringType(), False) \
        .add("series_meteringPointType", StringType(), False) \
        .add("series_product", StringType(), False) \
        .add("series_unit", StringType(), False) \
        .add("series_settlementMethod", StringType(), True) \
        .add("series_point_position", IntegerType(), True) \
        .add("series_point_observationDateTime", TimestampType(), False) \
        .add("series_point_quantity", quantity_type, True) \
        .add("series_point_quality", StringType(), True) \
        .add("correlationId", StringType(), False)

    # For right now, this is the simplest solution for getting master/parsed data
    # This should be improved
    @staticmethod
    def get_instance(schema_name: SchemaNames):
        if schema_name is SchemaNames.Master:
            return SchemaFactory.master_schema
        elif schema_name is SchemaNames.Parquet:
            return SchemaFactory.parquet_schema
        else:
            return None
