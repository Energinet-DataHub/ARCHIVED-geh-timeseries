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
import pytest
from package import timeseries_persister
import time
from pyspark.sql.types import (
    StructType,
    StructField,
    StringType,
    ArrayType,
    DecimalType,
    IntegerType,
    TimestampType,
    BooleanType,
    BinaryType,
    LongType,
)


# Code from https://stackoverflow.com/questions/45717433/stop-structured-streaming-query-gracefully
# Helper method to stop a streaming query
def stop_stream_query(query, wait_time):
    """Stop a running streaming query"""
    while query.isActive:
        msg = query.status["message"]
        data_avail = query.status["isDataAvailable"]
        trigger_active = query.status["isTriggerActive"]
        if not data_avail and not trigger_active and msg != "Initializing sources":
            print("Stopping query...")
            query.stop()
        time.sleep(0.5)

    # Okay wait for the stop to happen
    print("Awaiting termination...")
    query.awaitTermination(wait_time)


async def job_task(job):
    try:
        job.awaitTermination()
    except asyncio.CancelledError:
        stop_stream_query(job, 5000)
        # raise


@pytest.mark.asyncio
async def test_time_series_persister(delta_reader, time_series_persister):
    task = asyncio.create_task(job_task(time_series_persister))
    for x in range(20000):
        data = delta_reader("/unprocessed_time_series")
        if data is not None and data.count() > 0:
            task.cancel()
            return

    task.cancel()
    assert False, "No data was stored in Delta table"
