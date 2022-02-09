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
    unknown = 0
    kilo_watt_hour = 1
    mega_watt_hour = 2
    kilo_watt = 3
    mega_watt = 4
    kilo_var_hour = 5
    mega_var = 6
    tariff = 7
    tonne = 8
