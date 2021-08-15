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
from pyspark.sql import DataFrame
from pyspark.sql.types import BinaryType, LongType, StringType, StructType, TimestampType
from geh_stream.protodf import schema_for, message_to_row
from geh_stream.schemas import SchemaFactory, quantity_type, SchemaNames
from geh_stream.streaming_utils.input_source_readers.protobuf_message_parser import ProtobufMessageParser
from geh_stream.contracts.time_series_pb2 import \
    TimeSeriesCommandContract, DocumentContract, SeriesContract, PointContract, DecimalValueContract

# Create timestamp used in DataFrames
time_now = time.time()
timestamp_now = pd.Timestamp(time_now, unit='s')


def test_parse(parsed_data):
    "Test Parse data from protobuf messages"

    assert "correlation_id" in parsed_data.columns
    assert "document" in parsed_data.columns
    assert "series" in parsed_data.columns
    assert "points:" in parsed_data.schema.simpleString()

    metering_point_id = "meteringpointid1"
    first_series = parsed_data.first().series
    assert first_series.metering_point_id == metering_point_id


def test_parse_event_hub_message_returns_correct_schema(parsed_data):
    "Check that resulting DataFrame has expected schema"  # TODO: does this test make sense at all?
    assert str(parsed_data.schema) == str(SchemaFactory.get_instance(SchemaNames.Parsed))


@pytest.fixture(scope="class")
def parsed_data(event_hub_message_df):
    "Parse data"
    message_schema: StructType = SchemaFactory.get_instance(SchemaNames.MessageBody)
    return ProtobufMessageParser.parse(event_hub_message_df, message_schema)  # TODO: Schema is unused


@pytest.fixture(scope="class")
def timeseries_protobuf():
    "Create timeseries protobuf object"
    timeseries = TimeSeriesCommandContract()
    timeseries.correlation_id = "correlationid1"

    document = DocumentContract()
    document.id = "documentid1"

    timeseries.document.CopyFrom(document)

    series = SeriesContract()
    series.id = "seriesid1"
    series.metering_point_id = "meteringpointid1"

    point1 = PointContract()
    point1.position = 1

    point2 = PointContract()
    point2.position = 2

    series.points.append(point1)
    series.points.append(point2)

    timeseries.series.CopyFrom(series)

    return timeseries


@pytest.fixture(scope="class")
def event_hub_message_schema():
    "Schemas of Event Hub Message, nested json body message, and expected result Dataframe from parse function"
    return StructType() \
        .add("body", BinaryType(), False) \
        .add("partition", StringType(), False) \
        .add("offset", StringType(), False) \
        .add("sequenceNumber", LongType(), False) \
        .add("publisher", StringType(), False) \
        .add("partitionKey", StringType(), False) \
        .add("properties", StructType(), True) \
        .add("systemProperties", StructType(), True) \
        .add("enqueuedTime", TimestampType(), True)


@pytest.fixture(scope="class")
def event_hub_message_df(event_hub_message_schema, timeseries_protobuf, spark):
    "Create event hub message"
    # Create message body using the required fields
    binary_body_message = timeseries_protobuf.SerializeToString()

    # Create event hub message
    event_hub_message_pandas_df = pd.DataFrame({
        "body": [binary_body_message],
        "partition": ["1"],
        "offset": ["offset"],
        "sequenceNumber": [2],
        "publisher": ["publisher"],
        "partitionKey": ["partitionKey"],
        "properties": [None],
        "systemProperties": [None],
        "enqueuedTime": [timestamp_now]})

    return spark.createDataFrame(event_hub_message_pandas_df, event_hub_message_schema)
