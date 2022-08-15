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

from package.schemas.time_series_points import (
    time_series_points_schema,
    TimeSeriesPointsColname as TSPColname,
)
from package.schemas.time_series_unprocessed import (
    time_series_unprocessed_schema,
    TimeSeriesUnprocessedColname as TSUColname,
)
from pyspark.sql.types import (
    StructType,
    StructField,
    StringType,
    IntegerType,
    TimestampType,
)
from datetime import datetime
from decimal import Decimal
from package.transforms.jsonTransformer import (
    transform_unprocessed_time_series_to_points,
)

date_time_formatting_string = "%Y-%m-%dT%H:%M:%S.%fZ"


def test_time_series(time_series_unprocessed_factory, time_series_points_factory):
    time_series_unprocessed_df = time_series_unprocessed_factory(
        StartDateTime=datetime.strptime(
            "2022-06-08T12:09:15.000Z", date_time_formatting_string
        ),
        RegistrationDateTime=None,
    )

    expected_registration_data_time = time_series_unprocessed_df.collect()[0][
        TSUColname.CreatedDateTime
    ]

    expected_df = time_series_points_factory().drop("storedTime")

    actual_df = transform_unprocessed_time_series_to_points(
        time_series_unprocessed_df
    ).drop("storedTime")

    actual_df.show()
    expected_df.show()

    assert (
        actual_df.collect()[0][TSPColname.RegistrationDateTime]
        == expected_registration_data_time
    )
    assert actual_df.schema == expected_df.schema
    assert actual_df.collect() == expected_df.collect()


@pytest.fixture(scope="module")
def time_series_unprocessed_factory(spark):
    def factory(
        StartDateTime: TimestampType(),
        RegistrationDateTime: TimestampType(),
    ):
        df = [
            {
                TSUColname.BusinessReasonCode: 0,
                TSUColname.CreatedDateTime: datetime.strptime(
                    "2022-06-09T12:09:15.000Z", date_time_formatting_string
                ),
                TSUColname.DocumentId: "1",
                TSUColname.MeasureUnit: 0,
                TSUColname.MeteringPointId: "1",
                TSUColname.MeteringPointType: 2,
                TSUColname.Period: {
                    TSUColname.EndDateTime: StartDateTime,
                    TSUColname.Points: [
                        {
                            TSUColname.Position: 1,
                            TSUColname.Quality: 3,
                            TSUColname.Quantity: Decimal(1.1),
                        },
                        {
                            TSUColname.Position: 1,
                            TSUColname.Quality: 3,
                            TSUColname.Quantity: Decimal(1.1),
                        },
                    ],
                    TSUColname.Resolution: 2,
                    TSUColname.StartDateTime: datetime.strptime(
                        "2022-06-08T12:09:15.000Z", date_time_formatting_string
                    ),
                },
                TSUColname.Product: "1",
                TSUColname.Receiver: {
                    TSUColname.BusinessProcessRole: 0,
                    TSUColname.Id: "2",
                },
                TSUColname.RegistrationDateTime: RegistrationDateTime,
                TSUColname.Sender: {
                    TSUColname.BusinessProcessRole: 0,
                    TSUColname.Id: "1",
                },
                TSUColname.SeriesId: "1",
                TSUColname.TransactionId: "1",
                TSUColname.Year: 2022,
                TSUColname.Month: 6,
                TSUColname.Day: 9,
            }
        ]

        return spark.createDataFrame(df, time_series_unprocessed_schema)

    return factory


@pytest.fixture(scope="module")
def time_series_points_factory(spark):
    def factory():
        df = [
            {
                TSPColname.MeteringPointId: 1,
                TSPColname.TransactionId: 1,
                TSPColname.Quantity: Decimal(1.1),
                TSPColname.Quality: 3,
                TSPColname.Position: 1,
                TSPColname.Resolution: 2,
                TSPColname.StartDateTime: datetime.strptime(
                    "2022-06-08T12:09:15.000Z", date_time_formatting_string
                ),
                TSPColname.RegistrationDateTime: datetime.strptime(
                    "2022-06-09T12:09:15.000Z", date_time_formatting_string
                ),
                TSPColname.CreatedDateTime: datetime.strptime(
                    "2022-06-09T12:09:15.000Z", date_time_formatting_string
                ),
                "storedTime": None,
                TSPColname.Time: datetime.strptime(
                    "2022-06-08T12:09:15.000Z", date_time_formatting_string
                ),
                TSPColname.Year: 2022,
                TSPColname.Month: 6,
                TSPColname.Day: 8,
            },
            {
                TSPColname.MeteringPointId: 1,
                TSPColname.TransactionId: 1,
                TSPColname.Quantity: Decimal(1.1),
                TSPColname.Quality: 3,
                TSPColname.Position: 1,
                TSPColname.Resolution: 2,
                TSPColname.StartDateTime: datetime.strptime(
                    "2022-06-08T12:09:15.000Z", date_time_formatting_string
                ),
                TSPColname.RegistrationDateTime: datetime.strptime(
                    "2022-06-09T12:09:15.000Z", date_time_formatting_string
                ),
                TSPColname.CreatedDateTime: datetime.strptime(
                    "2022-06-09T12:09:15.000Z", date_time_formatting_string
                ),
                "storedTime": None,
                TSPColname.Time: datetime.strptime(
                    "2022-06-08T12:09:15.000Z", date_time_formatting_string
                ),
                TSPColname.Year: 2022,
                TSPColname.Month: 6,
                TSPColname.Day: 8,
            },
        ]

        return spark.createDataFrame(df, time_series_points_schema)

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
