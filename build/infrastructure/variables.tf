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
variable "resource_group_name" {
  type = string
}

variable "environment" {
  type          = string
  description   = "Enviroment that the infrastructure code is deployed into"
}

variable "project" {
  type          = string
  description   = "Project that is running the infrastructure code"
}

variable "organisation" {
  type          = string
  description   = "Organisation that is running the infrastructure code"
}

variable "tenant_id" {
  type          = string
  description   = "tenant id"
}

variable "spn_object_id" {
  type          = string
  description   = "spn_object_id"
}

variable "streaming_container_name" {
  type = string
  default = "messagedata"
}

variable "cacert_path" {
  type = string
  default = "c:\\cacert\cacert.pem"
}

# TODO BJARKE: From green-energy-hub-archived repo
# variable "current_spn_id" {
#   type          = string
#   description   = "Service Principal ID of the connection used to deploy the code"
# }

# variable "current_spn_secret" {
#   type          = string
#   description   = "Service Principal secret of the connection used to deploy the code"
# }

# variable "current_subscription_id" {
#   type          = string
#   description   = "The ID of the subscription that the infrastructure code is deployed into"
# }
