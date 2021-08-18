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
from geh_stream.streaming_utils.input_source_readers.time_series_reader import __parse_stream
from decimal import Decimal


def test_parse_series_from_stream(timeseries_protobuf_factory, event_hub_message_df_factory):
    "Test series is parsed correctly from stream"
    time_series_protobuf = timeseries_protobuf_factory(0, 0)
    event_hub_message_df = event_hub_message_df_factory(time_series_protobuf)

    parsed_time_series_point_stream = __parse_stream(event_hub_message_df)

    expected_metering_point_id = "meteringpointid1"
    first = parsed_time_series_point_stream.first()
    assert first.series_meteringPointId == expected_metering_point_id


def test_parse_series_point_observationDateTime_from_stream(timeseries_protobuf_factory, event_hub_message_df_factory):
    "Test series point observationDateTime is parsed correctly from stream"
    time_series_protobuf = timeseries_protobuf_factory(0, 0)
    event_hub_message_df = event_hub_message_df_factory(time_series_protobuf)

    parsed_time_series_point_stream = __parse_stream(event_hub_message_df)

    expected_observation_date_time = "2020-11-12T23:00:00Z"
    first = parsed_time_series_point_stream.first()
    assert first.series_point_observationDateTime.isoformat() + "Z" == expected_observation_date_time


def test_parse_series_point_quantity_0_337_from_stream(timeseries_protobuf_factory, event_hub_message_df_factory):
    "Test series point quantity is parsed correctly from stream"
    time_series_protobuf = timeseries_protobuf_factory(0, 337000000)
    event_hub_message_df = event_hub_message_df_factory(time_series_protobuf)

    parsed_time_series_point_stream = __parse_stream(event_hub_message_df)

    expected_quantity = Decimal('0.337')
    first = parsed_time_series_point_stream.first()
    assert first.series_point_quantity == expected_quantity


def test_parse_series_point_quantity_2_000_from_stream(timeseries_protobuf_factory, event_hub_message_df_factory):
    "Test series point quantity is parsed correctly from stream"
    time_series_protobuf = timeseries_protobuf_factory(2, 000000000)
    event_hub_message_df = event_hub_message_df_factory(time_series_protobuf)

    parsed_time_series_point_stream = __parse_stream(event_hub_message_df)

    expected_quantity = Decimal('2.000')
    first = parsed_time_series_point_stream.first()
    assert first.series_point_quantity == expected_quantity


def test_parse_series_point_quantity_3_100_from_stream(timeseries_protobuf_factory, event_hub_message_df_factory):
    "Test series point quantity is parsed correctly from stream"
    time_series_protobuf = timeseries_protobuf_factory(3, 100000000)
    event_hub_message_df = event_hub_message_df_factory(time_series_protobuf)

    parsed_time_series_point_stream = __parse_stream(event_hub_message_df)

    expected_quantity = Decimal('3.100')
    first = parsed_time_series_point_stream.first()
    assert first.series_point_quantity == expected_quantity
