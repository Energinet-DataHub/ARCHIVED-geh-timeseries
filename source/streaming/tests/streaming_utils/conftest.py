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
from pyspark.sql.functions import to_timestamp

from geh_stream.protodf import schema_for, message_to_row
from geh_stream.schemas import SchemaFactory, SchemaNames


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


@pytest.fixture(scope="session")
def data_parsed_from_protobuf_schema():
    return SchemaFactory.get_instance(SchemaNames.ParsedProtobuf)


@pytest.fixture(scope="session")
def time_series_points_schema():
    return SchemaFactory.get_instance(SchemaNames.TimeSeriesPoints)
