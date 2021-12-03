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
Ingestion Stream for Time Series of Market Evaluation Point Observations
"""

# TODO: consider using pyspark-stubs=3.0.0 and mypy

# %% Job Parameters
import configargparse

p = configargparse.ArgParser(prog='enrichment_and_validation.py',
                             description='Green Energy Hub Streaming',
                             default_config_files=['configuration/run_args_enrichment_and_validation.conf'],
                             formatter_class=configargparse.ArgumentDefaultsHelpFormatter)

# Storage account
p.add('--storage-account-name', type=str, required=True,
      help='Azure Storage account name (used for data output and checkpointing)')
p.add('--storage-account-key', type=str, required=True,
      help='Azure Storage key', env_var='GEH_STREAMING_STORAGE_KEY')
p.add('--storage-container-name', type=str, required=False, default='data',
      help='Azure Storage container name')

# Master data (in storage account)
p.add('--master-data-path', type=str, required=False, default="master-data/master-data.csv",
      help='Path to master data storage location (csv) relative to container''s root')

# Stored valid time series points (in storage account)
p.add('--output-path', type=str, required=False, default="delta/meter-data/",
      help='Path to stream output storage location (deltalake) relative to container''s root')

# Streamed data input source
p.add('--input-eh-connection-string', type=str, required=True,
      help='Input Event Hub connection string', env_var='GEH_STREAMING_INPUT_EH_CONNECTION_STRING')
p.add('--max-events-per-trigger', type=int, required=False, default=10000,
      help='Metering points to read per trigger interval')
p.add('--trigger-interval', type=str, required=False, default='1 second',
      help='Trigger interval to generate streaming batches (format: N seconds)')
p.add('--streaming-checkpoint-path', type=str, required=False, default="checkpoints/streaming",
      help='Path to checkpoint folder for streaming')

# Telemetry
p.add('--telemetry-instrumentation-key', type=str, required=True,
      help='Instrumentation key used for telemetry')

args, unknown_args = p.parse_known_args()

if unknown_args:
    print("Unknown args:")
    _ = [print(arg) for arg in unknown_args]

# %% Create or get Spark session
from pyspark import SparkConf
from pyspark.sql import SparkSession
from datetime import datetime, timezone

print(datetime.now(timezone.utc))

spark_conf = SparkConf(loadDefaults=True) \
    .set('fs.azure.account.key.{0}.dfs.core.windows.net'.format(args.storage_account_name),
         args.storage_account_key)

spark = SparkSession\
    .builder\
    .config(conf=spark_conf)\
    .getOrCreate()

sc = spark.sparkContext
print("Spark Configuration:")
_ = [print(k + '=' + v) for k, v in sc.getConf().getAll()]

# %% Get base storage path
BASE_STORAGE_PATH = "abfss://{0}@{1}.dfs.core.windows.net/".format(
    args.storage_container_name, args.storage_account_name
)

print("Base storage url:", BASE_STORAGE_PATH)

# %% Read master data from input source
from geh_stream.streaming_utils.input_source_readers import read_master_data

master_data_storage_path = BASE_STORAGE_PATH + args.master_data_path
master_data_df = read_master_data(spark, master_data_storage_path)

# %% Read raw time series streaming data from input source
import json

from geh_stream.streaming_utils.input_source_readers import get_time_series_point_stream

input_eh_starting_position = {
    "offset": "-1",         # starting from beginning of stream
    "seqNo": -1,            # not in use
    "enqueuedTime": None,   # not in use
    "isInclusive": True
}
input_eh_connection_string = args.input_eh_connection_string
input_eh_conf = {
    # Version 2.3.15 and up requires encryption
    'eventhubs.connectionString': \
    sc._jvm.org.apache.spark.eventhubs.EventHubsUtils.encrypt(input_eh_connection_string),
    'eventhubs.startingPosition': json.dumps(input_eh_starting_position),
    'maxEventsPerTrigger': args.max_events_per_trigger
}

print("Input event hub config:", input_eh_conf)

time_series_point_stream = get_time_series_point_stream(spark, input_eh_conf)

# %% Telemetry
from geh_stream.monitoring import Telemetry

telemetry_client = Telemetry.create_telemetry_client(args.telemetry_instrumentation_key)

# %% Process time series as points
from pyspark.sql import DataFrame
from pyspark.sql.types import StructType

from geh_stream.monitoring import MonitoredStopwatch
import geh_stream.batch_operations as batch_operations
from geh_stream.validation import Validator
from geh_stream.streaming_utils.streamhandlers.enricher import Enricher


def __process_data_frame(time_series_points_df: DataFrame, _: int):
    try:
        watch = MonitoredStopwatch.start_timer(telemetry_client, __process_data_frame.__name__)

        time_series_points_df = Enricher.enrich(time_series_points_df, master_data_df)
        time_series_points_df = Validator.add_validation_status_columns(time_series_points_df)

        # This validation cannot be done in the Validator due to the implementation.
        # It uses a Window, which can not be used in streaming without time.
        time_series_points_df = batch_operations.add_time_series_validation_status_column(time_series_points_df)

        # Cache the batch in order to avoid the risk of recalculation
        time_series_points_df = time_series_points_df.persist()

        # Make valid time series points available to aggregations (by storing in Delta lake)
        batch_operations.store_points_of_valid_time_series(time_series_points_df, output_delta_lake_path, watch)

        # Log invalid time series points
        batch_operations.log_invalid_time_series(time_series_points_df, telemetry_client)

        batch_count = batch_operations.get_rows_in_batch(time_series_points_df, watch)

        watch.stop_timer(batch_count)

        # Collect serializable data about the batch. In order to be able to use it on individual worker nodes
        # when sending telemetry per correlation ID from worker nodes.
        batch_info = {
            "batch_dependency_id": watch.watch_id,
            "batch_row_count": batch_count,
            "batch_duration_ms": watch.duration_ms
        }

        batch_operations.track_batch_back_to_original_correlation_requests(time_series_points_df, batch_info, args.telemetry_instrumentation_key)

        time_series_points_df.unpersist()
        print("Batch details:", batch_info)

    except Exception as err:
        # Make sure the exception is not accidently tracked on the last used parent
        telemetry_client.context.operation.parent_id = None
        # We need to track and flush the exception so it is not lost in case the exception will stop execution
        telemetry_client.track_exception()
        telemetry_client.flush()
        # The exception needs to continue its journey as to not cause data loss
        raise err


# checkpointLocation is used to support failure (or intentional shut-down)
# recovery with a exactly-once semantic. See more on
# https://spark.apache.org/docs/latest/structured-streaming-programming-guide.html#fault-tolerance-semantics.
# The trigger determines how often a batch is created and processed.
output_delta_lake_path = BASE_STORAGE_PATH + args.output_path
checkpoint_path = BASE_STORAGE_PATH + args.streaming_checkpoint_path
out_stream = time_series_point_stream \
    .writeStream \
    .option("checkpointLocation", checkpoint_path) \
    .trigger(processingTime=args.trigger_interval) \
    .foreachBatch(__process_data_frame)

# %% Start streaming
from datetime import datetime
import sys

out_stream.start().awaitTermination()
