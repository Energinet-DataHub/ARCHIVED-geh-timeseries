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

from pyspark import SparkConf
from pyspark.sql import DataFrame, SparkSession
from pyspark.sql.functions import col
from geh_stream.aggregation_utils.filters import time_period_filter
from datetime import datetime


class AggregationInitializer:

    def __init__(self, args, areas):
        self.args = args
        self.areas = areas

        # Parse the given date times
        date_time_formatting_string = "%Y-%m-%dT%H:%M:%S%z"
        self.end_date_time = datetime.strptime(args.end_date_time, date_time_formatting_string)
        self.beginning_date_time = datetime.strptime(args.beginning_date_time, date_time_formatting_string)

        # Set spark config with storage account names/keys and the session timezone so that datetimes are displayed consistently (in UTC)
        self.spark_conf = SparkConf(loadDefaults=True) \
            .set('fs.azure.account.key.{0}.dfs.core.windows.net'.format(args.input_storage_account_name), args.input_storage_account_key) \
            .set("spark.sql.session.timeZone", "UTC")

        self.spark = SparkSession \
            .builder\
            .config(conf=self.spark_conf)\
            .getOrCreate()

        # Uncomment to get some info on our spark context
        # sc = self.spark.sparkContext
        # print("Spark Configuration:")
        # _ = [print(k + '=' + v) for k, v in sc.getConf().getAll()]

        # Create input and output storage paths
        INPUT_STORAGE_PATH = "abfss://{0}@{1}.dfs.core.windows.net/{2}".format(
            self.args.input_storage_container_name, self.args.input_storage_account_name, self.args.input_path
        )

        print("Input storage url:", INPUT_STORAGE_PATH)

        # Read in time series data (delta doesn't support user specified schema)
        self.timeseries_df = self.spark \
            .read \
            .format("delta") \
            .load(INPUT_STORAGE_PATH)

        # Filter out time series data that is not in the specified time period
        self.valid_time_period_df = time_period_filter(self.timeseries_df, self.beginning_date_time, self.end_date_time)

        if areas:
            # Filter out time series data that do not belong to the specified grid areas
            self.valid_time_period_df = self.valid_time_period_df \
                .filter(col("MeteringGridArea_Domain_mRID").isin(areas))
