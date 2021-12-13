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
resource "databricks_job" "streaming_job" {
  name = var.module_name
  max_retries = 2
  max_concurrent_runs = 1

  new_cluster { 
    spark_version  = "9.1.x-scala2.12"
    node_type_id   = "Standard_DS3_v2"
    num_workers    = 1
  }
	
  library {
    maven {
      coordinates = "com.microsoft.azure:azure-eventhubs-spark_2.12:2.3.17"
    }
  }

  library {
    pypi {
      package = "configargparse==1.2.3"
    }
  }

  library {
    pypi {
      package = "protobuf==3.*"
    }
  }

  library {
    pypi {
      package = "applicationinsights==0.11.9"
    }
  }
  
  library {
    pypi {
      package = "azure-storage-blob==12.7.1"
    }
  }  

  library {
    whl = var.wheel_file
  } 

  spark_python_task {
    python_file = var.python_main_file
    parameters  = [
      "--storage-account-name=${var.storage_account_name}",
      "--storage-account-key=${var.storage_account_key}",
      "--storage-container-name=${var.streaming_container_name}",
      "--master-data-path=master-data",
      "--meter-data-path=meter-data/",
      "--input-eh-connection-string=${var.input_eventhub_listen_connection_string}",
      "--max-events-per-trigger=100",
      "--trigger-interval=1 second",
      "--streaming-checkpoint-path=checkpoints/streaming",
      "--telemetry-instrumentation-key=${var.appinsights_instrumentation_key}",
    ]
  }

  email_notifications {
    no_alert_for_skipped_runs = true
  }
}
