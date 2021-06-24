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
from pyspark.sql import DataFrame, SparkSession
from pyspark.sql.functions import col


class Enricher:

    @staticmethod
    def enrich(parsed_data: DataFrame, master_data: DataFrame):
        # Enrich time series points with master data by joining on metering point id and valid period.
        # validFrom is inclusive while validTo is exclusive.
        joined_data = parsed_data.alias("pd") \
            .join(master_data.alias("md"),
                  (col("pd.series_meteringPointId") == col("md.meteringPointId"))
                  & (col("pd.series_point_observationTime") >= col("md.validFrom"))
                  & (col("pd.series_point_observationTime") < col("md.validTo")), how="left")

        # Remove column that are only needed in order to be able to do the join
        return joined_data \
            .drop(master_data["validFrom"]).drop(master_data["validTo"])
