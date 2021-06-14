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

import requests
import gzip
from geh_stream.monitoring import Telemetry


class CoordinatorService:

    def __init__(self, args):
        self.coordinatorUrl = args.result_url
        self.resultId = args.result_id
        self.processType = args.process_type
        self.startTime = args.beginning_date_time
        self.endTime = args.end_date_time
        self.telemetry_client = Telemetry.create_telemetry_client(args.telemetry_instrumentation_key)

    def SendResultToCoordinator(self, result):
        try:
            bytes = result.encode()
            headers = {'result-id': self.resultId,
                       'process-type': self.processType,
                       'start-time': self.startTime,
                       'end-time': self.endTime,
                       'Content-Type': 'application/json',
                       'Content-Encoding': 'gzip'}

            request_body = gzip.compress(bytes)
            response = requests.post(self.coordinatorUrl, data=request_body, headers=headers)
            if response.status_code != requests.codes['ok']:
                raise Exception("Could not communicate with coordinator")
        except Exception:
            self.telemetry_client.track_exception(Exception)
            print(Exception)
            raise Exception
        self.telemetry_client.flush()
