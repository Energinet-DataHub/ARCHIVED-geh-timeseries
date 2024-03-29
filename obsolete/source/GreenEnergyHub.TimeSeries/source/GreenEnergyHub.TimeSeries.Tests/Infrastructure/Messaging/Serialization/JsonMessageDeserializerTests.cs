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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Kernel;
using AutoFixture.Xunit2;
using FluentAssertions;
using GreenEnergyHub.Json;
using GreenEnergyHub.Messaging.Transport;
using GreenEnergyHub.TimeSeries.Domain.Messages;
using GreenEnergyHub.TimeSeries.Infrastructure.Messaging.Registration;
using GreenEnergyHub.TimeSeries.Infrastructure.Messaging.Serialization;
using GreenEnergyHub.TimeSeries.TestCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.TimeSeries.Tests.Infrastructure.Messaging.Serialization
{
    [UnitTest]
    public class JsonMessageDeserializerTests
    {
        /// <summary>
        /// Return populated instances of all non-abstract implementations of <see cref="IInboundMessage"/>s
        /// from domain assembly.
        /// </summary>
        public static TheoryData<IInboundMessage> Messages
        {
            get
            {
                var data = new TheoryData<IInboundMessage>();
                var fixture = new Fixture().Customize(new AutoMoqCustomization());
                var domainAssembly = AppDomain
                    .CurrentDomain
                    .GetAssemblies()
                    .Single(a => a.GetName().Name == "GreenEnergyHub.TimeSeries.Domain");
                var messageTypes = domainAssembly
                    .GetTypes()
                    .Where(t => typeof(IMessage).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
                    .ToList();
                var messages = messageTypes
                    .Select(t => (IMessage)fixture.Create(t, new SpecimenContext(fixture)))
                    .ToList();
                messages
                    .ForEach(m => data.Add(m));
                return data;
            }
        }

        /// <summary>
        /// This is an effort to try to make sure that all <see cref="IInboundMessage"/> implementations can be
        /// deserialized by the messaging framework.
        ///
        /// The test suffers from a few weaknesses regarding the following assumptions about the messaging framework:
        /// * It is assumed that transport uses JSON serialized UTF8 strings converted to byte[]
        /// * It is assumed that it uses <see cref="TimeSeries.Core.Json.JsonSerializer.DeserializeAsync"/> for deserialization
        /// </summary>
        [Theory]
        [MemberData(nameof(Messages))]
        public async Task FromBytesAsync_CreatesMessage([NotNull] IInboundMessage expected)
        {
            // Arrange
            var jsonSerializer = GetMessagingDeserializer();
            await using var stream = GetStream(expected, jsonSerializer);

            // Act
            var actual = await jsonSerializer.DeserializeAsync(stream, expected.GetType()).ConfigureAwait(false);

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task FromBytesAsync_WhenCalledWithMessage_UsesJsonDeserializerToReturnResult(
            [NotNull] [Frozen] Mock<IJsonSerializer> serializer,
            [NotNull] byte[] data,
            [NotNull] Mock<IInboundMessage> message,
            [NotNull] JsonMessageDeserializer<IInboundMessage> sut)
        {
            // Arrange
            serializer.Setup(
                    s => s.DeserializeAsync(
                        It.IsAny<Stream>(),
                        It.IsAny<Type>()))
                .Returns(
                    new ValueTask<object>(
                        await Task.FromResult(message.Object)
                            .ConfigureAwait(false)));

            // Act
            var result = await sut.FromBytesAsync(data).ConfigureAwait(false);

            // Assert
            Assert.NotNull(result);
            serializer.Verify(
                s => s.DeserializeAsync(
                    It.IsAny<Stream>(),
                    It.IsAny<Type>()),
                Times.Once);
        }

        /// <summary>
        /// Get stream for the <paramref name="expected"/>.
        /// Assumes that this conversion corresponds with the expectations of the <see cref="MessageExtractor"/>
        /// implementation.
        /// </summary>
        private static MemoryStream GetStream(IInboundMessage expected, IJsonSerializer deserializer)
        {
            var jsonString = deserializer.Serialize(expected);
            var bytes = Encoding.UTF8.GetBytes(jsonString);
            return new MemoryStream(bytes);
        }

        /// <summary>
        /// Try to make sure that we use the same implementation of deserialization that is used in the messaging framework.
        /// </summary>
        private static IJsonSerializer GetMessagingDeserializer()
        {
            var services = new ServiceCollection();
            services.AddMessaging();
            return services
                .BuildServiceProvider()
                .GetRequiredService<IJsonSerializer>();
        }
    }
}
