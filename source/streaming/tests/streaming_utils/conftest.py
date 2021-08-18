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

import os
import pytest
import pandas as pd
import time
from pyspark.sql.types import BinaryType, LongType, StringType, StructType, TimestampType
from pyspark.sql.functions import to_timestamp

from geh_stream.protodf import schema_for, message_to_row
from geh_stream.schemas import SchemaFactory, SchemaNames


# Create timestamp used in DataFrames
time_now = time.time()
timestamp_now = pd.Timestamp(time_now, unit='s')


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


testdata_dir = os.path.dirname(os.path.realpath(__file__)) + "/testdata/"


@pytest.fixture(scope="session")
def master_data(spark):
    master_data_schema = SchemaFactory.get_instance(SchemaNames.Master)
    # dtype=str to prevent pandas from inferring types, which e.g. causes some strings to be parsed as floats and displayed in scientific notation
    master_data_pd = pd.read_csv(testdata_dir + "master-data.csv",
                                 sep=";",
                                 parse_dates=["validFrom", "validTo"],
                                 dtype=str)

    # Convert selected columns to int
    master_data_pd['settlementMethod'] = master_data_pd['settlementMethod'].astype('int')

    # Remove the meta column. The column is added in test csv file to let developers explain the data in the rows.
    master_data_pd.drop(columns=["meta"], inplace=True)

    master_data = spark.createDataFrame(master_data_pd, schema=master_data_schema)
    return master_data
