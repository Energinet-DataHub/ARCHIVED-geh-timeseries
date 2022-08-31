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

import sys

sys.path.append(r"/workspaces/geh-timeseries/source/databricks")

import asyncio
import shutil
import pytest
import subprocess
import os
from pyspark.sql import SparkSession
from pyspark.sql.functions import to_timestamp
from package import timeseries_publisher
from tests.integration.utils import streaming_job_asserter
from package.schemas import (
    time_series_unprocessed_schema,
    published_time_series_points_schema,
)
from tests.contract_utils import assert_contract_matches_schema


def test__timeseries_publisher_returns_exit_code_0(
    spark, databricks_path, data_lake_path
):
    time_series_unprocessed_path = f"{data_lake_path}/unprocessed_time_series"
    time_series_points_path = f"{data_lake_path}/time_series_points"
    time_series_checkpoint_path = f"{data_lake_path}/time_series_points_checkpoint"
    time_series_unprocessed_path_json = f"{data_lake_path}/unprocessed_time_series_json"

    # Remove used Delta tables in order to avoid side effects from previous/other test runs
    if os.path.exists(time_series_unprocessed_path):
        shutil.rmtree(time_series_unprocessed_path)
    if os.path.exists(time_series_points_path):
        shutil.rmtree(time_series_points_path)
    if os.path.exists(time_series_checkpoint_path):
        shutil.rmtree(time_series_checkpoint_path)
    if os.path.exists(time_series_unprocessed_path_json):
        shutil.rmtree(time_series_unprocessed_path_json)

    # Add test data to data source
    os.makedirs(time_series_unprocessed_path_json)
    f = open(f"{time_series_unprocessed_path_json}/test.json", "w")
    f.write(
        '{"BusinessReasonCode":0,"CreatedDateTime":"2022-06-09T12:09:15.000Z","MeasureUnit":0,"MeteringPointId":"1","MeteringPointType":2,"Period":{"EndDateTime":"2022-06-09T12:09:15.000Z","Points":[{"Position":1,"Quality":3,"Quantity":"1.1"},{"Position":1,"Quality":3,"Quantity":"1.1"}],"Resolution":2,"StartDateTime":"2022-06-08T12:09:15.000Z"},"Product":"1","Receiver":{"BusinessProcessRole":0,"Id":"2"},"RegistrationDateTime":"2022-06-09T12:09:15.000Z","Sender":{"BusinessProcessRole":0,"Id":"1"},"SeriesId":"1","TransactionId":"1","year":2022,"month":6,"day":9}\n'
    )
    f.write(
        '{"BusinessReasonCode":0,"CreatedDateTime":"2022-06-09T12:09:15.000Z","MeasureUnit":0,"MeteringPointId":"1","MeteringPointType":2,"Period":{"EndDateTime":"2022-06-10T12:09:15.000Z","Points":[{"Position":1,"Quality":3,"Quantity":"1.1"},{"Position":1,"Quality":3,"Quantity":"1.1"}],"Resolution":2,"StartDateTime":"2022-06-09T12:09:15.000Z"},"Product":"1","Receiver":{"BusinessProcessRole":0,"Id":"2"},"RegistrationDateTime":"2022-06-09T12:09:15.000Z","Sender":{"BusinessProcessRole":0,"Id":"2"},"SeriesId":"1","TransactionId":"1","year":2022,"month":6,"day":9}\n'
    )
    f.write(
        '{"BusinessReasonCode":0,"CreatedDateTime":"2022-06-09T12:09:15.000Z","MeasureUnit":0,"MeteringPointId":"1","MeteringPointType":2,"Period":{"EndDateTime":"2022-06-11T12:09:15.000Z","Points":[{"Position":1,"Quality":3,"Quantity":"1.1"},{"Position":1,"Quality":3,"Quantity":"1.1"}],"Resolution":2,"StartDateTime":"2022-06-10T12:09:15.000Z"},"Product":"1","Receiver":{"BusinessProcessRole":0,"Id":"2"},"RegistrationDateTime":"2022-06-09T12:09:15.000Z","Sender":{"BusinessProcessRole":0,"Id":"3"},"SeriesId":"1","TransactionId":"1","year":2022,"month":6,"day":9}\n'
    )
    f.close()
    # Add test data to data source
    (
        spark.read.schema(time_series_unprocessed_schema)
        .json(time_series_unprocessed_path_json)
        .write.format("parquet")
        .save(time_series_unprocessed_path)
    )

    exit_code = subprocess.call(
        [
            "python",
            f"{databricks_path}/package/timeseries_publisher_streaming.py",
            "--data-storage-account-name",
            "data-storage-account-name",
            "--data-storage-account-key",
            "data-storage-account-key",
            "--time_series_unprocessed_path",
            f"{data_lake_path}/unprocessed_time_series",
            "--time_series_points_path",
            f"{data_lake_path}/time_series_points",
            "--time_series_checkpoint_path",
            f"{data_lake_path}/time_series_points_checkpoint",
        ]
    )

    # Assert
    assert exit_code == 0, "Time-series publisher job did not return exit code 0"


@pytest.fixture(scope="session")
def time_series_publisher(spark, data_lake_path, integration_tests_path):
    # Setup paths
    time_series_checkpoint_path = f"{data_lake_path}/time_series_points_checkpoint"
    time_series_unprocessed_path = f"{data_lake_path}/unprocessed_time_series"
    time_series_unprocessed_path_json = f"{data_lake_path}/unprocessed_time_series_json"
    time_series_points_path = f"{data_lake_path}/time_series_points"

    # Remove used Datalake tables in order to avoid side effects from previous/other test runs
    if os.path.exists(time_series_unprocessed_path):
        shutil.rmtree(time_series_unprocessed_path)
    if os.path.exists(time_series_points_path):
        shutil.rmtree(time_series_points_path)
    if os.path.exists(time_series_checkpoint_path):
        shutil.rmtree(time_series_checkpoint_path)
    if os.path.exists(time_series_unprocessed_path_json):
        shutil.rmtree(time_series_unprocessed_path_json)

    os.makedirs(time_series_unprocessed_path_json)
    f = open(f"{time_series_unprocessed_path_json}/test.json", "w")
    f.write(
        '{"BusinessReasonCode":0,"CreatedDateTime":"2022-06-09T12:09:15.000Z","DocumentId":"1","MeasureUnit":0,"GsrnNumber":"1","MeteringPointType":2,"Period":{"EndDateTime":"2022-06-09T12:09:15.000Z","Points":[{"Position":1,"Quality":3,"Quantity":"1.1"},{"Position":1,"Quality":3,"Quantity":"1.1"}],"Resolution":2,"StartDateTime":"2022-06-08T12:09:15.000Z"},"Product":"1","Receiver":{"BusinessProcessRole":0,"Id":"2"},"RegistrationDateTime":"2022-06-09T12:09:15.000Z","Sender":{"BusinessProcessRole":0,"Id":"1"},"TransactionId":"1","year":2022,"month":6,"day":9}\n'
    )
    f.write(
        '{"BusinessReasonCode":0,"CreatedDateTime":"2022-06-09T12:09:15.000Z","DocumentId":"2","MeasureUnit":0,"GsrnNumber":"1","MeteringPointType":2,"Period":{"EndDateTime":"2022-06-10T12:09:15.000Z","Points":[{"Position":1,"Quality":3,"Quantity":"1.1"},{"Position":1,"Quality":3,"Quantity":"1.1"}],"Resolution":2,"StartDateTime":"2022-06-09T12:09:15.000Z"},"Product":"1","Receiver":{"BusinessProcessRole":0,"Id":"2"},"RegistrationDateTime":"2022-06-09T12:09:15.000Z","Sender":{"BusinessProcessRole":0,"Id":"2"},"TransactionId":"1","year":2022,"month":6,"day":9}\n'
    )
    f.write(
        '{"BusinessReasonCode":0,"CreatedDateTime":"2022-06-09T12:09:15.000Z","DocumentId":"3","MeasureUnit":0,"GsrnNumber":"1","MeteringPointType":2,"Period":{"EndDateTime":"2022-06-11T12:09:15.000Z","Points":[{"Position":1,"Quality":3,"Quantity":"1.1"},{"Position":1,"Quality":3,"Quantity":"1.1"}],"Resolution":2,"StartDateTime":"2022-06-10T12:09:15.000Z"},"Product":"1","Receiver":{"BusinessProcessRole":0,"Id":"2"},"RegistrationDateTime":"2022-06-09T12:09:15.000Z","Sender":{"BusinessProcessRole":0,"Id":"3"},"TransactionId":"1","year":2022,"month":6,"day":9}\n'
    )

    f.close()
    # Add test data to data source
    (
        spark.read.schema(time_series_unprocessed_schema)
        .json(time_series_unprocessed_path_json)
        .write.format("parquet")
        .save(time_series_unprocessed_path)
    )

    # Return the awaitable pyspark streaming job (the sut)
    return timeseries_publisher(
        spark,
        time_series_unprocessed_path,
        time_series_checkpoint_path,
        time_series_points_path,
    )


@pytest.mark.asyncio
async def test__publishes_points(parquet_reader, time_series_publisher):
    def verification_function():
        data = parquet_reader("/time_series_points")
        return data.count() == 6

    succeeded = streaming_job_asserter(time_series_publisher, verification_function)
    assert succeeded, "No data was stored in Datalake table"


# @pytest.mark.asyncio
# async def test__publishes_points_that_comply_with_public_contract(
#     source_path, parquet_reader, time_series_publisher
# ):
#     def verification_function():
#         data = parquet_reader("/time_series_points")
#         assert_contract_matches_schema(
#             f"{source_path}/contracts/published-time-series-points.json", data.schema
#         )

#         return True

#     assert streaming_job_asserter(time_series_publisher, verification_function)


def test__defined_schema_complies_with_public_contract(source_path):
    assert_contract_matches_schema(
        f"{source_path}/contracts/published-time-series-points.json",
        published_time_series_points_schema,
    )
