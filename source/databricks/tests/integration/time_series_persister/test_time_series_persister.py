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
import shutil
import subprocess
from pyspark.sql import SparkSession
sys.path.append(r"/workspaces/geh-timeseries/source/databricks")

import asyncio
import pytest
from package import timeseries_persister
from tests.integration.utils import streaming_job_asserter


def test_timeseries_persister_returns_0(
    spark,
    databricks_path,
    delta_lake_path
):
    time_series_raw_path = f"{delta_lake_path}/raw_time_series"
    time_series_unprocessed_path = f"{delta_lake_path}/unprocessed_time_series"
    time_series_checkpointpath = f"{delta_lake_path}/raw_time_series-checkpoint"

    # Remove test folders in order to avoid side effects from previous/other test runs
    if(os.path.exists(time_series_unprocessed_path)):
        shutil.rmtree(time_series_unprocessed_path)
    if(os.path.exists(time_series_raw_path)):
        shutil.rmtree(time_series_raw_path)
    if(os.path.exists(time_series_checkpointpath)):
        shutil.rmtree(time_series_checkpointpath)

    os.makedirs(time_series_raw_path)
    f = open(f"{time_series_raw_path}/test.json", 'w')
    f.write('{"time": "2022-06-09T12:09:15+00:00", "body": {"id": "111"}}')
    f.close()

    exit_code = subprocess.call([
        "python",
        f"{databricks_path}/streaming-jobs/timeseries_persister_streaming.py",
        "--data-storage-account-name", "data-storage-account-name",
        "--data-storage-account-key", "data-storage-account-key",
        "--time_series_unprocessed_path", f"{delta_lake_path}/unprocessed_time_series",
        "--time_series_raw_path", f"{delta_lake_path}/raw_time_series",
        "--time_series_checkpoint_path", f"{delta_lake_path}/raw_time_series-checkpoint"
    ])

    # Assert
    assert exit_code == 0, "Time-series publisher job did not return exit code 0"


def test_timeseries_persister_proccess_files(
    spark,
    databricks_path,
    delta_lake_path
):
    time_series_raw_path = f"{delta_lake_path}/raw_time_series"
    time_series_unprocessed_path = f"{delta_lake_path}/unprocessed_time_series"
    time_series_checkpointpath = f"{delta_lake_path}/raw_time_series-checkpoint"

    # Remove test folders in order to avoid side effects from previous/other test runs
    if(os.path.exists(time_series_unprocessed_path)):
        shutil.rmtree(time_series_unprocessed_path)
    if(os.path.exists(time_series_raw_path)):
        shutil.rmtree(time_series_raw_path)
    if(os.path.exists(time_series_checkpointpath)):
        shutil.rmtree(time_series_checkpointpath)

    os.makedirs(time_series_raw_path)
    f = open(f"{time_series_raw_path}/test.json", 'w')
    f.write('{"DocId":"1","CreatedDateTime":"2022-06-09T12:09:15+00:00","Sender":{"Id":"1","BusinessProcessRole":0},"Receiver":{"Id":"2","BusinessProcessRole":0},"BusinessReasonCode":0,"SeriesId":"1","TransactionId":"1","MeteringPointId":"1","MeteringPointType":2,"RegistrationDateTime":"2022-06-09T12:09:15+00:00","Product":"1","MeasureUnit":0,"Period":{"Resolution":2,"StartDateTime":"2022-06-08T12:09:15+00:00","EndDateTime":"2022-06-09T12:09:15+00:00","Points":[{"Quantity":1.1,"Quality":3,"Position":1},{"Quantity":1.1,"Quality":3,"Position":1}]}}\n')
    f.write('{"DocId":"2","CreatedDateTime":"2022-06-09T12:09:15+00:00","Sender":{"Id":"2","BusinessProcessRole":0},"Receiver":{"Id":"2","BusinessProcessRole":0},"BusinessReasonCode":0,"SeriesId":"1","TransactionId":"1","MeteringPointId":"1","MeteringPointType":2,"RegistrationDateTime":"2022-06-09T12:09:15+00:00","Product":"1","MeasureUnit":0,"Period":{"Resolution":2,"StartDateTime":"2022-06-09T12:09:15+00:00","EndDateTime":"2022-06-10T12:09:15+00:00","Points":[{"Quantity":1.1,"Quality":3,"Position":1},{"Quantity":1.1,"Quality":3,"Position":1}]}}\n')
    f.write('{"DocId":"3","CreatedDateTime":"2022-06-09T12:09:15+00:00","Sender":{"Id":"3","BusinessProcessRole":0},"Receiver":{"Id":"2","BusinessProcessRole":0},"BusinessReasonCode":0,"SeriesId":"1","TransactionId":"1","MeteringPointId":"1","MeteringPointType":2,"RegistrationDateTime":"2022-06-09T12:09:15+00:00","Product":"1","MeasureUnit":0,"Period":{"Resolution":2,"StartDateTime":"2022-06-10T12:09:15+00:00","EndDateTime":"2022-06-11T12:09:15+00:00","Points":[{"Quantity":1.1,"Quality":3,"Position":1},{"Quantity":1.1,"Quality":3,"Position":1}]}}\n')
    f.close()

    subprocess.call([
        "python",
        f"{databricks_path}/streaming-jobs/timeseries_persister_streaming.py",
        "--data-storage-account-name", "data-storage-account-name",
        "--data-storage-account-key", "data-storage-account-key",
        "--time_series_unprocessed_path", f"{delta_lake_path}/unprocessed_time_series",
        "--time_series_raw_path", f"{delta_lake_path}/raw_time_series",
        "--time_series_checkpoint_path", f"{delta_lake_path}/raw_time_series-checkpoint",
        "--test", "true"
    ])

    # Assert
    assert spark.read.parquet(time_series_unprocessed_path).count() == 3, "Time-series publisher job did not proccess files"
