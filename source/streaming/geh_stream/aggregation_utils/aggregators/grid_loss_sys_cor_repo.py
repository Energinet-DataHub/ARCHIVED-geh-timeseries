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
from datetime import datetime
from pyspark.sql.types import StructType, StringType, TimestampType, BooleanType
from pyspark import SparkConf
from pyspark.sql import SparkSession
import pandas as pd


class GridLossSysCorRepo:

    def __init__(self):
        default_valid_from = datetime(2010, 10, 1)

        self.grid_loss_mps = [
            {
                "MeteringGridArea_Domain_mRID": "500",
                "EnergySupplier_MarketParticipant_mRID": "8510000000006",
                "ValidFrom": default_valid_from,
                "ValidTo": None,
                "IsGridLoss": True,
                "IsSystemCorrection": False,
            },
            {
                "MeteringGridArea_Domain_mRID": "501",
                "EnergySupplier_MarketParticipant_mRID": "8510000000013",
                "ValidFrom": default_valid_from,
                "ValidTo": None,
                "IsGridLoss": True,
                "IsSystemCorrection": False,
            },
            {
                "MeteringGridArea_Domain_mRID": "502",
                "EnergySupplier_MarketParticipant_mRID": "8510000000020",
                "ValidFrom": default_valid_from,
                "ValidTo": None,
                "IsGridLoss": True,
                "IsSystemCorrection": False,
            },
        ]

        self.sys_cor_mps = [
            {
                "MeteringGridArea_Domain_mRID": "500",
                "EnergySupplier_MarketParticipant_mRID": "8510000000006",
                "ValidFrom": default_valid_from,
                "ValidTo": None,
                "IsGridLoss": False,
                "IsSystemCorrection": True,
            },
            {
                "MeteringGridArea_Domain_mRID": "501",
                "EnergySupplier_MarketParticipant_mRID": "8510000000020",
                "ValidFrom": default_valid_from,
                "ValidTo": None,
                "IsGridLoss": False,
                "IsSystemCorrection": True,
            },
            {
                "MeteringGridArea_Domain_mRID": "502",
                "EnergySupplier_MarketParticipant_mRID": "8510000000013",
                "ValidFrom": default_valid_from,
                "ValidTo": None,
                "IsGridLoss": False,
                "IsSystemCorrection": True,
            },
        ]

    def get_df(self):
        spark_conf = SparkConf(loadDefaults=True) \
            .set("spark.sql.session.timeZone", "UTC")
        spark_session = SparkSession \
            .builder \
            .config(conf=spark_conf) \
            .getOrCreate()

        schema = StructType() \
            .add("MeteringGridArea_Domain_mRID", StringType(), False) \
            .add("EnergySupplier_MarketParticipant_mRID", StringType()) \
            .add("ValidFrom", TimestampType()) \
            .add("ValidTo", TimestampType()) \
            .add("IsGridLoss", BooleanType()) \
            .add("IsSystemCorrection", BooleanType())

        pandas_df = pd.DataFrame(self.grid_loss_mps[0], index=[0])

        for i in range(1, len(self.grid_loss_mps)):
            pandas_df = pandas_df.append(self.grid_loss_mps[i], ignore_index=True)
        for j in range(len(self.sys_cor_mps)):
            pandas_df = pandas_df.append(self.sys_cor_mps[j], ignore_index=True)
        return spark_session.createDataFrame(pandas_df, schema)
