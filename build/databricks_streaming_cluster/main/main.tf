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
  name = "StreamingJob"
  max_retries = 2
  max_concurrent_runs = 1   
  always_running = true

  new_cluster {
    spark_version           = "8.4.x-scala2.12"
    node_type_id            = "Standard_DS3_v2"
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
      package = "azure-storage-blob==12.7.1"
    }
  }

  library {
    pypi {
      package = "dataclasses-json==0.5.6"
    }
  }

  library {
    whl = "dbfs:/package/package-1.0-py3-none-any.whl"
  } 

  spark_python_task {
    python_file = "dbfs:/timeseries/streaming.py"
    parameters  = [
   # params for job will go here
    ]
  }

  email_notifications {
    no_alert_for_skipped_runs = true
  }
}