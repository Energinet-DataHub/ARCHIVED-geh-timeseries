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

#Set the terraform required version
terraform {
  required_version = ">= 0.12.6"

  required_providers {
    databricks = {
      source = "databrickslabs/databricks"
      version = "0.2.5"
    }
  }
}

provider "databricks" {
  azure_workspace_resource_id = var.databricks_id
}

provider "azurerm" {
  features {}
  skip_provider_registration = true  
}