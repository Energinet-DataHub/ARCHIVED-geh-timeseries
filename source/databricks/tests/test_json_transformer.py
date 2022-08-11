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

import pytest
import pandas as pd
from package.schemas import time_series_unprocessed_schema
from pyspark.sql.types import (
    StructType,
    StructField,
    StringType,
    IntegerType,
    TimestampType,
)
from datetime import datetime
from package.transforms.jsonTransformer import (
    transform_unprocessed_time_series_to_points,
)

date_time_formatting_string = "%Y-%m-%dT%H:%M:%S.%fZ"


def test_time_series(time_series_unprocessed_factory):
    df = time_series_unprocessed_factory(
        StartDateTime="2022-06-08T12:09:15.000Z",
        RegistrationDateTime="2022-06-09T12:09:15.000Z",
    )
    df2 = transform_unprocessed_time_series_to_points(df)
    df2.show()
    df2.printSchema()
    assert 1 == 1


@pytest.fixture(scope="module")
def time_series_unprocessed_factory(spark):
    def factory(
        StartDateTime: TimestampType(),
        RegistrationDateTime: TimestampType(),
    ):
        df = [
            {
                "BusinessReasonCode": 0,
                "CreatedDateTime": datetime.strptime(
                    "2022-06-09T12:09:15.000Z", date_time_formatting_string
                ),
                "DocumentId": "1",
                "MeasureUnit": 0,
                "MeteringPointId": "1",
                "MeteringPointType": 2,
                "Period": {
                    "EndDateTime": datetime.strptime(
                        StartDateTime, date_time_formatting_string
                    ),
                    "Points": [
                        {"Position": 1, "Quality": 3, "Quantity": "1.1"},
                        {"Position": 1, "Quality": 3, "Quantity": "1.1"},
                    ],
                    "Resolution": 2,
                    "StartDateTime": datetime.strptime(
                        "2022-06-08T12:09:15.000Z", date_time_formatting_string
                    ),
                },
                "Product": "1",
                "Receiver": {"BusinessProcessRole": 0, "Id": "2"},
                "RegistrationDateTime": datetime.strptime(
                    RegistrationDateTime, date_time_formatting_string
                ),
                "Sender": {"BusinessProcessRole": 0, "Id": "1"},
                "SeriesId": "1",
                "TransactionId": "1",
                "year": 2022,
                "month": 6,
                "day": 9,
            }
        ]

        return spark.createDataFrame(df, time_series_unprocessed_schema)

    return factory


this_schema = StructType(
    [
        StructField("id", IntegerType(), True),
        StructField("test", StringType(), True),
    ]
)


@pytest.fixture(scope="module")
def test_factory(spark):
    def factory(
        id: IntegerType(),
        test: StringType(),
    ):
        pandas_df = pd.DataFrame().append(
            [
                {
                    "id": id,
                    "test": test,
                }
            ],
            ignore_index=True,
        )

        return spark.createDataFrame(pandas_df)

    return factory


@pytest.fixture(scope="module")
def test_factory2(spark):
    def factory():
        pandas_df = [
            {
                "id": 123,
                "test": "test",
            }
        ]

        return spark.createDataFrame(pandas_df, this_schema)

    return factory


@pytest.fixture(scope="module")
def test_factory3(spark):
    def factory(
        id: IntegerType(),
        test: StringType(),
    ):
        pandas_df = [
            {
                "id": id,
                "test": test,
            }
        ]

        return spark.createDataFrame(pandas_df, this_schema)

    return factory


def test__this__thang(spark, test_factory, test_factory2, test_factory3):
    df1 = test_factory(12, "123")
    print("df1")
    df1.show()
    df1.printSchema()
    df2 = test_factory3(id=122, test="4132")
    print("df2")
    df2.show()
    df2.printSchema()
    df3 = spark.createDataFrame([{"id": 123, "test": "test"}], this_schema)
    print("df3")
    df3.show()
    df3.printSchema()
    df4 = spark.createDataFrame([(123, "test")], this_schema)
    print("df4")
    df4.show()
    df4.printSchema()
    df5 = test_factory2()
    print("df5")
    df5.show()
    df5.printSchema()
    for col in df5.dtypes:
        print(col[0] + " , " + col[1])
    assert 1 == 1
