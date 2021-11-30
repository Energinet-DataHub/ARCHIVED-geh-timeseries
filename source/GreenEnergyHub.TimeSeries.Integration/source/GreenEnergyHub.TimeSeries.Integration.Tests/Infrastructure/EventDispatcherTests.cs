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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs.Producer;
using GreenEnergyHub.TimeSeries.Integration.Application.IntegrationEvents.MeteringPoints;
using GreenEnergyHub.TimeSeries.Integration.Application.Interfaces;
using GreenEnergyHub.TimeSeries.Integration.Domain;
using GreenEnergyHub.TimeSeries.Integration.Infrastructure;
using GreenEnergyHub.TimeSeries.Integration.Infrastructure.Wrappers;
using Microsoft.Extensions.Logging;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.TimeSeries.Integration.Tests.Infrastructure
{
    [UnitTest]
    public class EventDispatcherTests
    {
        [Fact]
        public async Task DispatchAsync_Called_ShouldCallEventHubProducerClient()
        {
            // Arrange
            var client = new Mock<IEventHubProducerClientWrapper>();
            var logger = new Mock<ILogger<EventHubProducerClientWrapper>>();
            var jsonSerializer = new Mock<IJsonSerializer>();
            var message = new ConsumptionMeteringPointCreatedEvent(
                "MeteringPointId",
                MeteringPointType.Consumption,
                "GridArea",
                SettlementMethod.Flex,
                MeteringMethod.Physical,
                Resolution.Hourly,
                Product.EnergyActive,
                ConnectionState.New,
                Unit.Kwh,
                Instant.FromUnixTimeSeconds(1000));
            var metadata = new Dictionary<string, string>();
            var cancellationToken = CancellationToken.None;

            // Act
            var sut = new EventDispatcher(client.Object, logger.Object, jsonSerializer.Object);
            await sut.DispatchAsync(message, metadata, cancellationToken);

            // Assert
            client.Verify(m => m.CreateEventBatchAsync(It.IsAny<string>(), metadata, cancellationToken), Times.Once);
            client.Verify(m => m.SendAsync(It.IsAny<EventDataBatch>(), cancellationToken), Times.Once);
            logger.Verify(
                m => m.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task DiscpatchAsync_CreatingEventBatchDataFails_ShouldLogAndReThrowException()
        {
            // Arrange
            var client = new Mock<IEventHubProducerClientWrapper>();
            var logger = new Mock<ILogger<EventHubProducerClientWrapper>>();
            var jsonSerializer = new Mock<IJsonSerializer>();
            var message = new ConsumptionMeteringPointCreatedEvent(
                "MeteringPointId",
                MeteringPointType.Consumption,
                "GridArea",
                SettlementMethod.Flex,
                MeteringMethod.Physical,
                Resolution.Hourly,
                Product.EnergyActive,
                ConnectionState.New,
                Unit.Kwh,
                Instant.FromUnixTimeSeconds(1000));

            var cancellationToken = CancellationToken.None;
            client.Setup(m => m.CreateEventBatchAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>())).Throws<Exception>();

            // Act
            var sut = new EventDispatcher(client.Object, logger.Object, jsonSerializer.Object);

            // Assert
            await Assert.ThrowsAsync<Exception>(() => sut.DispatchAsync(message, It.IsAny<Dictionary<string, string>>(), cancellationToken));
            logger.Verify(
                m => m.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
        }
    }
}
