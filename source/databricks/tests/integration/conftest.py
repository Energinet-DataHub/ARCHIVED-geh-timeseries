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
import subprocess

from pyspark import SparkConf
from pyspark.sql import SparkSession


@pytest.fixture()
def spark():
    # spark.hadoop.fs.* for Azurite storage
    spark_conf = SparkConf(loadDefaults=True) \
        .set("spark.sql.session.timeZone", "UTC")
        #.set("spark.hadoop.fs.defaultFS", "wasb://container@azurite") \
        #.set("spark.hadoop.fs.azure.storage.emulator.account.name", "azurite")
    return SparkSession \
        .builder \
        .config(conf=spark_conf) \
        .config("spark.sql.streaming.schemaInference", True) \
        .getOrCreate()


@pytest.fixture(scope="session")
def azurite():
    """Fixture for starting Azurite blob service"""
    azurite_process = subprocess.Popen(args=["azurite-blob", "-l", ".azurite-files"], stdout=subprocess.PIPE)

    # Terminate Azurite service at end of test session
    yield
    azurite_process.terminate()
