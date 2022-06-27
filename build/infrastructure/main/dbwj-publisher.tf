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

resource "databricks_job" "publisher_streaming_job" {
  name = "PublisherStreamingJob"
  max_retries = -1
  max_concurrent_runs = 1
  always_running = true

  task {
     # The job must be recreated with each deployment and this is achieved using a unique resource id.
    task_key = "${databricks_job.publisher_streaming_job.id}_${uuid()}"

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
    whl = "dbfs:/geh-timeseries/package-1.0-py3-none-any.whl"
  } 

  python_wheel_task {
    package_name = "package"
    entry_point = "start_publisher"
    # IMPORTANT: Be careful about changing the name of the time series points Delta table name
    # as it is part of the public contract for published time series points
    parameters  = [
         "--data-storage-account-name=${data.azurerm_key_vault_secret.st_shared_data_lake_name.value}",
         "--data-storage-account-key=${data.azurerm_key_vault_secret.kvs_st_data_lake_primary_access_key.value}",
         "--event-hub-connection-key=${module.evh_received_timeseries.primary_connection_strings["listen"]}",
         "--time_series_unprocessed_path=abfss://timeseries-data@${data.azurerm_key_vault_secret.st_shared_data_lake_name.value}.dfs.core.windows.net/timeseries-unprocessed",
         "--time_series_points_path=abfss://timeseries-data@${data.azurerm_key_vault_secret.st_shared_data_lake_name.value}.dfs.core.windows.net/time-series-points",
         "--time_series_checkpoint_path=abfss://timeseries-data@${data.azurerm_key_vault_secret.st_shared_data_lake_name.value}.dfs.core.windows.net/checkpoint-timeseries-publisher"
    ]
  }
}

  email_notifications {
    no_alert_for_skipped_runs = true
  }
}
