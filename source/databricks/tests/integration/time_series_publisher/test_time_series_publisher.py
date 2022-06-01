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
from package.codelists.colname import Colname
from tests.integration.utils import streaming_job_asserter


def test_timeseries_publisher_returns_0(
    spark,
    databricks_path,
    delta_lake_path,
    unprocessed_time_series_json_string
):
    time_series_unprocessed_path = f"{delta_lake_path}/unprocessed_time_series"
    time_series_points_path = f"{delta_lake_path}/time_series_points"

    # Remove used Delta tables in order to avoid side effects from previous/other test runs
    if(os.path.exists(time_series_unprocessed_path)):
        shutil.rmtree(time_series_unprocessed_path)
    if(os.path.exists(time_series_points_path)):
        shutil.rmtree(time_series_points_path)

    # Add test data to data source
    columns = [Colname.timeseries, Colname.year, Colname.month, Colname.day, Colname.registration_date_time]
    time_series_data = [(unprocessed_time_series_json_string, 2022, 3, 21, "2022-12-17T09:30:47Z")]
    (spark
     .sparkContext
     .parallelize(time_series_data)
     .toDF(columns)
     .withColumn(Colname.registration_date_time, to_timestamp(Colname.registration_date_time))
     .write
     .format("delta")
     .save(time_series_unprocessed_path))

    exit_code = subprocess.call([
        "python",
        f"{databricks_path}/streaming-jobs/timeseries_publisher_streaming.py",
        "--data-storage-account-name", "data-storage-account-name",
        "--data-storage-account-key", "data-storage-account-key",
        "--time_series_unprocessed_path", f"{delta_lake_path}/unprocessed_time_series",
        "--time_series_points_path", f"{delta_lake_path}/time_series_points",
        "--time_series_checkpoint_path", f"{delta_lake_path}/time_series_points/checkpoint",
        ])

    # Assert
    assert exit_code == 0, "Preparation job did not return exit code 0"


@pytest.fixture(scope="session")
def time_series_publisher(spark, delta_lake_path, integration_tests_path, unprocessed_time_series_json_string):
    # Setup paths
    checkpoint_path = f"{delta_lake_path}/time_series_points/checkpoint"
    time_series_unprocessed_path = f"{delta_lake_path}/unprocessed_time_series"
    time_series_points_path = f"{delta_lake_path}/time_series_points"

    # Remove used Delta tables in order to avoid side effects from previous/other test runs
    if(os.path.exists(time_series_unprocessed_path)):
        shutil.rmtree(time_series_unprocessed_path)
    if(os.path.exists(time_series_points_path)):
        shutil.rmtree(time_series_points_path)

    # Add test data to data source
    columns = [Colname.timeseries, Colname.year, Colname.month, Colname.day, Colname.registration_date_time]
    time_series_data = [(unprocessed_time_series_json_string, 2022, 3, 21, "2022-12-17T09:30:47Z")]
    (spark
     .sparkContext
     .parallelize(time_series_data)
     .toDF(columns)
     .withColumn(Colname.registration_date_time, to_timestamp(Colname.registration_date_time))
     .write
     .format("delta")
     .save(time_series_unprocessed_path))

    # Return the awaitable pyspark streaming job (the sut)
    return timeseries_publisher(spark, time_series_unprocessed_path, checkpoint_path, time_series_points_path)


@pytest.mark.asyncio
async def test_publishes_points(delta_reader, time_series_publisher):
    def verification_function():
        data = delta_reader("/time_series_points")
        return data.count() > 0

    succeeded = streaming_job_asserter(time_series_publisher, verification_function)
    assert succeeded, "No data was stored in Delta table"
