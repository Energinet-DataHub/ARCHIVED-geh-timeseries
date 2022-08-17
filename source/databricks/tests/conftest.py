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
from pyspark.sql import SparkSession
from pyspark.sql.types import StringType
from datetime import datetime


# Create Spark Conf/Session.
@pytest.fixture(scope="session")
def spark():
    spark_conf = SparkConf(loadDefaults=True).set("spark.sql.session.timeZone", "UTC")

    return SparkSession.builder.config(conf=spark_conf).getOrCreate()


@pytest.fixture(scope="session")
def timestamp_factory():
    "Creates timestamp from utc string in correct format yyyy-mm-ddThh:mm:ss.nnnZ"

    def factory(date_time_string: str) -> datetime:
        date_time_formatting_string = "%Y-%m-%dT%H:%M:%S.%fZ"
        if date_time_string is None:
            return None
        return datetime.strptime(date_time_string, date_time_formatting_string)

    return factory
