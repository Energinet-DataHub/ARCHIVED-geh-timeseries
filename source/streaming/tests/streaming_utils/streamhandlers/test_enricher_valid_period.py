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

import pandas as pd
import time
from decimal import Decimal
from datetime import datetime, timedelta
import pytest

from geh_stream.codelists import SettlementMethod
from geh_stream.streaming_utils.streamhandlers import Enricher


def __create_time_stamp(offset_datetime, minutes_offset: int):
    new_time = offset_datetime + timedelta(minutes=minutes_offset)
    return pd.Timestamp(new_time, unit='s')


offset_time = datetime.now()

# Simulate two master data intervals (with different data)
valid_from1 = __create_time_stamp(offset_time, 0)
valid_to1 = __create_time_stamp(offset_time, 60)  # validTo of first interval and validFrom of second interval
valid_to2 = __create_time_stamp(offset_time, 120)


@pytest.fixture(scope="class")
def master_data(master_data_factory):
    """
    Create two master data intervals. Column 'settlement_method' is arbitrary selected as a flag to make it indetifyable,
    which interval was used in enrichment.
    """
    return master_data_factory([
        dict(metering_point_id="1", valid_from=valid_from1, valid_to=valid_to1, settlement_method=SettlementMethod.profiled.value),
        dict(metering_point_id="1", valid_from=valid_to1, valid_to=valid_to2, settlement_method=SettlementMethod.flex.value)
    ])


@pytest.fixture(scope="class")
def enriched_data_factory(master_data, parsed_data_factory):
    def __factory(**args):

        # TODO: hvorfor bliver der parset et dictionary af et dictionary her, så __get_value_if_exits() ikke kan hive en enkelt værdi ud?

        metering_point_id = __get_value_if_exits(args, "metering_point_id", "mepm")
        quantity = __get_value_if_exits(args, "quantity", Decimal('1.0'))
        observation_date_time = __get_value_if_exits(args, "observation_date_time", valid_from1)

        parsed_data = parsed_data_factory(metering_point_id=metering_point_id, quantity=quantity, observation_date_time=observation_date_time)
        return Enricher.enrich(parsed_data, master_data)
    return __factory


def __get_value_if_exits(args, key, default):
    return args[key] if args.get(key) is not None else default


def test_valid_from_is_inclusive(enriched_data_factory):
    # enriched_data = enriched_data_factory(metering_point_id='1', observation_date_time=pd.Timestamp(valid_from1, unit='s'))
    enriched_data = enriched_data_factory(metering_point_id='1', observation_date_time=valid_from1)
    first = enriched_data.first()
    print(str(first))
    assert first.series_settlementMethod == SettlementMethod.flex.value


def test_valid_to_is_exclusive(enriched_data_factory):
    # Act
    enriched_data = enriched_data_factory(metering_point_id='1', observation_date_time=valid_to1)

    # Assert: Only one of the intervals was used in join (previously we had an error where the time series point
    # multiplied because it was matched by both the end of first interval and beginning of second interval).
    assert enriched_data.count() == 1

    # Assert: The time series point was matched with the second interval (when it matches both the end of first
    # interval and the begining of the second interval).
    assert enriched_data.first().series_settlementMethod == SettlementMethod.flex.value
