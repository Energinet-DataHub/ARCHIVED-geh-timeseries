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
p.add('--time_series_unprocessed_path', type=str, required=True)
p.add('--time_series_points_path', type=str, required=True)
p.add('--time_series_checkpoint_path', type=str, required=True)

args, unknown_args = p.parse_known_args()
spark = initialize_spark(args)

time_series_unprocessed_path = f"{args.time_series_unprocessed_path}"
time_series_points_path = f"{args.time_series_points_path}"
time_series_checkpoint_path = f"{args.time_series_checkpoint_path}"

# Start the timeseries publisher
timeseries_publisher(spark, time_series_unprocessed_path, time_series_checkpoint_path, time_series_points_path)
