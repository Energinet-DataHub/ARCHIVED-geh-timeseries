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
from pyspark.sql.types import StringType, StructType

sys.path.append(r'/workspaces/geh-timeseries/source/databricks')
sys.path.append(r'/opt/conda/lib/python3.8/site-packages')

import configargparse

from package import timeseries_persister, initialize_spark

p = configargparse.ArgParser(description='Timeseries events stream ingestor', formatter_class=configargparse.ArgumentDefaultsHelpFormatter)
p.add('--data-storage-account-name', type=str, required=True)
p.add('--data-storage-account-key', type=str, required=True)
p.add('--time_series_unprocessed_path', type=str, required=True)
p.add('--time_series_raw_path', type=str, required=True)
p.add('--time_series_checkpoint_path', type=str, required=True)
p.add('--test', type=str, required=False)

args, unknown_args = p.parse_known_args()

spark = initialize_spark(args)

time_series_unprocessed_path = f"{args.time_series_unprocessed_path}"
time_series_raw_path = f"{args.time_series_raw_path}"
checkpoint_path = f"{args.time_series_checkpoint_path}"

from package.codelists import Colname
from pyspark.sql.types import DecimalType, StructType, StructField, StringType, TimestampType, IntegerType, LongType, ArrayType

time_series_points_schema = StructType([
    StructField("BusinessReasonCode", LongType(), True),
    StructField("CreatedDateTime", TimestampType(), True),
    StructField("DocId", StringType(), True),
    StructField("MeasureUnit", LongType(), True),
    StructField("MeteringPointId", StringType(), True),
    StructField("MeteringPointType", LongType(), True),
    StructField("Period", StructType([
        StructField("EndDateTime", TimestampType(), True),
        StructField("Points", ArrayType(
            StructType([
                StructField("Position", LongType(), True),
                StructField("Quality", LongType(), True),
                StructField("Quantity", StringType(), True)
            ])
        )),
        StructField("Resolution", LongType(), True),
        StructField("StartDateTime", TimestampType(), True)
    ])),
    StructField("Product", StringType(), True),
    StructField("Receiver", StructType([
        StructField("BusinessProcessRole", LongType(), True),
        StructField("Id", StringType(), True)
    ])),
    StructField("RegistrationDateTime", TimestampType(), True),
    StructField("Sender", StructType([
        StructField("BusinessProcessRole", LongType(), True),
        StructField("Id", StringType(), True) 
    ])),
    StructField("SeriesId", StringType(), True),
    StructField("TransactionId", StringType(), True)
])

streamingDF = (spark
               .readStream
               .schema(time_series_points_schema)
               .json(time_series_raw_path)) 

# start the timeseries persister job
if args.test == "true":
    timeseries_persister(streamingDF, checkpoint_path, time_series_unprocessed_path).awaitTermination(25)
else:
    timeseries_persister(streamingDF, checkpoint_path, time_series_unprocessed_path)