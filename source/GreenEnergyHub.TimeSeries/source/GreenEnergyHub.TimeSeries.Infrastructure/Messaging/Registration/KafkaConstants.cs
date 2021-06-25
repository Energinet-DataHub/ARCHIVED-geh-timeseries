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

namespace GreenEnergyHub.TimeSeries.Infrastructure.Messaging.Registration
{
    /// <summary>
    /// Variables needed for establishing EventHub connection via Kafka.
    /// For the time being they are hardcoded. Later, they are expected to be retrieved elsewhere.
    /// For more info, <see href="https://docs.confluent.io/platform/current/clients/confluent-kafka-dotnet/_site/api/Confluent.Kafka.ProducerConfig.html#properties">Kafka properties</see>
    /// </summary>
    public static class KafkaConstants
    {
        public const string SaslMechanism = "Plain";

        public const string SaslUsername = "$ConnectionString";

        public const string SecurityProtocol = "SaslSsl";

        public const string SslCaLocation = "C:\\cacert\\cacert.pem";

        public const int MessageTimeoutMs = 1000;

        public const int MessageSendMaxRetries = 5;
    }
}
