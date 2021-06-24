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
module "evhnm_received_queue" {
  source                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//event-hub-namespace?ref=1.3.0"
  name                      = "evhnm-received-queue-${var.project}-${var.organisation}-${var.environment}"
  resource_group_name       = data.azurerm_resource_group.main.name
  location                  = data.azurerm_resource_group.main.location
  sku                       = "Standard"
  capacity                  = 1
  tags                      = data.azurerm_resource_group.main.tags
}

module "evh_receivedqueue" {
  source                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//event-hub?ref=1.3.0"
  name                      = "evh-received-queue"
  namespace_name            = module.evhnm_received_queue.name
  resource_group_name       = data.azurerm_resource_group.main.name
  partition_count           = 32
  message_retention         = 1
  dependencies              = [module.evhnm_received_queue.dependent_on]
}

module "evhar_receivedqueue_sender" {
  source                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//event-hub-auth-rule?ref=1.3.0"
  name                      = "evhar-receivedqueue-sender"
  namespace_name            = module.evhnm_received_queue.name
  eventhub_name             = module.evh_receivedqueue.name
  resource_group_name       = data.azurerm_resource_group.main.name
  send                      = true
  dependencies              = [module.evh_receivedqueue.dependent_on]
}

module "evhar_receivedqueue_receiver" {
  source                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//event-hub-auth-rule?ref=1.3.0"
  name                      = "evhar-receivedqueue-receiver"
  namespace_name            = module.evhnm_received_queue.name
  eventhub_name             = module.evh_receivedqueue.name
  resource_group_name       = data.azurerm_resource_group.main.name
  listen                    = true
  dependencies              = [module.evh_receivedqueue.dependent_on]
}

module "kvs_receivedqueue_sender_connection_string" {
  source                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//key-vault-secret?ref=1.3.0"
  name                      = "evhar-receivedqueue-sender-connection-string"
  value                     = module.evhar_receivedqueue_sender.primary_connection_string
  key_vault_id              = module.kv_timeseries.id
  dependencies = [
      module.kv_timeseries.dependent_on, 
      module.evhar_receivedqueue_sender.dependent_on
  ]
}

module "evhar_receivedqueue_receiver_connection_string" {
  source                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//key-vault-secret?ref=1.3.0"
  name                      = "evhar-receivedqueue-receiver-connection-string"
  value                     = module.evhar_receivedqueue_receiver.primary_connection_string
  key_vault_id              = module.kv_timeseries.id
  dependencies = [
      module.kv_timeseries.dependent_on, 
      module.evhar_receivedqueue_receiver.dependent_on
  ]
}
