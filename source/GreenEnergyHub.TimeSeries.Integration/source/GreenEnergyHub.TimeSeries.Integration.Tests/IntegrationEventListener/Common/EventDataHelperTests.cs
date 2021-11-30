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
using FluentAssertions;
using GreenEnergyHub.TimeSeries.Integration.Application.Extensions;
using GreenEnergyHub.TimeSeries.Integration.Infrastructure.Serialization;
using GreenEnergyHub.TimeSeries.Integration.IntegrationEventListener.Common;
using Microsoft.Azure.Functions.Worker;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.TimeSeries.Integration.Tests.IntegrationEventListener.Common
{
    [UnitTest]
    public class EventDataHelperTests
    {
        private readonly string _expectedEventIdentification = "eventIdentification";
        private readonly string _expectedMessageType = "messageType";
        private readonly string _expectedOperationCorrelationId = "operationCorrelationId";
        private readonly int _expectedMessageVersion = 1;
        private readonly Instant _expectedOperationTimestamp = Instant.FromUtc(2020, 1, 1, 0, 0);
        private readonly string _expectedDomain = "domain";

        [Fact]
        public void GetEventMetaData_ThrowsArgumentNullException_WhenContextIsNull()
        {
            var sut = new EventDataHelper(new JsonSerializer());

            Assert.Throws<ArgumentNullException>(() => sut.GetEventMetaData(null));
        }

        [Fact]
        public void GetEventMetaData_ThrowsInvalidOperationException_WhenUserPropertiesDoesNotExist()
        {
            var sut = new EventDataHelper(new JsonSerializer());

            var context = GetContext();

            Assert.Throws<InvalidOperationException>(() => sut.GetEventMetaData(context.Object));
        }

        [Fact]
        public void GetEventMetaData_ThrowsArgumentException_WhenEventIdentification_IsNotSet()
        {
            var sut = new EventDataHelper(new JsonSerializer());

            var context = GetContext(EventMetadataToJson(SetEventMetadata()));

            var exception = Assert.Throws<ArgumentException>(() => sut.GetEventMetaData(context.Object));

            Assert.Equal("EventIdentification is not set", exception.Message);
        }

        [Fact]
        public void GetEventMetaData_ThrowsArgumentException_WhenMessageType_IsNotSet()
        {
            var sut = new EventDataHelper(new JsonSerializer());

            var context = GetContext(EventMetadataToJson(SetEventMetadata(
                eventIdentification: _expectedEventIdentification)));

            var exception = Assert.Throws<ArgumentException>(() => sut.GetEventMetaData(context.Object));

            Assert.Equal("MessageType is not set", exception.Message);
        }

        [Fact]
        public void GetEventMetaData_ThrowsArgumentException_WhenOperationCorrelationId_IsNotSet()
        {
            var sut = new EventDataHelper(new JsonSerializer());

            var context = GetContext(EventMetadataToJson(SetEventMetadata(
                eventIdentification: _expectedEventIdentification,
                messageType: _expectedMessageType)));

            var exception = Assert.Throws<ArgumentException>(() => sut.GetEventMetaData(context.Object));

            Assert.Equal("OperationCorrelationId is not set", exception.Message);
        }

        [Fact]
        public void GetEventMetaData_ThrowsArgumentException_WhenMessageVersion_IsNotSet()
        {
            var sut = new EventDataHelper(new JsonSerializer());

            var context = GetContext(EventMetadataToJson(SetEventMetadata(
                eventIdentification: _expectedEventIdentification,
                messageType: _expectedMessageType,
                operationCorrelationId: _expectedOperationCorrelationId)));

            var exception = Assert.Throws<ArgumentException>(() => sut.GetEventMetaData(context.Object));

            Assert.Equal("MessageVersion is not set", exception.Message);
        }

        [Fact]
        public void GetEventMetaData_ThrowsArgumentException_WhenOperationTimestamp_IsMinValue()
        {
            var sut = new EventDataHelper(new JsonSerializer());

            var context = GetContext(EventMetadataToJson(SetEventMetadata(
                eventIdentification: _expectedEventIdentification,
                messageType: _expectedMessageType,
                operationCorrelationId: _expectedOperationCorrelationId,
                messageVersion: _expectedMessageVersion)));

            var exception = Assert.Throws<ArgumentException>(() => sut.GetEventMetaData(context.Object));

            Assert.Equal("OperationTimestamp is not set", exception.Message);
        }

        [Fact]
        public void GetEventMetaData_ReturnsEventMetadataObject()
        {
            var sut = new EventDataHelper(new JsonSerializer());

            var expectedJson = EventMetadataToJson(SetEventMetadata(
                eventIdentification: _expectedEventIdentification,
                messageType: _expectedMessageType,
                operationCorrelationId: _expectedOperationCorrelationId,
                messageVersion: _expectedMessageVersion,
                operationTimestamp: _expectedOperationTimestamp));

            var expected = new JsonSerializer().Deserialize<EventMetadata>(expectedJson);

            var context = GetContext(expectedJson);

            var result = sut.GetEventMetaData(context.Object);

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void GetEventHubMetaData_ThrowsArgumentNullException_WhenMetadata_IsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new EventDataHelper(new JsonSerializer()).GetEventhubMetaData(null, null));
        }

        [Fact]
        public void GetEventHubMetaData_ReturnsDictionary()
        {
            var metadata = SetEventMetadata(
                eventIdentification: _expectedEventIdentification,
                messageType: _expectedMessageType,
                operationCorrelationId: _expectedOperationCorrelationId,
                messageVersion: _expectedMessageVersion,
                operationTimestamp: _expectedOperationTimestamp);

            var expected = new Dictionary<string, string>
            {
                { "event_id", _expectedEventIdentification },
                { "processed_date", _expectedOperationTimestamp.ToIso8601GeneralString() },
                { "event_name", _expectedMessageType },
                { "domain", _expectedDomain },
            };

            var result = new EventDataHelper(new JsonSerializer()).GetEventhubMetaData(metadata, _expectedDomain);

            result.Should().BeEquivalentTo(expected);
        }

        private Mock<FunctionContext> GetContext(string metadata = null)
        {
            var context = new Mock<FunctionContext>();
            var bindingContext = new Mock<BindingContext>();
            var dict = new Dictionary<string, object?>();

            if (metadata != null)
            {
                dict.Add("UserProperties", metadata);
            }

            bindingContext.Setup(x => x.BindingData).Returns(dict);
            context.Setup(x => x.BindingContext).Returns(bindingContext.Object);

            return context;
        }

        private EventMetadata SetEventMetadata(
            Instant? operationTimestamp = null,
            int messageVersion = 0,
            string messageType = "",
            string eventIdentification = "",
            string operationCorrelationId = "")
        {
            return new EventMetadata(messageVersion, messageType, eventIdentification, operationTimestamp ?? Instant.MinValue, operationCorrelationId);
        }

        private string EventMetadataToJson(EventMetadata metadata)
        {
            return new JsonSerializer().Serialize(metadata);
        }
    }
}
