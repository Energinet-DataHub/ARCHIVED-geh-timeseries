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

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using GreenEnergyHub.Messaging.Transport;
using GreenEnergyHub.Queues.Kafka;
using GreenEnergyHub.TimeSeries.Infrastructure.Messaging;
using GreenEnergyHub.TimeSeries.TestCore;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.TimeSeries.Tests.Infrastructure.Messaging
{
    [UnitTest]
    public class EventHubChannelTests
    {
        [Theory]
        [InlineAutoMoqData]
        public async Task WriteAsync_WhenCalled_CallsKafkaDispatcher(
            [Frozen] [NotNull] Mock<IKafkaDispatcher> dispatcher,
            [Frozen] [NotNull] Mock<IKafkaDispatcher<IOutboundMessage>> dispatcherContainer,
            [NotNull] string topic,
            [NotNull] byte[] data)
        {
            // Arrange
            dispatcherContainer
                .Setup(
                    c => c.Instance)
                .Returns(dispatcher.Object);

            dispatcherContainer
                .Setup(
                    k => k.Topic)
                .Returns(topic);

            string? calledTopic = null;
            dispatcher
                .Setup(
                    d => d.DispatchAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>()))
                .Returns(Task.CompletedTask)
                .Callback<string, string>(
                    (_, t) => calledTopic = t);

            var sut = new TestableEventHubChannel<IOutboundMessage>(dispatcherContainer.Object);

            // Act
            await sut.WriteToAsync(data).ConfigureAwait(false);

            // Assert
            Assert.NotNull(calledTopic);
            Assert.Equal(topic, calledTopic);
        }
    }
}
