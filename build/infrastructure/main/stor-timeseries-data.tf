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
module "stor_timeseries_data" {
  source                          = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//storage-account?ref=1.8.0"
  name                            = "data${lower(var.project)}${lower(var.organisation)}${lower(var.environment)}"
  resource_group_name             = data.azurerm_resource_group.main.name
  location                        = data.azurerm_resource_group.main.location
  account_replication_type        = "LRS"
  access_tier                     = "Hot"
  account_tier                    = "Standard"
  is_hns_enabled                  = true
  tags                            = data.azurerm_resource_group.main.tags
}

module "kvs_timeseries_storage_account_key" {
  source                          = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//key-vault-secret?ref=1.8.0"
  name                            = "timeseries-storage-account-key"
  value                           = module.stor_timeseries_data.primary_access_key
  key_vault_id                    = module.kv_timeseries.id
  dependencies = [
    module.kv_timeseries.dependent_on,
    module.stor_timeseries_data.dependent_on
  ]
}

module "stor_streaming_container" {
  source                          = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//storage-container?ref=1.8.0"
  container_name                  = var.streaming_container_name
  storage_account_name            = module.stor_timeseries_data.name
  container_access_type           = "private"
  dependencies                    = [ module.stor_timeseries_data.dependent_on ]
}

resource "azurerm_storage_blob" "master_data" {
  name                            = "master-data/master-data.csv"
  storage_account_name            = module.stor_timeseries_data.name
  storage_container_name          = module.stor_streaming_container.name
  type                            = "Block"
  source                          = "../../source/streaming/tests/streaming_utils/testdata/master-data.csv"
}
