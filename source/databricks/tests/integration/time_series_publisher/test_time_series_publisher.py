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
import os
from pyspark.sql import SparkSession
sys.path.append(r"/workspaces/geh-timeseries/source/databricks")

import asyncio
import shutil
import pytest
from package import timeseries_publisher
from tests.integration.utils import streaming_job_asserter


#def time_series_publisher(spark: SparkSession, timeseries_unprocessed_path: str, checkpoint_path: str, timeseries_processed_path: str):
@pytest.fixture(scope="session")
def time_series_publisher(spark, delta_lake_path, integration_tests_path):
    checkpoint_path = f"{delta_lake_path}/time_series_points/checkpoint"
    time_series_unprocessed_path = f"{delta_lake_path}/unprocessed_time_series"
    time_series_points_path = f"{delta_lake_path}/time_series_points_path"

    if(os.path.exists(time_series_unprocessed_path)):
        shutil.rmtree(time_series_unprocessed_path)
    if(os.path.exists(time_series_points_path)):
        shutil.rmtree(time_series_points_path)

    # streamingDf = spark.readStream.schema(time_series_received_schema).json(
    #     f"{integration_tests_path}/time_series_persister/time_series_received*.json"
    # )
    
    return timeseries_publisher(spark, time_series_unprocessed_path, checkpoint_path, time_series_points_path)


@pytest.mark.asyncio
async def test_publishes_points(delta_reader, time_series_publisher):
    def verification_function():
        data = delta_reader("/time_series_points")
        return data.count() > 0

    succeeded = streaming_job_asserter(time_series_publisher, verification_function)
    assert succeeded, "No data was stored in Delta table"
