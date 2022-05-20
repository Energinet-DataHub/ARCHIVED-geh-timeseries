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
  source                          = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/eventhub-namespace?ref=6.0.0"

  name                            = "timeseries"
  project_name                    = var.domain_name_short
  environment_short               = var.environment_short
  environment_instance            = var.environment_instance
  resource_group_name             = azurerm_resource_group.this.name
  location                        = azurerm_resource_group.this.location
  sku                             = "Standard"
  capacity                        = 1
  log_analytics_workspace_id      = data.azurerm_key_vault_secret.log_shared_id.value
  private_endpoint_subnet_id      = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  approved_sender_subnet_id       = data.azurerm_key_vault_secret.snet_vnet_integrations_id.value

  tags                            = azurerm_resource_group.this.tags
}

module "evh_received_timeseries" {
  source                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/eventhub?ref=6.0.0"

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
