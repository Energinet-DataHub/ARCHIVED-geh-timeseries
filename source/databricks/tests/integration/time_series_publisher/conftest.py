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


@pytest.fixture(scope="session")
def unprocessed_time_series_json_string():
    return """
    {
      "BusinessReasonCode": 0,
      "CreatedDateTime": "2022-06-09T12:09:15.000Z",
      "MeasureUnit": 0,
      "MeteringPointId": "1",
      "MeteringPointType": 2,
      "Period": {
        "EndDateTime": "2022-06-09T12:09:15.000Z",
        "Points": [{
          "Position": 1,
          "Quality": 3,
          "Quantity": "1.1"
        }, {
          "Position": 1,
          "Quality": 3,
          "Quantity": "1.1"
        }],
        "Resolution": 2,
        "StartDateTime": "2022-06-08T12:09:15.000Z"
      },
      "Product": "1",
      "Receiver": {
        "BusinessProcessRole": 0,
        "Id": "2"
      },
      "RegistrationDateTime": "2022-06-09T12:09:15.000Z",
      "Sender": {
        "BusinessProcessRole": 0,
        "Id": "1"
      },
      "SeriesId": "1",
      "TransactionId": "1"
    }"""
