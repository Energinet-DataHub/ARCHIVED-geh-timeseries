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


class QuantityQuality(Enum):
    unknown = 0,
    measured = 1 # Received as E01 in ebiX
    revised = 2 # Received as 36 in ebiX
    estimated = 3 # Received as 56 in ebiX
    quantity_missing = 4 # Received in separate boolean field in ebiX
    calculated = 5 # Received as D01 in ebiX