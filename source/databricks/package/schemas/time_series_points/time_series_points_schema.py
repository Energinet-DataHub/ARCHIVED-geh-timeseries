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
from pyspark.sql.types import (
    DecimalType,
    StructType,
    StructField,
    StringType,
    TimestampType,
    IntegerType,
    LongType,
)
from .time_series_points_colname import TimeSeriesPointsColname as Colname


time_series_points_schema = StructType(
    [
        StructField(Colname.MeteringPointId, StringType(), True),
        StructField(Colname.TransactionId, StringType(), True),
        StructField(Colname.Quantity, DecimalType(18, 3), True),
        StructField(Colname.Quality, LongType(), True),
        StructField(Colname.Position, LongType(), True),
        StructField(Colname.Resolution, LongType(), True),
        StructField(Colname.StartDateTime, TimestampType(), True),
        StructField(Colname.RegistrationDateTime, TimestampType(), True),
        StructField(Colname.CreatedDateTime, TimestampType(), True),
        StructField(Colname.StoredTime, TimestampType(), True),
        StructField(Colname.Time, TimestampType(), True),
        StructField(Colname.Year, IntegerType(), True),
        StructField(Colname.Month, IntegerType(), True),
        StructField(Colname.Day, IntegerType(), True),
    ]
)
