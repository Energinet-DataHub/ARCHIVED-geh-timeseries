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

# https://github.com/aroch/protobuf-dataframe
# from protodf import schema_for
# from protodf import message_to_row
from geh_stream.protodf import schema_for, message_to_row

from pyspark import RDD, Row
from pyspark.sql.functions import udf, col
from pyspark.sql import DataFrame
from pyspark.sql.types import StructType

from geh_stream.contracts.time_series_pb2 import TimeSeriesCommandContract


def specific_message_bytes_to_row(pb_bytes):
    msg = TimeSeriesCommandContract.FromString(pb_bytes)
    row = message_to_row(TimeSeriesCommandContract().DESCRIPTOR, msg)
    return row


schema = schema_for(TimeSeriesCommandContract().DESCRIPTOR)
specific_message_bytes_to_row_udf = udf(specific_message_bytes_to_row, schema)


class ProtobufMessageParser:
    @staticmethod
    def parse(raw_data: DataFrame, message_schema: StructType) -> DataFrame:
        parsed_data = raw_data.withColumn("event", specific_message_bytes_to_row_udf(col("body")))

        parsed_data = parsed_data.select("event.*")
        print("Parsed stream schema:")
        parsed_data.printSchema()

        return parsed_data
