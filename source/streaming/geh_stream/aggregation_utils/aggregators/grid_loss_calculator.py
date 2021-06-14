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
from pyspark.sql.functions import col, when

grid_area = "MeteringGridArea_Domain_mRID"
time_window = "time_window"


# Function used to calculate grid loss (step 6)
def calculate_grid_loss(agg_net_exchange: DataFrame, agg_hourly_consumption: DataFrame, agg_flex_consumption: DataFrame, agg_production: DataFrame):
    agg_net_exchange_result = agg_net_exchange.selectExpr(grid_area, "result as net_exchange_result", "time_window")
    agg_hourly_consumption_result = agg_hourly_consumption \
        .selectExpr(grid_area, "sum_quantity as hourly_result", "time_window") \
        .groupBy(grid_area, "time_window") \
        .sum("hourly_result") \
        .withColumnRenamed("sum(hourly_result)", "hourly_result")
    agg_flex_consumption_result = agg_flex_consumption \
        .selectExpr(grid_area, "sum_quantity as flex_result", "time_window") \
        .groupBy(grid_area, "time_window") \
        .sum("flex_result") \
        .withColumnRenamed("sum(flex_result)", "flex_result")
    agg_production_result = agg_production \
        .selectExpr(grid_area, "sum_quantity as prod_result", "time_window") \
        .groupBy(grid_area, "time_window") \
        .sum("prod_result") \
        .withColumnRenamed("sum(prod_result)", "prod_result")

    result = agg_net_exchange_result \
        .join(agg_production_result, [grid_area, time_window]) \
        .join(agg_hourly_consumption_result.join(agg_flex_consumption_result, [grid_area, time_window]), [grid_area, time_window]) \
        .orderBy(grid_area, time_window)
    result = result.withColumn("grid_loss", result.net_exchange_result + result.prod_result - (result.hourly_result + result.flex_result))

    return result.select(grid_area, time_window, "grid_loss")


# Function to calculate system correction to be added (step 8)
def calculate_added_system_correction(df: DataFrame):
    result = df.withColumn("added_system_correction", when(col("grid_loss") < 0, (col("grid_loss")) * (-1)).otherwise(0))
    return result.select(grid_area, time_window, "added_system_correction")


# Function to calculate grid loss to be added (step 9)
def calculate_added_grid_loss(df: DataFrame):
    result = df.withColumn("added_grid_loss", when(col("grid_loss") > 0, col("grid_loss")).otherwise(0))
    return result.select(grid_area, time_window, "added_grid_loss")


# Function to calculate total consumption (step 21)
def calculate_total_consumption(agg_net_exchange: DataFrame, agg_production: DataFrame):
    grid_area = "MeteringGridArea_Domain_mRID"

    result_production = agg_production.selectExpr(grid_area, "time_window", "sum_quantity").groupBy(grid_area, "time_window").sum("sum_quantity")
    result_net_exchange = agg_net_exchange.selectExpr(grid_area, "time_window", "result").groupBy(grid_area, "time_window").sum("result")
    result = result_production.join(result_net_exchange, [grid_area, "time_window"]) \
        .withColumn("total_consumption", col("sum(result)") + col("sum(sum_quantity)")) \
        .orderBy(grid_area, "time_window")
    result = result.select(grid_area, "time_window", "total_consumption")

    return result
