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


class MeteringPointType(Enum):
    consumption = 1 # E17 in ebIX
    production = 2 # E18 in ebIX
    exchange = 3 # E20 in ebIX
    ve_production = 4 # D01 in ebIX
    analysis = 5 # D02 in ebIX
    surplus_production_group = 6 # D04 in ebIX
    own_production = 11 # D09 in ebIX
    exchange_reactive_energy = 20 # D20 in ebIX
