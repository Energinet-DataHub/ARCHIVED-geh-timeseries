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
from package.codelists import Resolution, TimeSeriesQuality
from tests.contract_utils import assert_codelist_matches_contract


def test_resolution_is_subset_of_contract(source_path):
    assert_codelist_matches_contract(
        Resolution, f"{source_path}/contracts/enums/time-series-resolution.json"
    )


# TODO: How do we know when the contract is with .NET and when it's for external contracts?
#       Comment applies to contracts in opengeh-wholesale as-well
def test_timeseries_quality_enum_equals_timeseries_contract(source_path):
    assert_codelist_matches_contract(
        TimeSeriesQuality, f"{source_path}/contracts/enums/timeseries-quality.json"
    )
