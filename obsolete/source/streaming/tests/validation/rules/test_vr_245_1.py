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
import pytest
from geh_stream.codelists import MeteringPointType
from geh_stream.validation.rules.vr_245_1 import validate_vr_245_1


@pytest.mark.parametrize(
    "quantity,metering_point_type,expected",
    [
        pytest.param(
            0.0, MeteringPointType.consumption.value, True, id="valid because quantity is not negative"
        ),
        pytest.param(
            -1.0, MeteringPointType.consumption.value, False, id="invalid because quantity is negative"
        ),
        pytest.param(
            -1.0, MeteringPointType.production.value, True, id="valid when quantity is negative because it's not a consumption metering point"
        ),
    ],
)
def test_vr_245_1(quantity, metering_point_type, expected, enriched_data_factory):
    data = enriched_data_factory(quantity=quantity,
                                 metering_point_type=metering_point_type)
    validated_data = validate_vr_245_1(data)
    assert validated_data.first()["VR-245-1-Is-Valid"] == expected
