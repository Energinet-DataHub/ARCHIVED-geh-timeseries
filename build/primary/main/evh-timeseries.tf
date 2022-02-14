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

module "evhnm_timeseries" {
  source                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/eventhub-namespace?ref=5.1.0"

  name                      = "timeseries"
  project_name              = var.domain_name_short
  environment_short         = var.environment_short
  environment_instance      = var.environment_instance
  resource_group_name       = azurerm_resource_group.this.name
  location                  = azurerm_resource_group.this.location
  sku                       = "Standard"
  capacity                  = 1

  tags                      = azurerm_resource_group.this.tags
}

module "evh_received_timeseries" {
  source                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/eventhub?ref=5.1.0"

  name                      = "received-timeseries"
  namespace_name            = module.evhnm_timeseries.name
  resource_group_name       = azurerm_resource_group.this.name
  partition_count           = 4
  message_retention         = 1
  auth_rules            = [
    {
      name    = "send",
      send    = true
    },
    {
      name    = "listen",
      listen  = true
    },
  ]
}

module "kvs_evh_timeseries_listen_key" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=5.1.0"

  name          = "evh-timeseries-listen-connection-string"
  value         = module.evh_received_timeseries.primary_connection_strings["listen"]
  key_vault_id  = data.azurerm_key_vault.kv_shared.id

  tags          = azurerm_resource_group.this.tags
}