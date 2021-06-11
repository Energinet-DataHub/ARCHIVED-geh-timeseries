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
from azure.storage.blob import BlobServiceClient


class BlobService:
    def __init__(self, storage_account_name, storage_account_key, storage_container_name):
        self.storage_container_name = storage_container_name

        self.account_url = "https://{0}.blob.core.windows.net/".format(storage_account_name)
        self.blobService = BlobServiceClient(account_url=self.account_url, credential=storage_account_key)

    def get_blob_poperties(self, blob_name):
        blob_client = self.blobService.get_blob_client(self.storage_container_name, blob_name)
        return blob_client.get_blob_properties()
