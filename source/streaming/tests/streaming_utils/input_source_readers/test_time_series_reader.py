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
from pyspark.sql.types import StructType
from geh_stream.schemas import SchemaFactory, SchemaNames
from geh_stream.streaming_utils.input_source_readers.time_series_reader import __parse_stream, __to_quantity, __get_flattened_time_series_points
from decimal import Decimal


def test_parse_invalid_series_from_stream(timeseries_protobuf_factory, event_hub_message_df_factory):
    "Test invalid series is parsed correctly from stream"
    expected_metering_point_id = "non-existing metering point id 123498hhkjwer8"
    time_series_protobuf = timeseries_protobuf_factory(metering_point_id=expected_metering_point_id)
    event_hub_message_df = event_hub_message_df_factory(time_series_protobuf)

    parsed_time_series_point_stream = __parse_stream(event_hub_message_df)

    first = parsed_time_series_point_stream.first()
    assert first.series_meteringPointId == expected_metering_point_id


def test_parse_series_from_stream(timeseries_protobuf_factory, event_hub_message_df_factory):
    "Test series is parsed correctly from stream"
    expected_metering_point_id = "571313180000000005"
    time_series_protobuf = timeseries_protobuf_factory(metering_point_id=expected_metering_point_id)
    event_hub_message_df = event_hub_message_df_factory(time_series_protobuf)

    parsed_time_series_point_stream = __parse_stream(event_hub_message_df)

    first = parsed_time_series_point_stream.first()
    assert first.series_meteringPointId == expected_metering_point_id


def test_parse_series_point_observationDateTime_from_stream(timeseries_protobuf_factory, event_hub_message_df_factory):
    "Test series point observationDateTime is parsed correctly from stream"
    expected_observation_date_time = "2020-11-12T23:00:00Z"
    time_series_protobuf = timeseries_protobuf_factory(observation_date_time=expected_observation_date_time)
    event_hub_message_df = event_hub_message_df_factory(time_series_protobuf)

    parsed_time_series_point_stream = __parse_stream(event_hub_message_df)

    first = parsed_time_series_point_stream.first()
    assert first.series_point_observationDateTime.isoformat() + "Z" == expected_observation_date_time


def test_parse_series_point_quantity_0_337_from_stream(timeseries_protobuf_factory, event_hub_message_df_factory):
    "Test series point quantity is parsed correctly from stream"
    expected_quantity = Decimal('0.337')
    time_series_protobuf = timeseries_protobuf_factory(quantity=expected_quantity)
    event_hub_message_df = event_hub_message_df_factory(time_series_protobuf)

    parsed_time_series_point_stream = __parse_stream(event_hub_message_df)

    first = parsed_time_series_point_stream.first()
    assert first.series_point_quantity == expected_quantity


def test_parse_series_point_quantity_2_000_from_stream(timeseries_protobuf_factory, event_hub_message_df_factory):
    "Test series point quantity is parsed correctly from stream"
    expected_quantity = Decimal('2.000')
    time_series_protobuf = timeseries_protobuf_factory(quantity=expected_quantity)
    event_hub_message_df = event_hub_message_df_factory(time_series_protobuf)

    parsed_time_series_point_stream = __parse_stream(event_hub_message_df)

    first = parsed_time_series_point_stream.first()
    assert first.series_point_quantity == expected_quantity


def test_parse_series_point_quantity_3_100_from_stream(timeseries_protobuf_factory, event_hub_message_df_factory):
    "Test series point quantity is parsed correctly from stream"
    expected_quantity = Decimal('3.100')
    time_series_protobuf = timeseries_protobuf_factory(quantity=expected_quantity)
    event_hub_message_df = event_hub_message_df_factory(time_series_protobuf)

    parsed_time_series_point_stream = __parse_stream(event_hub_message_df)

    first = parsed_time_series_point_stream.first()
    assert first.series_point_quantity == expected_quantity


def test_get_flattened_time_series_points(parsed_data):
    "Test __get_flattened_time_series_points"
    flattened_time_series_points = __get_flattened_time_series_points(parsed_data)

    first = flattened_time_series_points.first()
    assert first.series_id == "seriesid1"
    assert "correlation_id" in flattened_time_series_points.columns
    assert "document_id" in flattened_time_series_points.columns
    assert "series_id" in flattened_time_series_points.columns


def test_to_quantity():
    "Test to_quantity"
    expected = Decimal('12345.6789')

    units = 12345
    nanos = 678900000

    returnValue = __to_quantity(units, nanos)

    assert returnValue == expected


def test_to_quantity_with_None():
    "Test to_quantity with None values"
    expected = Decimal('0.0')

    units = None
    nanos = None

    returnValue = __to_quantity(units, nanos)

    assert returnValue == expected
