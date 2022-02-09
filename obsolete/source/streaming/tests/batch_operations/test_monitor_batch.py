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
from pytest_mock import MockerFixture
from unittest.mock import patch, MagicMock
from pyspark.sql import DataFrame
from geh_stream.batch_operations import get_rows_in_batch
from geh_stream.monitoring import MonitoredStopwatch


@patch("geh_stream.monitoring.MonitoredStopwatch")
@patch("pyspark.sql.DataFrame")
def test_get_rows_in_batch_returns_count(df, watch):
    df.count = MagicMock(return_value=3)
    actual = get_rows_in_batch(df, watch)
    assert actual == 3
