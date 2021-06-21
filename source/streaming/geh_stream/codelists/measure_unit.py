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
from enum import Enum


class MeasureUnit(Enum):
    unknown = 0,
    kilo_watt_hour = 1 # Received as KWH in ebiX
    mega_watt_hour = 2 # Received as MWH in ebiX
    kilo_watt = 3 # Received as KWT in ebiX
    mega_watt = 4 # Received as MAW in ebiX
    kilo_var_hour = 5 # Received as K3 in ebiX
    mega_var = 6 # Received as Z03 in ebiX
    tariff = 7 # Received as Z14 in ebiX
    tonne = 8 # Received as TNE in ebiX
