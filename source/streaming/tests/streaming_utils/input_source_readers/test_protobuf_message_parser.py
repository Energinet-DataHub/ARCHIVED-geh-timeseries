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
from geh_stream.streaming_utils.input_source_readers.protobuf_message_parser import ProtobufMessageParser
from geh_stream.protodf import schema_for
from geh_stream.contracts.time_series_pb2 import TimeSeriesCommand


def test_parse_data_returns_expected_metering_point_id(timeseries_protobuf_factory, event_hub_message_df_factory, data_parsed_from_protobuf_schema):
    "Test Parse data from protobuf messages"
    metering_point_id = "571313180000000005"
    time_series_protobuf = timeseries_protobuf_factory(metering_point_id=metering_point_id)
    event_hub_message_df = event_hub_message_df_factory(time_series_protobuf)

    parsed_data = ProtobufMessageParser.parse(event_hub_message_df)

    first_series = parsed_data.first().series
    assert first_series.metering_point_id == metering_point_id


def test_parse_data_return_dataframe_with_correct_schema(timeseries_protobuf_factory, event_hub_message_df_factory, data_parsed_from_protobuf_schema):
    "Test Parse data from protobuf messages"
    time_series_protobuf = timeseries_protobuf_factory()
    event_hub_message_df = event_hub_message_df_factory(time_series_protobuf)
    parsed_data = ProtobufMessageParser.parse(event_hub_message_df)

    assert str(parsed_data.schema) == str(data_parsed_from_protobuf_schema)


def test_parse_event_hub_message_returns_correct_schema(parsed_data):
    "Check that resulting DataFrame has expected schema"
    schema = schema_for(TimeSeriesCommand().DESCRIPTOR)
    assert str(parsed_data.schema) == str(schema)
