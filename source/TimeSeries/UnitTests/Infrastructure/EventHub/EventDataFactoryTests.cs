//  Copyright 2020 Energinet DataHub A/S
//
//  Licensed under the Apache License, Version 2.0 (the "License2");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.

using System.Diagnostics.CodeAnalysis;
using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.TimeSeries.Infrastructure.EventHub;
using FluentAssertions;
using Moq;
using Xunit;

namespace Energinet.DataHub.TimeSeries.UnitTests.Infrastructure.EventHub
{
    public class EventDataFactoryTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void Create_WhenCalled_ReturnedEventDataHasCorrelationIdSet(
            string correlationId,
            [NotNull] Mock<ICorrelationContext> correlationContext,
            [NotNull] EventDataFactory sut)
        {
            correlationContext.Setup(context => context.Id).Returns(correlationId);
            var actual = sut.Create(System.Array.Empty<byte>());
            actual.CorrelationId.Should().Be(correlationId);
        }
    }
}
