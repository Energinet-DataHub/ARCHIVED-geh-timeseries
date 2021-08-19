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
using GreenEnergyHub.TimeSeries.Domain.MarketDocument;
using GreenEnergyHub.TimeSeries.Domain.Notification;
using GreenEnergyHub.TimeSeries.TestCore.Protobuf;
using Xunit;
using Xunit.Categories;
using proto = GreenEnergyHub.TimeSeries.Contracts.Internal;

namespace GreenEnergyHub.TimeSeries.Tests.Infrastructure.Internal
{
    [UnitTest]
    public class TimeSeriesProtobufEnumTests
    {
        [Fact]
        public void ProductContract_ShouldBeSubsetOfDomainEnum()
        {
            ProtobufAssert.ContractEnumIsSubSet<proto.Product, Product>();
        }

        [Fact]
        public void QualityContract_ShouldBeSubsetOfDomainEnum()
        {
            ProtobufAssert.ContractEnumIsSubSet<proto.Quality, Quality>();
        }

        [Fact]
        public void ResolutionContract_ShouldBeSubsetOfDomainEnum()
        {
            ProtobufAssert.ContractEnumIsSubSet<proto.Resolution, TimeSeriesResolution>();
        }

        [Fact]
        public void MeasureUnitContract_ShouldBeSubsetOfDomainEnum()
        {
            ProtobufAssert.ContractEnumIsSubSet<proto.MeasureUnit, MeasureUnit>();
        }

        [Fact]
        public void SettlementMethodContract_ShouldBeSubsetOfDomainEnum()
        {
            // TODO: Should we add support for ignoring XX_NULL enum values in ProtobufAssert.ContractEnumIsSubSet?
            // ProtobufAssert.ContractEnumIsSubSet<proto.SettlementMethod, SettlementMethod>();
            var settlementMethods = Enum
                .GetValues<proto.SettlementMethod>()
                .Where(v => v != proto.SettlementMethod.SmNotSet);
            foreach (var protoSettlementMethod in settlementMethods)
            {
                Assert.True(Enum.IsDefined(typeof(SettlementMethod), (int)(object)protoSettlementMethod));
            }
        }

        [Fact]
        public void BusinessProcessRoleContract_ShouldBeSubsetOfDomainEnum()
        {
            ProtobufAssert.ContractEnumIsSubSet<proto.BusinessProcessRole, MarketParticipantRole>();
        }

        [Fact]
        public void BusinessReasonCodeContract_ShouldBeSubsetOfDomainEnum()
        {
            ProtobufAssert.ContractEnumIsSubSet<proto.BusinessReasonCode, BusinessReasonCode>();
        }

        [Fact]
        public void MeteringPointTypeContract_ShouldBeSubsetOfDomainEnum()
        {
            ProtobufAssert.ContractEnumIsSubSet<proto.MeteringPointType, MeteringPointType>();
        }
    }
}
