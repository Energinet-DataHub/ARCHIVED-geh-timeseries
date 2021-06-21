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
from pyspark.sql import DataFrame, Window
from pyspark.sql.functions import year, month, dayofmonth, to_json, \
    struct, col, from_json, coalesce, lit
import pyspark.sql.functions as F

from geh_stream.monitoring import MonitoredStopwatch
import geh_stream.dataframelib as D


def add_time_series_validation_status_column(batch_df: DataFrame):
    return batch_df.withColumn("IsTimeSeriesValid", F.min(col("IsTimeSeriesPointValid")).over(Window.partitionBy("series_id")))


def store_points_of_valid_time_series(batch_df: DataFrame, output_delta_lake_path, watch: MonitoredStopwatch):
    timer = watch.start_sub_timer(store_points_of_valid_time_series.__name__)
    batch_df \
        .filter(col("IsTimeSeriesValid") == lit(True)) \
        .select(col("series_meteringPointId"),
                col("series_point_observationTime").alias("time"),
                col("series_point_quantity").alias("quantity"),
                col("correlationId"),
                col("document_id"),
                col("document_createdDateTime").alias("CreatedDateTime"),
                col("document_sender_id").alias("sender_id"),
                col("document_ProcessType").alias("ProcessType"),
                col("document_sender_Type").alias("sender_Type"),
                col("series_id"),
                col("MktActivityRecord_Status"),
                col("series_meteringPointType"),
                col("series_point_quality").alias("quality"),
                col("MeterReadingPeriodicity"),
                col("MeteringMethod"),
                col("MeteringGridArea_Domain_mRID"),
                col("ConnectionState"),
                col("EnergySupplier_MarketParticipant_mRID"),
                col("BalanceResponsibleParty_MarketParticipant_mRID"),
                col("InMeteringGridArea_Domain_mRID"),
                col("OutMeteringGridArea_Domain_mRID"),
                col("Parent_Domain_mRID"),
                col("document_MarketServiceCategory_Kind").alias("ServiceCategory_Kind"),
                col("series_settlementMethod"),
                col("QuantityMeasurementUnit_Name"),
                col("series_product"),

                year("series_point_observationTime").alias("year"),
                month("series_point_observationTime").alias("month"),
                dayofmonth("series_point_observationTime").alias("day")) \
        .repartition("year", "month", "day") \
        .write \
        .partitionBy("year", "month", "day") \
        .format("delta") \
        .mode("append") \
        .save(output_delta_lake_path)
    timer.stop_timer()
