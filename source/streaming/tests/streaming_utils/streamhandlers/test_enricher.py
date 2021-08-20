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
import time
from pyspark import SparkConf
from pyspark.sql import DataFrame, SparkSession

from geh_stream.streaming_utils.streamhandlers import Enricher
from geh_stream.dataframelib import has_column

# Create timestamps used in DataFrames
time_now = time.time()
time_future = time.time() + 1000
time_past = time.time() - 1000
time_far_future = time.time() + 10000
timestamp_now = pd.Timestamp(time_now, unit='s')
timestamp_future = pd.Timestamp(time_future, unit='s')
timestamp_past = pd.Timestamp(time_past, unit='s')
timestamp_far_future = pd.Timestamp(time_far_future, unit='s')


# Run the enrich function
@pytest.fixture(scope="class")
def enriched_data(parsed_data_factory, master_data_factory):

    parsed_data1 = parsed_data_factory(metering_point_id="1", quantity=1.0, observation_date_time=timestamp_now)
    parsed_data2 = parsed_data_factory(metering_point_id="1", quantity=2.0, observation_date_time=timestamp_far_future)
    parsed_data3 = parsed_data_factory(metering_point_id="2", quantity=3.0, observation_date_time=timestamp_now)

    # parsed_data1 = parsed_data_factory(metering_point_id="1")
    # parsed_data2 = parsed_data_factory(metering_point_id="1")
    # parsed_data3 = parsed_data_factory(metering_point_id="2")

    parsed_data = parsed_data1.union(parsed_data2).union(parsed_data3)

    # parsed_data = parsed_data_factory([
    #     dict(metering_point_id="1", quantity=1.0, observation_time=timestamp_now),
    #     # Not matched because it's outside the master data valid period
    #     dict(metering_point_id="1", quantity=2.0, observation_time=timestamp_far_future),
    #     # Not matched because no master data exists for this market evalution point
    #     dict(metering_point_id="2", quantity=3.0, observation_time=timestamp_now)
    # ])
    master_data = master_data_factory(dict(metering_point_id="1"))
    return Enricher.enrich(parsed_data, master_data)


# Is the row count maintained
def test_enrich_returns_correct_row_count(enriched_data):
    assert enriched_data.count() == 3


def test_enricher_adds_metering_point_type(enriched_data):
    assert has_column(enriched_data, "md.meteringPointType")


def test_enricher_adds_settlement_method(enriched_data):
    assert has_column(enriched_data, "md.settlementMethod")
