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
from package.codelists import Colname
from pyspark.sql.types import (
    DecimalType,
    StructType,
    StructField,
    StringType,
    TimestampType,
    IntegerType,
    LongType,
    ArrayType,
)

time_series_unprocessed_schema = StructType(
    [
        StructField("BusinessReasonCode", LongType(), True),
        StructField("CreatedDateTime", TimestampType(), True),
        StructField("DocumentId", StringType(), True),
        StructField("MeasureUnit", LongType(), True),
        StructField("MeteringPointId", StringType(), True),
        StructField("MeteringPointType", LongType(), True),
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
                                    StructField("Position", LongType(), True),
                                    StructField("Quality", LongType(), True),
                                    StructField("Quantity", DecimalType(18, 3), True),
                                ]
                            )
                        ),
                    ),
                    StructField("Resolution", LongType(), True),
                    StructField("StartDateTime", TimestampType(), True),
                ]
            ),
        ),
        StructField("Product", StringType(), True),
        StructField(
            "Receiver",
            StructType(
                [
                    StructField("BusinessProcessRole", LongType(), True),
                    StructField("Id", StringType(), True),
                ]
            ),
        ),
        StructField("RegistrationDateTime", TimestampType(), True),
        StructField(
            "Sender",
            StructType(
                [
                    StructField("BusinessProcessRole", LongType(), True),
                    StructField("Id", StringType(), True),
                ]
            ),
        ),
        StructField("SeriesId", StringType(), True),
        StructField("TransactionId", StringType(), True),
        StructField("year", IntegerType(), True),
        StructField("month", IntegerType(), True),
        StructField("day", IntegerType(), True),
    ]
)
