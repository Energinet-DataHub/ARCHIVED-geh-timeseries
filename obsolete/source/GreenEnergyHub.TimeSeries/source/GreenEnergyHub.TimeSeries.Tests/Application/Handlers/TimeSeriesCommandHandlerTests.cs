﻿// Copyright 2020 Energinet DataHub A/S
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
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using GreenEnergyHub.TimeSeries.Application;
using GreenEnergyHub.TimeSeries.Application.Handlers;
using GreenEnergyHub.TimeSeries.Domain.Notification;
using GreenEnergyHub.TimeSeries.Infrastructure.Messaging;
using GreenEnergyHub.TimeSeries.TestCore;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.TimeSeries.Tests.Application.Handlers
{
    [UnitTest]
    public class TimeSeriesCommandHandlerTests
    {
        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_WhenCalled_DispatchesCommandAndReturnsSuccessfulResult(
            [Frozen] [NotNull] Mock<IMessageDispatcher<TimeSeriesCommand>> dispatcher,
            [NotNull] TimeSeriesCommand command,
            [NotNull] TimeSeriesCommandHandler sut)
        {
            // Act
            var result = await sut.HandleAsync(command).ConfigureAwait(false);

            // Assert
            dispatcher.Verify(
                d => d.DispatchAsync(
                    command,
                    It.IsAny<CancellationToken>()),
                Times.Once);
            Assert.NotNull(result);
            Assert.True(result.IsSucceeded);
        }
    }
}
