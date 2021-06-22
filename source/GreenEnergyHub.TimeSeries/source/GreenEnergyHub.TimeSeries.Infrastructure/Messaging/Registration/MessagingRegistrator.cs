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
            _services.AddScoped<JsonMessageDeserializer<TInboundMessage>, JsonMessageDeserializer<TInboundMessage>>();

            return this;
        }

        /// <summary>
        /// Register services required to resolve a <see cref="IMessageDispatcher{TInboundMessage}"/>.
        /// </summary>
        public MessagingRegistrator AddMessageDispatcher<TOutboundMessage>(
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

        public MessagingRegistrator AddMessageDispatcher<TOutboundMessage>()
            where TOutboundMessage : IOutboundMessage
        {
            _services.AddScoped<IMessageDispatcher<TOutboundMessage>, MessageDispatcher<TOutboundMessage>>();
            _services.AddScoped<Channel<TOutboundMessage>, EventHubChannel<TOutboundMessage>>();

            _services.AddSingleton<IKafkaDispatcher<TOutboundMessage>>(
                _ =>
                {
                    var kafkaConfiguration = new KafkaConfiguration
                    {
                        BoostrapServers = "TESTME123",
                        SaslMechanism = "Plain",
                        SaslUsername = "$ConnectionString",
                        SaslPassword = "TESTME123",
                        SecurityProtocol = "SaslSsl",
                        SslCaLocation = "C:\\cacert\\cacert.pem",
                        MessageTimeoutMs = 1000,
                        MessageSendMaxRetries = 5,
                    };
                    var producer = new KafkaProducerFactory(kafkaConfiguration);
                    var instance = new KafkaDispatcher(producer);
                    return new KafkaDispatcher<TOutboundMessage>(instance);
                });

            return this;
        }

        /*_services.AddSingleton<>(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var kaftaConfiguration = new KafkaConfiguration
            {
                BoostrapServers = configuration.GetValue<string>("TIMESERIES_QUEUE_URL"),
                SaslMechanism = configuration.GetValue<string>("KAFKA_SASL_MECHANISM"),
                SaslUsername = configuration.GetValue<string>("KAFKA_USERNAME"),
                SaslPassword = configuration.GetValue<string>("TIMESERIES_QUEUE_CONNECTION_STRING"),
                SecurityProtocol = configuration.GetValue<string>("KAFKA_SECURITY_PROTOCOL"),
                SslCaLocation =
                    Environment.ExpandEnvironmentVariables(configuration.GetValue<string>("KAFKA_SSL_CA_LOCATION")),
                MessageTimeoutMs = configuration.GetValue<int>("KAFKA_MESSAGE_TIMEOUT_MS"),
                MessageSendMaxRetries = configuration.GetValue<int>("KAFKA_MESSAGE_SEND_MAX_RETRIES"),
            };
            string messageQueueTopic = configuration.GetValue<string>("TIMESERIES_QUEUE_TOPIC");
            return new TimeSeriesMessageQueueDispatcher(
                new KafkaDispatcher(new KafkaProducerFactory(kaftaConfiguration)),
                sp.GetRequiredService<IJsonSerializer>(),
                messageQueueTopic);
        });*/
    }
}
