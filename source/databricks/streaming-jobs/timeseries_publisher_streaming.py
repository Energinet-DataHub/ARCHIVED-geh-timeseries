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

import sys
sys.path.append(r'/workspaces/geh-timeseries/source/databricks')
sys.path.append(r'/opt/conda/lib/python3.8/site-packages')

import configargparse

from package import timeseries_publisher, initialize_spark

p = configargparse.ArgParser(description='Timeseries etl stream', formatter_class=configargparse.ArgumentDefaultsHelpFormatter)
p.add('--data-storage-account-name', type=str, required=True)
p.add('--data-storage-account-key', type=str, required=True)
p.add('--event-hub-connection-key', type=str, required=True)
p.add('--delta-lake-container-name', type=str, required=True)
p.add('--timeseries-unprocessed-blob-name', type=str, required=True)
p.add('--timeseries-processed-blob-name', type=str, required=True)

args, unknown_args = p.parse_known_args()

spark = initialize_spark(args)

timeseries_unprocessed_path = f'abfss://{args.delta_lake_container_name}@{args.data_storage_account_name}.dfs.core.windows.net/{args.timeseries_unprocessed_blob_name}'
timeseries_processed_path = f'abfss://{args.delta_lake_container_name}@{args.data_storage_account_name}.dfs.core.windows.net/{args.timeseries_processed_blob_name}'

# start the unprocessed timeseries publisher
timeseries_publisher(args.delta_lake_container_name, args.data_storage_account_name, timeseries_unprocessed_path, timeseries_processed_path)
