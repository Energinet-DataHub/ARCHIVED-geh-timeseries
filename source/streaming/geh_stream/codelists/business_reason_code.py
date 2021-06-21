# Copyright 2021 Energinet DataHub A/S
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


class BusinessReasonCode(Enum):
    unknown = 0,
    continuous_meter_reading = 1 # This will be received as D06 i ebiX
    periodic_metering = 2 # This will be received as E23 in ebiX
    historical_data = 3 # This will be received as E30 in ebiX
    periodic_flex_metering = 4 # This will be received as D42 in ebiX
