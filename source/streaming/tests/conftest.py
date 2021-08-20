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
"""
By having a conftest.py in this directory, we are able to add all packages
defined in the geh_stream directory in our tests.
"""

import pytest
from pyspark import SparkConf
from pyspark.sql import SparkSession, DataFrame
from pyspark.sql.functions import col, lit, to_timestamp, explode
from pyspark.sql.types import StructType, StructField, StringType, ArrayType, \
    DecimalType, IntegerType, TimestampType, BooleanType, BinaryType, LongType
import pandas as pd
from decimal import Decimal
from datetime import datetime
import time
import uuid

from geh_stream.codelists import MeasureUnit, MeteringPointType, QuantityQuality, Product, SettlementMethod
from geh_stream.streaming_utils.streamhandlers import Enricher
from geh_stream.schemas import SchemaNames, SchemaFactory, quantity_type
from geh_stream.dataframelib import flatten_df
from geh_stream.streaming_utils.input_source_readers.protobuf_message_parser import ProtobufMessageParser
from geh_stream.contracts.time_series_pb2 import TimeSeriesCommand, Document, Series, Point, DecimalValue
from geh_stream.streaming_utils.input_source_readers.time_series_reader import __parse_stream


# Create Spark Conf/Session
@pytest.fixture(scope="session")
def spark():
    spark_conf = SparkConf(loadDefaults=True) \
        .set("spark.sql.session.timeZone", "UTC")
    return SparkSession \
        .builder \
        .config(conf=spark_conf) \
        .getOrCreate()


@pytest.fixture(scope="session")
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


@pytest.fixture(scope="session")
def event_hub_message_df_factory(event_hub_message_schema, spark):
    "Create event hub message factory"

    def event_hub_message_df(timeseries_protobuf):
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

    return event_hub_message_df


# Create timestamps used in DataFrames
time_now = time.time()
time_future = time.time() + 1000
time_past = time.time() - 1000
timestamp_now = pd.Timestamp(time_now, unit='s')
timestamp_future = pd.Timestamp(time_future, unit='s')
timestamp_past = pd.Timestamp(time_past, unit='s')


@pytest.fixture(scope="session")
def master_schema():
    return SchemaFactory.get_instance(SchemaNames.Master)


@pytest.fixture(scope="session")
def parsed_data(timeseries_protobuf_factory, event_hub_message_df_factory):
    "Parse data"
    time_series_protobuf = timeseries_protobuf_factory()
    event_hub_message_df = event_hub_message_df_factory(time_series_protobuf)

    return ProtobufMessageParser.parse(event_hub_message_df)


@pytest.fixture(scope="session")
def timeseries_protobuf_factory():
    "Timeseries protobuf factory"

    def timeseries_protobuf(**args):
        "Create timeseries protobuf object"

        return __create_timeseries_protobuf(args)

    return timeseries_protobuf


# @pytest.fixture(scope="session")
# def intimeseries_protobuf_factory():
#     "Invalid timeseries protobuf factory"

#     def invalid_timeseries_protobuf(units, nanos):
#         "Create invalid timeseries protobuf object"

#         time_series_protobuf = __create_valid_timeseries_protobuf(units, nanos)
#         time_series_protobuf.series.metering_point_id = "non-existing metering point id 123498hhkjwer8"

#         return time_series_protobuf

#     return invalid_timeseries_protobuf


def __create_timeseries_protobuf(args):
    "Create timeseries protobuf object"

    metering_point_id = __get_value_if_exits(args, "metering_point_id", "mepm")
    quantity = __get_value_if_exits(args, "quantity", Decimal('1.0'))
    observation_date_time = __get_value_if_exits(args, "observation_date_time", timestamp_now)

    # metering_point_id = args["metering_point_id"] or "mepm"
    # quantity = args["quantity"] or Decimal('1.0')
    # observation_time = args["observation_time"] or timestamp_now

    timeseries = TimeSeriesCommand()
    timeseries.correlation_id = "correlationid1"

    document = Document()
    document.id = "documentid1"
    document.request_date_time.FromJsonString("2020-12-15T13:15:11.8349163Z")
    document.created_date_time.FromJsonString("2020-12-01T13:16:29.33Z")
    document.sender.id = "8100000000030"
    document.sender.business_process_role = 4
    document.business_reason_code = 4
    timeseries.document.CopyFrom(document)

    series = Series()
    series.id = "seriesid1"
    series.metering_point_id = metering_point_id
    series.metering_point_type = MeteringPointType.consumption.value
    series.settlement_method = SettlementMethod.flex.value
    series.registration_date_time.FromJsonString("2021-06-17T11:41:28.8457326Z")
    series.product = 5
    series.unit = 1
    series.resolution = 2
    series.start_date_time.FromJsonString("2020-11-20T23:00:00Z")
    series.end_date_time.FromJsonString("2020-11-21T23:00:00Z")

    point1 = Point()
    point1.position = 1
    point1.observation_date_time.FromDatetime(observation_date_time)
    point1.quantity.units = int(quantity)
    point1.quantity.nanos = int(quantity % 1 * 10**9)
    point1.quality = 1

    # point2 = PointContract()
    # point2.position = 2
    # point2.observation_date_time.FromJsonString("2020-11-13T00:00:00Z")
    # point2.quantity.units = 2
    # point2.quantity.nanos = 000000000
    # point2.quality = 1

    series.points.append(point1)
    # series.points.append(point2)

    timeseries.series.CopyFrom(series)

    return timeseries


def __get_value_if_exits(args, key, default):
    return args[key] if args.get(key) is not None else default

# Create parsed data and master data Dataframes
@pytest.fixture(scope="session")
def master_data_factory(spark, master_schema):

    def __create_pandas(metering_point_id="mepm",
                        valid_from=timestamp_past,
                        valid_to=timestamp_future,
                        metering_point_type=MeteringPointType.consumption.value,
                        settlement_method=SettlementMethod.flex.value):
        return pd.DataFrame({
            'meteringPointId': [metering_point_id],
            "validFrom": [valid_from],
            "validTo": [valid_to],
            "meteringPointType": [metering_point_type],
            "settlementMethod": [settlement_method]})

    def factory(arg):
        if not isinstance(arg, list):
            arg = [arg]

        pandas_dfs = []
        for dic in arg:
            pandas_dfs.append(__create_pandas(**dic))

        return spark.createDataFrame(pd.concat(pandas_dfs), schema=master_schema)
    return factory


# @pytest.fixture(scope="session")
# def time_series_json_factory():
#     def factory(metering_point_id="mepm",
#                 quantity=1.0,
#                 observation_time=timestamp_now):
#         json_str = """
#             {{
#                 "Document": {{
#                     "Id": "c",
#                     "RequestDateTime": "{7}",
#                     "Type": 1,
#                     "CreatedDateTime": "{0}",
#                     "Sender": {{
#                         "Id": "x",
#                         "BusinessProcessRole": 4
#                     }},
#                     "Recipient": {{
#                         "Id": "x",
#                         "BusinessProcessRole": 3
#                     }},
#                     "BusinessReasonCode": 2
#                 }},
#                 "Series": {{
#                     "Id": "g",
#                     "MeteringPointId": "{4}",
#                     "Product": 5,
#                     "MeteringPointType": "{1}",
#                     "SettlementMethod": {6},
#                     "RegistrationDateTime": "{8}",
#                     "Unit": 1,
#                     "Resolution": 2,
#                     "StartDateTime": "{0}",
#                     "EndDateTime": "{0}",
#                     "Points": [
#                         {{
#                             "Position": 1,
#                             "ObservationDateTime": "{5}",
#                             "Quantity": "{2}",
#                             "Quality": {3}
#                         }}
#                     ]
#                 }},
#                 "CorrelationId": "a"
#             }}
#         """.format(timestamp_now.isoformat() + "Z",
#                    MeteringPointType.consumption.value,
#                    quantity,
#                    QuantityQuality.measured.value,
#                    metering_point_id,
#                    observation_time,
#                    SettlementMethod.flex.value,
#                    timestamp_now,
#                    timestamp_now)
#         return json_str

#     return factory


# @pytest.fixture(scope="session")
# def time_series_json(time_series_json_factory):
#     return time_series_json_factory("mepm", 1.0)


# @pytest.fixture(scope="session")
# def parsed_data_factory(spark, time_series_json_factory):
#     def factory(arg):
#         """
#         Accepts either a dictionary in which case a single row is created,
#         or accepts a list of dictionaries in which case a set of rows are created.
#         """
#         if not isinstance(arg, list):
#             arg = [arg]

#         json_strs = []
#         for dic in arg:
#             json_strs.append(time_series_json_factory(**dic))
#         json_array_str = "[{0}]".format(", ".join(json_strs))
#         json_rdd = spark.sparkContext.parallelize([json_array_str])
#         parsed_data = spark.read.json(json_rdd,
#                                       schema=None,
#                                       dateFormat="yyyy-MM-dd'T'HH:mm:ss.SSSSSSS'Z'")
#         return parsed_data

#     return factory


# @pytest.fixture(scope="session")
# def enriched_data_factory(parsed_data_factory, master_data_factory):
#     def creator(metering_point_id="mepm",
#                 quantity=1.0,
#                 metering_point_type=MeteringPointType.consumption.value,
#                 settlement_method=SettlementMethod.flex.value,
#                 do_fail_enrichment=False):
#         parsed_data = parsed_data_factory(dict(metering_point_id=metering_point_id, quantity=quantity))

#         # Should join find a matching master data record or not?
#         # If so use a non matching metering point id for the master data record.
#         if do_fail_enrichment:
#             non_matching_metering_point_id = str(uuid.uuid4())
#             metering_point_id = non_matching_metering_point_id

#         master_data = master_data_factory(dict(metering_point_id=metering_point_id,
#                                                metering_point_type=metering_point_type,
#                                                settlement_method=settlement_method))
#         return Enricher.enrich(parsed_data, master_data)
#     return creator


@pytest.fixture(scope="session")
def parsed_data_factory(spark, timeseries_protobuf_factory, event_hub_message_df_factory):
    def factory(
            metering_point_id="mepm",
            quantity=Decimal('1.0'),
            observation_date_time=timestamp_now):

        # args = dict(metering_point_id=metering_point_id, quantity=quantity, observation_date_time=observation_date_time)
        # args = dict([(i, locals()[i]) for i in ('metering_point_id', 'quantity', 'observation_date_time')])

        time_series_protobuf = timeseries_protobuf_factory(metering_point_id=metering_point_id, quantity=quantity, observation_date_time=observation_date_time)
        event_hub_message_df = event_hub_message_df_factory(time_series_protobuf)

        parsed_data = __parse_stream(event_hub_message_df)

        return parsed_data
    return factory


@pytest.fixture(scope="session")
def enriched_data_factory(timeseries_protobuf_factory, event_hub_message_df_factory, master_data_factory):
    def creator(metering_point_id="mepm",
                quantity=1.0,
                metering_point_type=MeteringPointType.consumption.value,
                settlement_method=SettlementMethod.flex.value,
                do_fail_enrichment=False):
        time_series_protobuf = timeseries_protobuf_factory(metering_point_id=metering_point_id, quantity=quantity)

        event_hub_message_df = event_hub_message_df_factory(time_series_protobuf)

        parsed_data = __parse_stream(event_hub_message_df)

        # Should join find a matching master data record or not?
        # If so use a non matching metering point id for the master data record.
        if do_fail_enrichment:
            non_matching_metering_point_id = str(uuid.uuid4())
            metering_point_id = non_matching_metering_point_id

        master_data = master_data_factory(dict(metering_point_id=metering_point_id,
                                               metering_point_type=metering_point_type,
                                               settlement_method=settlement_method))
        return Enricher.enrich(parsed_data, master_data)
    return creator


@pytest.fixture(scope="session")
def enriched_data(enriched_data_factory):
    return enriched_data_factory()


@pytest.fixture(scope="session")
def non_enriched_data(enriched_data_factory):
    """Simulate data with no master data for market evaluation point."""
    return enriched_data_factory(do_fail_enrichment=True)
