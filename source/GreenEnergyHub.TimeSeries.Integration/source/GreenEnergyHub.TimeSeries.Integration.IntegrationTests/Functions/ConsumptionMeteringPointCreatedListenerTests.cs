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
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using Energinet.DataHub.Core.FunctionApp.TestCommon.EventHub.ListenerMock;
using FluentAssertions;
using GreenEnergyHub.TimeSeries.Integration.IntegrationTests.Assets;
using GreenEnergyHub.TimeSeries.Integration.IntegrationTests.Fixtures;
using GreenEnergyHub.TimeSeries.Integration.IntegrationTests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace GreenEnergyHub.TimeSeries.Integration.IntegrationTests.Functions
{
    [Collection(nameof(AggregationsFunctionAppCollectionFixture))]
    public class ConsumptionMeteringPointCreatedListenerTests_RunAsync : FunctionAppTestBase<AggregationsFunctionAppFixture>
    {
        public ConsumptionMeteringPointCreatedListenerTests_RunAsync(AggregationsFunctionAppFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture, testOutputHelper)
        {
            Fixture.EventHubListener.Reset();
        }

        private TimeSpan DefaultTimeout { get; } = TimeSpan.FromSeconds(10);

        [Fact]
        public async Task When_ReceivingEvent_Then_EventIsProcessed()
        {
            // Arrange
            var message = TestMessages.CreateMpCreatedMessage();

            using var isReceivedEvent = await Fixture.EventHubListener
                .When(e =>
                    e.Properties.Any(p =>
                        p.Key == "SchemaType" && (string)p.Value == "ConsumptionMeteringPointCreated"))
                .VerifyOnceAsync()
                .ConfigureAwait(false);

            // Act
            await Fixture.MPCreatedTopic.SenderClient.SendMessageAsync(message)
                .ConfigureAwait(false);

            // Assert
            var isReceived = isReceivedEvent.Wait(DefaultTimeout);
            isReceived.Should().BeTrue();
        }
    }
}
