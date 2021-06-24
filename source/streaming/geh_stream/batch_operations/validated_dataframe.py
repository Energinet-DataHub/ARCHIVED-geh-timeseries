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
        .select(col("document_id"),
                col("document_requestDateTime"),
                col("document_type"),
                col("document_createdDateTime"),
                col("document_sender_id"),
                col("document_sender_businessProcessRole"),
                col("document_recipient_id"),
                col("document_recipient_businessProcessRole"),
                col("document_businessReasonCode"),
                col("series_id"),
                col("series_meteringPointId"),
                col("series_meteringPointType"),
                col("series_settlementMethod"),
                col("series_registrationDateTime"),
                col("series_product"),
                col("series_unit"),
                col("series_resolution"),
                col("series_startDateTime"),
                col("series_endDateTime"),
                col("series_point_position"),
                col("series_point_observationDateTime"),
                col("series_point_quantity"),
                col("series_point_quality"),
                col("transaction_mRID"),
                col("correlationId"),

                year("series_point_observationDateTime").alias("year"),
                month("series_point_observationDateTime").alias("month"),
                dayofmonth("series_point_observationDateTime").alias("day")) \
        .repartition("year", "month", "day") \
        .write \
        .partitionBy("year", "month", "day") \
        .format("delta") \
        .mode("append") \
        .save(output_delta_lake_path)
    timer.stop_timer()


def log_invalid_time_series(batched_time_series_points: DataFrame, telemetry_client):
    invalid_time_series_points = batched_time_series_points \
        .filter(col("IsTimeSeriesValid") == lit(False)) \
        .select(col("document_id"),
                col("document_requestDateTime"),
                col("document_type"),
                col("document_createdDateTime"),
                col("document_sender_id"),
                col("document_sender_businessProcessRole"),
                col("document_recipient_id"),
                col("document_recipient_businessProcessRole"),
                col("document_businessReasonCode"),
                col("series_id"),
                col("series_meteringPointId"),
                col("series_meteringPointType"),
                col("series_settlementMethod"),
                col("series_registrationDateTime"),
                col("series_product"),
                col("series_unit"),
                col("series_resolution"),
                col("series_startDateTime"),
                col("series_endDateTime"),
                col("series_point_position"),
                col("series_point_observationDateTime"),
                col("series_point_quantity"),
                col("series_point_quality"),
                col("transaction_mRID"),
                col("correlationId"),
                col("IsTimeSeriesValid"),
                col("VR-200-Is-Valid"),
                col("VR-245-1-Is-Valid"),
                col("VR-250-Is-Valid"),
                col("VR-251-Is-Valid"),
                col("VR-611-Is-Valid"),
                col("VR-612-Is-Valid"))
    invalid_time_series_points.show()
