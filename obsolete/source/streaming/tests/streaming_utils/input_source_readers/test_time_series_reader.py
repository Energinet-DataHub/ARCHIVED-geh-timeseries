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
import datetime
from pyspark.sql.types import StructType
from geh_stream.schemas import SchemaFactory, SchemaNames
from geh_stream.codelists import BusinessProcessRole, BusinessReasonCode, ResolutionDuration, \
    MeasureUnit, MeteringPointType, QuantityQuality, Product, SettlementMethod
from geh_stream.streaming_utils.input_source_readers.time_series_reader import \
    __parse_stream, __to_quantity, __get_flattened_time_series_points
from decimal import Decimal


def test_parse_stream_schema(timeseries_protobuf_factory, event_hub_message_df_factory, time_series_points_schema):
    "Test schema of streamed time series points"

    time_series_protobuf = timeseries_protobuf_factory()
    event_hub_message_df = event_hub_message_df_factory(time_series_protobuf)

    parsed_time_series_point_stream = __parse_stream(event_hub_message_df)

    parsed_data_schema = str(parsed_time_series_point_stream.schema)
    expected_schema = str(time_series_points_schema)

    assert parsed_data_schema == expected_schema


@pytest.mark.parametrize(
    "expected_metering_point_id",
    [
        pytest.param("non-existing metering point id 123498hhkjwer8", id="Non-existing metering_point_id is streamed"),
        pytest.param("571313180000000005", id="Existing metering_point_id is streamed"),
    ],
)
def test_parse_series_metering_point_id_from_stream(
        expected_metering_point_id, timeseries_protobuf_factory, event_hub_message_df_factory):
    "Test series is parsed correctly from stream"
    time_series_protobuf = timeseries_protobuf_factory(metering_point_id=expected_metering_point_id)
    event_hub_message_df = event_hub_message_df_factory(time_series_protobuf)

    parsed_time_series_point_stream = __parse_stream(event_hub_message_df)

    first = parsed_time_series_point_stream.first()
    assert first.series_meteringPointId == expected_metering_point_id


def test_parse_series_point_observationDateTime_from_stream(timeseries_protobuf_factory, event_hub_message_df_factory):
    "Test series point observationDateTime is parsed correctly from stream"
    expected_observation_date_time = datetime.datetime.strptime('2020-11-12T23:00:00.000Z', '%Y-%m-%dT%H:%M:%S.%fZ')
    time_series_protobuf = timeseries_protobuf_factory(observation_date_time=expected_observation_date_time)
    event_hub_message_df = event_hub_message_df_factory(time_series_protobuf)

    parsed_time_series_point_stream = __parse_stream(event_hub_message_df)

    first = parsed_time_series_point_stream.first()
    assert first.series_point_observationDateTime.isoformat() + "Z" == expected_observation_date_time.isoformat() + "Z"


@pytest.mark.parametrize(
    "expected_quantity",
    [
        pytest.param(Decimal('0.337'), id="Zero units and non zero nanos"),
        pytest.param(Decimal('2.000'), id="Non-zero units and zero nanos"),
        pytest.param(Decimal('3.100'), id="Non zero units and non zero nanos"),
    ],
)
def test_parse_series_point_quantity_from_stream(expected_quantity, timeseries_protobuf_factory, event_hub_message_df_factory):
    "Test series point quantity is parsed correctly from stream"
    time_series_protobuf = timeseries_protobuf_factory(quantity=expected_quantity)
    event_hub_message_df = event_hub_message_df_factory(time_series_protobuf)

    parsed_time_series_point_stream = __parse_stream(event_hub_message_df)

    first = parsed_time_series_point_stream.first()
    assert first.series_point_quantity == expected_quantity


def test_parse_series_point_all_hardcoded_test_values(timeseries_protobuf_factory, event_hub_message_df_factory):
    "Test series hardcoded test values are parsed correctly from stream"
    # Arrange
    document_id = "documentid1"
    document_requestDateTime = datetime.datetime.strptime("2020-12-15T13:15:11.000Z", '%Y-%m-%dT%H:%M:%S.%fZ')
    document_createdDateTime = datetime.datetime.strptime("2020-12-01T13:16:29.000Z", '%Y-%m-%dT%H:%M:%S.%fZ')
    document_sender_id = "8100000000030"
    document_sender_businessProcessRole = BusinessProcessRole.metered_data_responsible.value
    document_businessReasonCode = BusinessReasonCode.periodic_flex_metering.value
    series_id = "seriesid1"
    series_meteringPointType = MeteringPointType.consumption.value
    series_settlementMethod = SettlementMethod.flex.value
    series_registrationDateTime = datetime.datetime.strptime('2021-06-17T11:41:28.000Z', '%Y-%m-%dT%H:%M:%S.%fZ')
    series_product = Product.energy_active.value
    series_unit = MeasureUnit.kilo_watt_hour.value
    series_resolution = ResolutionDuration.hour.value
    series_startDateTime = datetime.datetime.strptime('2020-11-20T23:00:00.000Z', '%Y-%m-%dT%H:%M:%S.%fZ')
    series_endDateTime = datetime.datetime.strptime('2020-11-21T23:00:00.000Z', '%Y-%m-%dT%H:%M:%S.%fZ')
    series_point_position = 1
    series_point_quality = QuantityQuality.measured.value
    correlationId = "correlationid1"

    time_series_protobuf = timeseries_protobuf_factory()
    event_hub_message_df = event_hub_message_df_factory(time_series_protobuf)

    # Act
    parsed_time_series_point_stream = __parse_stream(event_hub_message_df)

    # Assert
    first = parsed_time_series_point_stream.first()
    assert first.document_id == document_id
    assert first.document_requestDateTime.isoformat() + "Z" == document_requestDateTime.isoformat() + "Z"
    assert first.document_createdDateTime.isoformat() + "Z" == document_createdDateTime.isoformat() + "Z"
    assert first.document_sender_id == document_sender_id
    assert first.document_sender_businessProcessRole == document_sender_businessProcessRole
    assert first.document_businessReasonCode == document_businessReasonCode
    assert first.series_id == series_id
    assert first.series_meteringPointType == series_meteringPointType
    assert first.series_settlementMethod == series_settlementMethod
    assert first.series_registrationDateTime.isoformat() + "Z" == series_registrationDateTime.isoformat() + "Z"
    assert first.series_product == series_product
    assert first.series_unit == series_unit
    assert first.series_resolution == series_resolution
    assert first.series_startDateTime.isoformat() + "Z" == series_startDateTime.isoformat() + "Z"
    assert first.series_endDateTime.isoformat() + "Z" == series_endDateTime.isoformat() + "Z"
    assert first.series_point_position == series_point_position
    assert first.series_point_quality == series_point_quality
    assert first.correlationId == correlationId


def test_get_flattened_time_series_points(parsed_data):
    "Test __get_flattened_time_series_points"
    flattened_time_series_points = __get_flattened_time_series_points(parsed_data)

    first = flattened_time_series_points.first()
    assert first.series_id == "seriesid1"
    assert "correlation_id" in flattened_time_series_points.columns
    assert "document_id" in flattened_time_series_points.columns
    assert "series_id" in flattened_time_series_points.columns


@pytest.mark.parametrize(
    "units,nanos,expected",
    [
        pytest.param(12345, 678900000, Decimal('12345.6789'), id="Non zero units and nanos"),
        pytest.param(0, 678900000, Decimal('0.6789'), id="Zero units and non zero nanos"),
        pytest.param(12345, 0, Decimal('12345.0'), id="Non zero units and zero nanos"),
        pytest.param(None, None, Decimal('0'), id="None units and None nanos"),
        pytest.param(None, 678900000, Decimal('0.6789'), id="None units and non zero nanos"),
        pytest.param(12345, None, Decimal('12345'), id="Non zero units and None nanos"),
    ],
)
def test_to_quantity(units, nanos, expected):
    "Test to_quantity"

    return_value = __to_quantity(units, nanos)

    assert return_value == expected
