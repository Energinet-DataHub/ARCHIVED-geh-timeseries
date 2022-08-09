// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Energinet.DataHub.TimeSeries.TimeSeriesBundleIngestor
{
    public static class EnvironmentSettingNames
    {
        public static string B2CTenantId => "B2C_TENANT_ID";

        public static string BackendServiceAppId => "BACKEND_SERVICE_APP_ID";

        public static string StorageAccountName => "DATA_LAKE_ACCOUNT_NAME";

        public static string StorageContainerName => "DATA_LAKE_CONTAINER_NAME";

        public static string StorageConnectionString => "DATA_LAKE_CONNECTION_STRING";

        public static string StorageKey => "DATA_LAKE_KEY";

        public static string TimeSeriesRaw => "TimeSeriesRawFolder:FolderName";

        public static string DatabricksApiToken => "DATABRICKS_API_TOKEN";

        public static string DatabricksApiUri => "DATABRICKS_API_URI";

        public static string DatabricksPresisterStreamingJob => "DATABRICKS_PRESISTER_STREAMING_JOB_NAME";

        public static string DatabricksPublisherStreamingJob => "DATABRICKS_PUBLISHER_STREAMING_JOB_NAME";

        public static string DatabricksHealthCheckEnabled => "DATABRICKS_HEALTH_CHECK_ENABLED";
    }
}