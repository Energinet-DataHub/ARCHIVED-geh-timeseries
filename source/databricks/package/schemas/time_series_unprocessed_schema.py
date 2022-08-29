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
from pyspark.sql.types import (
    DecimalType,
    StructType,
    StructField,
    StringType,
    TimestampType,
    IntegerType,
    ByteType,
    ArrayType,
    ShortType,
)

# Be expressive about that they are represented as enums in the Python code
EnumType = ByteType

time_series_unprocessed_schema = StructType(
    [
        StructField("BusinessReasonCode", EnumType(), True),
        StructField("CreatedDateTime", TimestampType(), True),
        StructField("DocumentId", StringType(), True),
        StructField("MeasureUnit", EnumType(), True),
        StructField("GsrnNumber", StringType(), True),
        StructField("MeteringPointType", EnumType(), True),
        StructField(
            "Period",
            StructType(
                [
                    StructField("EndDateTime", TimestampType(), True),
                    StructField(
                        "Points",
                        ArrayType(
                            StructType(
                                [
                                    StructField("Position", IntegerType(), True),
                                    StructField("Quality", EnumType(), True),
                                    StructField("Quantity", DecimalType(18, 3), True),
                                ]
                            )
                        ),
                    ),
                    StructField("Resolution", EnumType(), True),
                    StructField("StartDateTime", TimestampType(), True),
                ]
            ),
        ),
        StructField("Product", EnumType(), True),
        StructField(
            "Receiver",
            StructType(
                [
                    StructField("BusinessProcessRole", EnumType(), True),
                    StructField("Id", StringType(), True),
                ]
            ),
        ),
        StructField("RegistrationDateTime", TimestampType(), True),
        StructField(
            "Sender",
            StructType(
                [
                    StructField("BusinessProcessRole", EnumType(), True),
                    StructField("Id", StringType(), True),
                ]
            ),
        ),
        StructField("SeriesId", StringType(), True),
        StructField("TransactionId", StringType(), True),
        StructField("year", ShortType(), True),
        StructField("month", ByteType(), True),
        StructField("day", ByteType(), True),
    ]
)
