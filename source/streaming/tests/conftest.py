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
from pyspark.sql.types import StructType, StructField, StringType, ArrayType, DecimalType, IntegerType, TimestampType, BooleanType
import pandas as pd
from decimal import Decimal
from datetime import datetime
import time
import uuid

from geh_stream.codelists import MeasureUnit, MeteringPointType, QuantityQuality, Product, SettlementMethod
from geh_stream.streaming_utils.streamhandlers import Enricher
from geh_stream.schemas import SchemaNames, SchemaFactory, quantity_type
from geh_stream.dataframelib import flatten_df
from geh_stream.streaming_utils.streamhandlers.denormalization import denormalize_parsed_data
from geh_stream.streaming_utils.input_source_readers.protobuf_message_parser import ProtobufMessageParser
from geh_stream.contracts.time_series_pb2 import TimeSeriesCommand, Document, Series, Point, DecimalValue


# Create Spark Conf/Session
@pytest.fixture(scope="session")
def spark():
    spark_conf = SparkConf(loadDefaults=True) \
        .set("spark.sql.session.timeZone", "UTC")
    return SparkSession \
        .builder \
        .config(conf=spark_conf) \
        .getOrCreate()


# Create timestamps used in DataFrames
time_now = time.time()
time_future = time.time() + 1000
time_past = time.time() - 1000
timestamp_now = pd.Timestamp(time_now, unit='s')
timestamp_future = pd.Timestamp(time_future, unit='s')
timestamp_past = pd.Timestamp(time_past, unit='s')


# Create schema of parsed data (timeseries data) and master data
@pytest.fixture(scope="session")
def parsed_schema():
    return SchemaFactory.get_instance(SchemaNames.Parsed)


@pytest.fixture(scope="session")
def master_schema():
    return SchemaFactory.get_instance(SchemaNames.Master)


@pytest.fixture(scope="session")
def parsed_data(valid_timeseries_protobuf_factory, event_hub_message_df_factory):
    "Parse data"
    time_series_protobuf = valid_timeseries_protobuf_factory(0, 0)
    event_hub_message_df = event_hub_message_df_factory(time_series_protobuf)
    message_schema: StructType = SchemaFactory.get_instance(SchemaNames.MessageBody)

    return ProtobufMessageParser.parse(event_hub_message_df, message_schema)  # TODO: Schema is unused


@pytest.fixture(scope="session")
def valid_timeseries_protobuf_factory():
    "Valid timeseries protobuf factory"

    def valid_timeseries_protobuf(units, nanos):
        "Create valid timeseries protobuf object"

        return __create_valid_timeseries_protobuf(units, nanos)

    return valid_timeseries_protobuf


@pytest.fixture(scope="session")
def invalid_timeseries_protobuf_factory():
    "Invalid timeseries protobuf factory"

    def invalid_timeseries_protobuf(units, nanos):
        "Create invalid timeseries protobuf object"

        time_series_protobuf = __create_valid_timeseries_protobuf(units, nanos)
        time_series_protobuf.series.metering_point_id = "non-existing metering point id 123498hhkjwer8"

        return time_series_protobuf

    return invalid_timeseries_protobuf


def __create_valid_timeseries_protobuf(units, nanos):
    "Create valid timeseries protobuf object"

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
    series.metering_point_id = "571313180000000005"
    series.metering_point_type = 1
    series.settlement_method = 3
    series.registration_date_time.FromJsonString("2021-06-17T11:41:28.8457326Z")
    series.product = 5
    series.unit = 1
    series.resolution = 2
    series.start_date_time.FromJsonString("2020-11-20T23:00:00Z")
    series.end_date_time.FromJsonString("2020-11-21T23:00:00Z")

    point1 = Point()
    point1.position = 1
    point1.observation_date_time.FromJsonString("2020-11-12T23:00:00Z")
    point1.quantity.units = units
    point1.quantity.nanos = nanos
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


@pytest.fixture(scope="session")
def time_series_json_factory():
    def factory(metering_point_id="mepm",
                quantity=1.0,
                observation_time=timestamp_now):
        json_str = """
            {{
                "Document": {{
                    "Id": "c",
                    "RequestDateTime": "{7}",
                    "Type": 1,
                    "CreatedDateTime": "{0}",
                    "Sender": {{
                        "Id": "x",
                        "BusinessProcessRole": 4
                    }},
                    "Recipient": {{
                        "Id": "x",
                        "BusinessProcessRole": 3
                    }},
                    "BusinessReasonCode": 2
                }},
                "Series": {{
                    "Id": "g",
                    "MeteringPointId": "{4}",
                    "Product": 5,
                    "MeteringPointType": "{1}",
                    "SettlementMethod": {6},
                    "RegistrationDateTime": "{8}",
                    "Unit": 1,
                    "Resolution": 2,
                    "StartDateTime": "{0}",
                    "EndDateTime": "{0}",
                    "Points": [
                        {{
                            "Position": 1,
                            "ObservationDateTime": "{5}",
                            "Quantity": "{2}",
                            "Quality": {3}
                        }}
                    ]
                }},
                "CorrelationId": "a"
            }}
        """.format(timestamp_now.isoformat() + "Z",
                   MeteringPointType.consumption.value,
                   quantity,
                   QuantityQuality.measured.value,
                   metering_point_id,
                   observation_time,
                   SettlementMethod.flex.value,
                   timestamp_now,
                   timestamp_now)
        return json_str

    return factory


@pytest.fixture(scope="session")
def time_series_json(time_series_json_factory):
    return time_series_json_factory("mepm", 1.0)


@pytest.fixture(scope="session")
def parsed_data_factory(spark, parsed_schema, time_series_json_factory):
    def factory(arg):
        """
        Accepts either a dictionary in which case a single row is created,
        or accepts a list of dictionaries in which case a set of rows are created.
        """
        if not isinstance(arg, list):
            arg = [arg]

        json_strs = []
        for dic in arg:
            json_strs.append(time_series_json_factory(**dic))
        json_array_str = "[{0}]".format(", ".join(json_strs))
        json_rdd = spark.sparkContext.parallelize([json_array_str])
        parsed_data = spark.read.json(json_rdd,
                                      schema=None,
                                      dateFormat="yyyy-MM-dd'T'HH:mm:ss.SSSSSSS'Z'")
        return parsed_data

    return factory


@pytest.fixture(scope="session")
def enriched_data_factory(parsed_data_factory, master_data_factory):
    def creator(metering_point_id="mepm",
                quantity=1.0,
                metering_point_type=MeteringPointType.consumption.value,
                settlement_method=SettlementMethod.flex.value,
                do_fail_enrichment=False):
        parsed_data = parsed_data_factory(dict(metering_point_id=metering_point_id, quantity=quantity))
        denormalized_parsed_data = denormalize_parsed_data(parsed_data)

        # Should join find a matching master data record or not?
        # If so use a non matching metering point id for the master data record.
        if do_fail_enrichment:
            non_matching_metering_point_id = str(uuid.uuid4())
            metering_point_id = non_matching_metering_point_id

        master_data = master_data_factory(dict(metering_point_id=metering_point_id,
                                               metering_point_type=metering_point_type,
                                               settlement_method=settlement_method))
        return Enricher.enrich(denormalized_parsed_data, master_data)
    return creator


@pytest.fixture(scope="session")
def enriched_data(enriched_data_factory):
    return enriched_data_factory()


@pytest.fixture(scope="session")
def non_enriched_data(enriched_data_factory):
    """Simulate data with no master data for market evaluation point."""
    return enriched_data_factory(do_fail_enrichment=True)


date_time_formatting_string = "%Y-%m-%dT%H:%M:%S%z"
default_obs_time = datetime.strptime("2020-01-01T00:00:00+0000", date_time_formatting_string)


# TODO: What is this? Should probably be either removed or refactored (or at least updated).
@pytest.fixture(scope="module")
def valid_atomic_value_schema():
    """
    Valid atomic data point schema to send
    """
    return StructType([
        StructField("IsTimeSeriesValid", BooleanType(), False),
        StructField("CorrelationId", StringType(), False),
        StructField("Series_MeteringPointId", StringType(), False),
        StructField("Series_Product", StringType(), False),
        StructField("Series_Unit", StringType(), False),
        StructField("Series_MeteringPointType", IntegerType(), False),
        StructField("Series_SettlementMethod", IntegerType(), False),
        StructField("Document_BusinessReasonCode", StringType(), False),
        StructField("Document_Recipient_BusinessProcessRole", StringType(), False),
        StructField("Series_Point_Quantity", SchemaFactory.quantity_type, False),
        StructField("Series_Point_Quality", IntegerType(), False),
        StructField("Series_Point_ObservationDateTime", TimestampType(), False),
        StructField("Document_CreatedDateTime", TimestampType(), False),
        StructField("EventHubEnqueueTime", TimestampType(), False)
    ])


# TODO: What is this? Should probably be either removed or refactored.
@pytest.fixture(scope="module")
def valid_atomic_values_for_actors_sample_df(spark, valid_atomic_value_schema):
    structureData = [
        (True, "10024", "3456", "15min", "product1", "m", "pointtype", "smet", "pr_type", "actor_role", "SC_KIND", ["r1", "r2", "r3"], Decimal(20048), "", default_obs_time, default_obs_time, default_obs_time)
    ]
    df2 = spark.createDataFrame(data=structureData, schema=valid_atomic_value_schema)
    return df2


@pytest.fixture(scope="module")
def invalid_time_series_schema():
    """
    Invalid time series schema to send
    """
    return StructType([
        StructField("Series_Id", StringType(), False),
        StructField("IsTimeSeriesValid", BooleanType(), False),
        StructField("CorrelationId", StringType(), False),
        StructField("Document_Sender_Id", StringType(), False),
        StructField("Document_Sender_BusinessProcessRole", StringType(), False),
        StructField("Document_Id", StringType(), False),
        StructField("Document_BusinessReasonCode", StringType(), False),
        StructField("VR-245-1-Is-Valid", BooleanType(), False),
        StructField("VR-250-Is-Valid", BooleanType(), False),
        StructField("VR-251-Is-Valid", BooleanType(), False),
        StructField("VR-611-Is-Valid", BooleanType(), False),
        StructField("VR-612-Is-Valid", BooleanType(), False)
    ])


# TODO: What is this? Should probably be either removed or refactored.
@pytest.fixture(scope="module")
def invalid_atomic_values_for_actors_sample_df(spark, invalid_time_series_schema):
    structureData = [
        ("tseries_id", False, "10024", "420901", "actor_role", "12345", "ptype", True, False, True, True, True)
    ]
    df2 = spark.createDataFrame(data=structureData, schema=invalid_time_series_schema)
    return df2


# TODO: Seems brittle
@pytest.fixture(scope="module")
def validation_results_schema():
    """
    Validation subset of columns
    """
    return StructType([
        StructField("VR-245-1-Is-Valid", BooleanType(), False),
        StructField("VR-250-Is-Valid", BooleanType(), False),
        StructField("VR-251-Is-Valid", BooleanType(), False),
        StructField("VR-611-Is-Valid", BooleanType(), False),
        StructField("VR-612-Is-Valid", BooleanType(), False)
    ])


# TODO: What is this? Should probably be either removed or refactored.
@pytest.fixture(scope="module")
def validation_results_values_for_actors_sample_df(spark, validation_results_schema):
    structureData = [
        (True, True, True, True, True),
        (False, True, True, True, True),
        (True, False, True, True, True),
        (True, True, False, True, True),
        (True, True, True, False, True),
        (True, True, True, True, False),
        (False, False, False, False, False)
    ]
    df2 = spark.createDataFrame(data=structureData, schema=validation_results_schema)
    return df2
