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
from package.schemas import time_series_points_schema, time_series_unprocessed_schema
from pyspark.sql.types import (
    StructType,
    StructField,
    StringType,
    IntegerType,
    TimestampType,
    LongType,
)
from datetime import datetime
from decimal import Decimal
from package.transforms.jsonTransformer import (
    transform_unprocessed_time_series_to_points,
)


@pytest.fixture(scope="module")
def time_series_unprocessed_factory(spark):
    def factory(
        CreatedDateTime: TimestampType(),
        RegistrationDateTime: TimestampType(),
    ):
        df = [
            {
                "BusinessReasonCode": 0,
                "CreatedDateTime": timestamp("2022-06-09T12:09:15.000Z"),
                "DocumentId": "1",
                "MeasureUnit": 0,
                "MeteringPointId": "1",
                "MeteringPointType": 2,
                "Period": {
                    "EndDateTime": timestamp("2022-06-09T12:09:15.000Z"),
                    "Points": [
                        {
                            "Position": 1,
                            "Quality": 3,
                            "Quantity": Decimal(1.1),
                        },
                        {
                            "Position": 1,
                            "Quality": 3,
                            "Quantity": Decimal(1.1),
                        },
                    ],
                    "Resolution": 2,
                    "StartDateTime": timestamp("2022-06-08T12:09:15.000Z"),
                },
                "Product": "1",
                "Receiver": {
                    "BusinessProcessRole": 0,
                    "Id": "2",
                },
                "RegistrationDateTime": RegistrationDateTime,
                "Sender": {
                    "BusinessProcessRole": 0,
                    "Id": "1",
                },
                "SeriesId": "1",
                "TransactionId": "1",
                "year": 2022,
                "month": 6,
                "day": 9,
            }
        ]

        return spark.createDataFrame(df, time_series_unprocessed_schema)

    return factory


date_time_formatting_string = "%Y-%m-%dT%H:%M:%S.%fZ"


def timestamp(dts: StringType()) -> TimestampType():
    return datetime.strptime(dts, date_time_formatting_string)


@pytest.mark.parametrize(
    "registration_date_time, expected_registration_date_time",
    [
        (None, timestamp("2022-06-09T12:09:15.000Z")),
        (timestamp("2022-06-10T12:09:15.000Z"), timestamp("2022-06-10T12:09:15.000Z")),
    ],
)
def test__transform_unprocessed_time_series_to_points__registration_date_time_fallsback_to_created_date_time_when_none(
    time_series_unprocessed_factory,
    registration_date_time,
    expected_registration_date_time,
):
    # Arrange
    time_series_unprocessed_df = time_series_unprocessed_factory(
        RegistrationDateTime=registration_date_time,
        CreatedDateTime=timestamp("2022-06-09T12:09:15.000Z"),
    )

    # Act
    actual_df = transform_unprocessed_time_series_to_points(time_series_unprocessed_df)
    actual_registration_data_time = actual_df.collect()[0]["RegistrationDateTime"]

    # Assert
    assert actual_registration_data_time == expected_registration_date_time
