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
p.add('--master-data-path', type=str, required=False, default="master-data/MasterData.csv",
      help='Path to master data storage location (csv) relative to container''s root')

# Stored valid time series points (in storage account)
p.add('--output-path', type=str, required=False, default="delta/meter-data/",
      help='Path to stream output storage location (deltalake) relative to container''s root')

# Streamed data input source
p.add('--input-eh-connection-string', type=str, required=True,
      help='Input Event Hub connection string', env_var='GEH_STREAMING_INPUT_EH_CONNECTION_STRING')
p.add('--max-events-per-trigger', type=int, required=False, default=10000,
      help='Metering points to read per trrigger interval')
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
from geh_stream.streaming_utils.input_source_readers import read_master_data_from_csv

master_data_storage_path = BASE_STORAGE_PATH + args.master_data_path
master_data_df = read_master_data_from_csv(spark, master_data_storage_path)

# %% Read raw time series streaming data from input source
import json

from geh_stream.streaming_utils.input_source_readers import read_time_series_streaming_data

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

raw_streaming_data = read_time_series_streaming_data(spark, input_eh_conf)

# %% Telemetry
from geh_stream.monitoring import Telemetry

telemetry_client = Telemetry.create_telemetry_client(args.telemetry_instrumentation_key)

# %% Process time series as points
from pyspark.sql import DataFrame

from geh_stream.streaming_utils import parse_enrich_and_validate_time_series_as_points
from geh_stream.monitoring import MonitoredStopwatch
import geh_stream.batch_operations as batch_operations

time_series_points = parse_enrich_and_validate_time_series_as_points(raw_streaming_data, master_data_df)


def __process_data_frame(batched_time_series_points: DataFrame, _: int):
    try:
        watch = MonitoredStopwatch.start_timer(telemetry_client, __process_data_frame.__name__)

        # This validation cannot be done in the Validator due to the implementation.
        # It uses a Window, which can not be used in streaming without time.
        batched_time_series_points = batch_operations.add_time_series_validation_status_column(batched_time_series_points)

        # Cache the batch in order to avoid the risk of recalculation in each write operation
        batched_time_series_points = batched_time_series_points.persist()

        # Make valid time series points available to aggregations (by storing in Delta lake)
        batch_operations.store_points_of_valid_time_series(batched_time_series_points, output_delta_lake_path, watch)

        batch_count = batch_operations.get_rows_in_batch(batched_time_series_points, watch)

        watch.stop_timer(batch_count)

        # Collect serializable data about the batch. In order to be able to use it on individual worker nodes
        # when sending telemetry per correlation ID from worker nodes.
        batch_info = {
            "batch_dependency_id": watch.watch_id,
            "batch_row_count": batch_count,
            "batch_duration_ms": watch.duration_ms
        }

        batch_operations.track_batch_back_to_original_correlation_requests(batched_time_series_points, batch_info, args.telemetry_instrumentation_key)

        batched_time_series_points.unpersist()

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
out_stream = time_series_points \
    .writeStream \
    .option("checkpointLocation", checkpoint_path) \
    .trigger(processingTime=args.trigger_interval) \
    .foreachBatch(__process_data_frame)

# %% Start streaming
from datetime import datetime
import sys

from geh_stream.azure import BlobService


def log(message):
    print("%s %s..." % (datetime.utcnow(), message))
    telemetry_client.track_trace("Streaming", {"message": message})
    telemetry_client.flush()


# Purpose of this construction is to ensure that the master data used to enrich the streaming data
# is no older than 5 minutes.
# The alternative of restarting the job every 5ish minutes was considered too expensive.
# The assumption here is that reading of master data and restart of the streaming
# can be done in less than 30 seconds.

blob_service = BlobService(args.storage_account_name, args.storage_account_key, args.storage_container_name)
blob_master_data_version = blob_service.get_blob_poperties(args.master_data_path).last_modified
spark_master_data_version = blob_master_data_version
is_master_data_blob_newer = True

failure_count = 0
max_retry_count = 5

log("Starting streaming...")
while True:
    try:
        if is_master_data_blob_newer:
            # Persist master data to avoid rereading them in each batch
            master_data_df.persist()

            execution = out_stream.start()
            spark_master_data_version = blob_master_data_version

        # Use .awaitTermination() instead of time.sleep() because sleep() makes the stream restart
        # wait despite that the streaming has already stopped.
        execution.awaitTermination(4.5 * 60)

        blob_master_data_version = blob_service.get_blob_poperties(args.master_data_path).last_modified
        is_master_data_blob_newer = blob_master_data_version > spark_master_data_version

        if is_master_data_blob_newer:
            # .awaitTermination() doesn't stop the execution on timeout
            execution.stop()
            log("Continuing with new master data that was updated in ODS at %s..." % blob_master_data_version)
        else:
            log("Continuing with current master data because they have not been updated in ODS...")

        failure_count = 0

    except Exception as error:
        failure_count += 1

        # Make sure the exception is not accidently tracked on the last used parent
        telemetry_client.context.operation.parent_id = None
        # We need to track and flush the exception so it is not lost in case the exception will stop execution
        telemetry_client.track_exception()
        telemetry_client.flush()

        log(str(error))

        # Only retry a certain number of times when consecutive executions fail
        if failure_count > max_retry_count:
            message = "Tried to recover %s times. Giving up now. RIP dear streaming job." % (max_retry_count)
            log(message)
            sys.exit(message)

        log("Trying to recover. Attempt no %s..." % (failure_count))

    finally:
        if is_master_data_blob_newer:
            master_data_df.unpersist()
