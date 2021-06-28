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

using Azure.Messaging.ServiceBus;
using GreenEnergyHub.Messaging.Transport;
using GreenEnergyHub.Queues.Kafka;
using GreenEnergyHub.TimeSeries.Application;
using GreenEnergyHub.TimeSeries.Infrastructure.Messaging.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace GreenEnergyHub.TimeSeries.Infrastructure.Messaging.Registration
{
    public class MessagingRegistrator
    {
        private readonly IServiceCollection _services;

        internal MessagingRegistrator(IServiceCollection services)
        {
            _services = services;
        }

        /// <summary>
        /// Register services required to resolve a <see cref="MessageExtractor{TInboundMessage}"/>.
        /// </summary>
        public MessagingRegistrator AddMessageExtractor<TInboundMessage>()
            where TInboundMessage : IInboundMessage
        {
            _services.AddScoped<MessageExtractor<TInboundMessage>>();
            _services.AddScoped<MessageDeserializer<TInboundMessage>, JsonMessageDeserializer<TInboundMessage>>();

            return this;
        }

        /// <summary>
        /// Register services required to resolve a <see cref="IMessageDispatcher{TInboundMessage}"/>.
        /// </summary>
        public MessagingRegistrator AddServiceBusMessageDispatcher<TOutboundMessage>(
            string serviceBusConnectionString,
            string serviceBusTopicName)
            where TOutboundMessage : IOutboundMessage
        {
            _services.AddScoped<IMessageDispatcher<TOutboundMessage>, MessageDispatcher<TOutboundMessage>>();
            _services.AddScoped<Channel<TOutboundMessage>, ServiceBusChannel<TOutboundMessage>>();

            // Must be a singleton as per documentation of ServiceBusClient and ServiceBusSender
            _services.AddSingleton<IServiceBusSender<TOutboundMessage>>(
                _ =>
                {
                    var client = new ServiceBusClient(serviceBusConnectionString);
                    var instance = client.CreateSender(serviceBusTopicName);
                    return new ServiceBusSender<TOutboundMessage>(instance);
                });

            return this;
        }

        public MessagingRegistrator AddKafkaMessageDispatcher<TOutboundMessage>(
            string eventHubQueue,
            string eventHubPassword,
            string eventHubTopic,
            string cacertPath)
            where TOutboundMessage : IOutboundMessage
        {
            _services.AddScoped<IMessageDispatcher<TOutboundMessage>, MessageDispatcher<TOutboundMessage>>();
            _services.AddScoped<Channel<TOutboundMessage>, EventHubChannel<TOutboundMessage>>();

            _services.AddSingleton<IKafkaDispatcher<TOutboundMessage>>(
                _ =>
                {
                    var kafkaConfiguration = new KafkaConfiguration
                    {
                        BoostrapServers = eventHubQueue,
                        SaslMechanism = KafkaConstants.SaslMechanism,
                        SaslUsername = KafkaConstants.SaslUsername,
                        SaslPassword = eventHubPassword,
                        SecurityProtocol = KafkaConstants.SecurityProtocol,
                        SslCaLocation = cacertPath,
                        MessageTimeoutMs = KafkaConstants.MessageTimeoutMs,
                        MessageSendMaxRetries = KafkaConstants.MessageSendMaxRetries,
                    };
                    var producer = new KafkaProducerFactory(kafkaConfiguration);
                    var instance = new KafkaDispatcher(producer);
                    return new KafkaDispatcher<TOutboundMessage>(instance, eventHubTopic);
                });

            return this;
        }
    }
}
