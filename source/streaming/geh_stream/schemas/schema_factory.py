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


class SchemaFactory:
    quantity_type = DecimalType(18, 3)

    message_body_schema: StructType = StructType() \
        .add("document", StructType()
             .add("id", StringType(), False)
             .add("requestDateTime", TimestampType(), False)
             .add("type", IntegerType(), False)
             .add("createdDateTime", TimestampType(), False)
             .add("sender", StructType()
                  .add("id", StringType(), False)
                  .add("businessProcessRole", IntegerType(), False), False)
             .add("recipient", StructType()
                  .add("id", StringType(), False)
                  .add("businessProcessRole", IntegerType(), True), False)
             .add("businessReasonCode", IntegerType(), False), False) \
        .add("series", StructType()
             .add("id", StringType(), False)
             .add("meteringPointId", StringType(), False)
             .add("meteringPointType", IntegerType(), False)
             .add("settlementMethod", IntegerType(), True)
             .add("registrationDateTime", TimestampType(), False)
             .add("product", IntegerType(), False)
             .add("unit", IntegerType(), False)
             .add("resolution", IntegerType(), False)
             .add("startDateTime", TimestampType(), False)
             .add("endDateTime", TimestampType(), False)
             .add("points", ArrayType(StructType()
                  .add("position", IntegerType(), False)
                  .add("observationTime", TimestampType(), False)
                  .add("quantity", quantity_type, False)
                  .add("quality", IntegerType(), False), True), False), False) \
        .add("transaction", StructType()
            .add("mRID", StringType(), False), False) \
        .add("correlationId", StringType(), False)

    # ValidFrom and ValidTo are not to be included in outputs from the time series point streaming process
    master_schema: StructType = StructType() \
        .add("meteringPointId", StringType(), False) \
        .add("ValidFrom", TimestampType(), False) \
        .add("ValidTo", TimestampType(), True) \
        .add("MeterReadingPeriodicity", StringType(), False) \
        .add("MeteringMethod", StringType(), False) \
        .add("MeteringGridArea_Domain_mRID", StringType(), True) \
        .add("ConnectionState", StringType(), True) \
        .add("EnergySupplier_MarketParticipant_mRID", StringType(), True) \
        .add("BalanceResponsibleParty_MarketParticipant_mRID", StringType(), True) \
        .add("InMeteringGridArea_Domain_mRID", StringType(), False) \
        .add("OutMeteringGridArea_Domain_mRID", StringType(), False) \
        .add("Parent_Domain_mRID", StringType(), False) \
        .add("ServiceCategory_Kind", StringType(), False) \
        .add("meteringPointType", StringType(), False) \
        .add("settlementMethod", IntegerType(), False) \
        .add("unit", IntegerType(), False) \
        .add("product", IntegerType(), False) \
        .add("Technology", StringType(), True) \
        .add("OutMeteringGridArea_Domain_Owner_mRID", StringType(), False) \
        .add("InMeteringGridArea_Domain_Owner_mRID", StringType(), False) \
        .add("DistributionList", StringType(), False)

    distribution_list_schema: ArrayType = ArrayType(
        StructType([
            StructField("mRID", StringType(), False),
            StructField("role", IntegerType(), False)]))

    parsed_schema = copy.deepcopy(message_body_schema) \
        .add("EventHubEnqueueTime", TimestampType(), True)

    # NOTE: This is a workaround because for some unknown reason pyspark parsing from JSON
    #       (in event_hub_parser.py) causes all to be nullable regardless of the schema
    make_all_nullable(parsed_schema)

    parquet_schema: StructType = StructType() \
        .add("correlationId", StringType(), False) \
        .add("document_id", StringType(), False) \
        .add("createdDateTime", TimestampType(), False) \
        .add("sender_id", StringType(), False) \
        .add("businessReasonCode", StringType(), False) \
        .add("sender_businessProcessRole", StringType(), False) \
        .add("series_id", StringType(), False) \
        .add("product", StringType(), False) \
        .add("unit", StringType(), False) \
        .add("meteringPointType", StringType(), False) \
        .add("settlementMethod", StringType(), True) \
        .add("meteringPointId", StringType(), False) \
        .add("quantity", quantity_type, True) \
        .add("quality", StringType(), True) \
        .add("observationTime", TimestampType(), False) \
        .add("MeteringMethod", StringType(), True) \
        .add("MeterReadingPeriodicity", StringType(), True) \
        .add("MeteringGridArea_Domain_mRID", StringType(), False) \
        .add("ConnectionState", StringType(), False) \
        .add("EnergySupplier_MarketParticipant_mRID", StringType(), False) \
        .add("BalanceResponsibleParty_MarketParticipant_mRID", StringType(), False) \
        .add("InMeteringGridArea_Domain_mRID", StringType(), True) \
        .add("OutMeteringGridArea_Domain_mRID", StringType(), True) \
        .add("Parent_Domain_mRID", StringType(), True) \
        .add("ServiceCategory_Kind", StringType(), True) \
        .add("Technology", StringType(), True)

    # For right now, this is the simplest solution for getting master/parsed data
    # This should be improved
    @staticmethod
    def get_instance(schema_name: SchemaNames):
        if schema_name is SchemaNames.Parsed:
            return SchemaFactory.parsed_schema
        elif schema_name is SchemaNames.Master:
            return SchemaFactory.master_schema
        elif schema_name is SchemaNames.MessageBody:
            return SchemaFactory.message_body_schema
        elif schema_name is SchemaNames.Parquet:
            return SchemaFactory.parquet_schema
        elif schema_name is SchemaNames.DistributionList:
            return SchemaFactory.distribution_list_schema
        else:
            return None
