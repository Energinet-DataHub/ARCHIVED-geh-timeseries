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

using System;
using System.Threading.Tasks;
using Confluent.Kafka;

namespace GreenEnergyHub.Queues.Kafka
{
    public class KafkaBinaryDispatcher : IDisposable, IKafkaBinaryDispatcher
    {
        private readonly IProducer<Null, byte[]> _producer;
        private bool _disposed;

        public KafkaBinaryDispatcher(IKafkaProducerFactory producerFactory)
        {
            if (producerFactory is null)
            {
                throw new ArgumentNullException(nameof(producerFactory));
            }

            _producer = producerFactory.BuildBinary();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task DispatchAsync(byte[] data, string topic)
        {
            var producerMessage = CreateProducerMessage(data);
            var deliveryResult = await _producer.ProduceAsync(topic, producerMessage).ConfigureAwait(false);

            EnsureDelivered(deliveryResult);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _producer.Dispose();
            }

            _disposed = true;
        }

        private static Message<Null, byte[]> CreateProducerMessage(byte[] data)
        {
            return new Message<Null, byte[]>()
            {
                Value = data,
            };
        }

        private static void EnsureDelivered(DeliveryResult<Null, byte[]> deliveryResult)
        {
            if (deliveryResult.Status != PersistenceStatus.Persisted)
            {
                throw new MessageQueueException("Failed to dispatch request to inbound queue.");
            }
        }
    }
}
