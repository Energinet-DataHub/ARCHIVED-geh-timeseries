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
from pyspark.sql import DataFrame
from pyspark.sql.functions import col, window
from geh_stream.codelists import MarketEvaluationPointType, ConnectionState

grid_area = 'MeteringGridArea_Domain_mRID'
time_window = 'time_window'


# Function to aggregate hourly consumption per grid area, balance responsible party and energy supplier (step 2)
def aggregate_net_exchange(df: DataFrame):
    exchangeIn = df \
        .filter(col("MarketEvaluationPointType") == MarketEvaluationPointType.exchange.value) \
        .filter((col("ConnectionState") == ConnectionState.connected.value) | (col("ConnectionState") == ConnectionState.disconnected.value))
    exchangeIn = exchangeIn \
        .groupBy("InMeteringGridArea_Domain_mRID", window(col("Time"), "1 hour")) \
        .sum("Quantity") \
        .withColumnRenamed("sum(Quantity)", "in_sum") \
        .withColumnRenamed("window", time_window) \
        .withColumnRenamed("InMeteringGridArea_Domain_mRID", grid_area)
    exchangeOut = df \
        .filter(col("MarketEvaluationPointType") == MarketEvaluationPointType.exchange.value) \
        .filter((col("ConnectionState") == ConnectionState.connected.value) | (col("ConnectionState") == ConnectionState.disconnected.value))
    exchangeOut = exchangeOut \
        .groupBy("OutMeteringGridArea_Domain_mRID", window(col("Time"), "1 hour")) \
        .sum("Quantity") \
        .withColumnRenamed("sum(Quantity)", "out_sum") \
        .withColumnRenamed("window", time_window) \
        .withColumnRenamed("OutMeteringGridArea_Domain_mRID", grid_area)
    joined = exchangeIn \
        .join(exchangeOut,
              (exchangeIn.MeteringGridArea_Domain_mRID == exchangeOut.MeteringGridArea_Domain_mRID) & (exchangeIn.time_window == exchangeOut.time_window),
              how="outer") \
        .select(exchangeIn["*"], exchangeOut["out_sum"])
    resultDf = joined.withColumn(
        "result", joined["in_sum"] - joined["out_sum"])
    resultDf = resultDf.sort(grid_area, time_window)
    return resultDf
