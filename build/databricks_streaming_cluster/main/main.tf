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

data "databricks_spark_version" "latest_lts" {
  long_term_support = true
}

resource "databricks_job" "persister_streaming_job" {
  name = "PersisterStreamingJob"
  max_retries = 2
  max_concurrent_runs = 1   
  always_running = true

  new_cluster {
    spark_version           = data.databricks_spark_version.latest_lts.id
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
    whl = "dbfs:/package/package-1.0-py3-none-any.whl"
  } 

  spark_python_task {
    python_file = "dbfs:/timeseries/timeseries_persister_streaming.py"
    parameters  = [
         "--data-storage-account-name=${data.azurerm_key_vault_secret.st_data_lake_name.value}",
         "--data-storage-account-key=${data.azurerm_key_vault_secret.st_data_lake_primary_access_key.value}",
         "--event-hub-connection-key=${var.evh_timeseries_listen_connection_string}",
         "--time_series_unprocessed_path=abfss://timeseries-data@${data.azurerm_key_vault_secret.st_data_lake_name.value}.dfs.core.windows.net/timeseries-unprocessed",
         "--time_series_raw_path=abfss://timeseries-data@${data.azurerm_key_vault_secret.st_data_lake_name.value}.dfs.core.windows.net/timeseries-raw",
         "--time_series_checkpoint_path=abfss://timeseries-data@${data.azurerm_key_vault_secret.st_data_lake_name.value}.dfs.core.windows.net/checkpoint-timeseries-persister"
    ]
  }


  email_notifications {
    no_alert_for_skipped_runs = true
  }
}

resource "databricks_job" "publisher_streaming_job" {
  name = "PublisherStreamingJob"
  max_retries = 2
  max_concurrent_runs = 1   
  always_running = true

  new_cluster {
    spark_version           = data.databricks_spark_version.latest_lts.id
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
    whl = "dbfs:/package/package-1.0-py3-none-any.whl"
  } 

  spark_python_task {
    python_file = "dbfs:/timeseries/timeseries_publisher_streaming.py"
    # IMPORTANT: Be careful about changing the name of the time series points Delta table name
    # as it is part of the public contract for published time series points
    parameters  = [
         "--data-storage-account-name=${data.azurerm_key_vault_secret.st_data_lake_name.value}",
         "--data-storage-account-key=${data.azurerm_key_vault_secret.st_data_lake_primary_access_key.value}",
         "--event-hub-connection-key=${var.evh_timeseries_listen_connection_string}",
         "--time_series_unprocessed_path=abfss://timeseries-data@${data.azurerm_key_vault_secret.st_data_lake_name.value}.dfs.core.windows.net/timeseries-unprocessed",
         "--time_series_points_path=abfss://timeseries-data@${data.azurerm_key_vault_secret.st_data_lake_name.value}.dfs.core.windows.net/time-series-points",
         "--time_series_checkpoint_path=abfss://timeseries-data@${data.azurerm_key_vault_secret.st_data_lake_name.value}.dfs.core.windows.net/checkpoint-timeseries-publisher"
    ]
  }

  email_notifications {
    no_alert_for_skipped_runs = true
  }
}