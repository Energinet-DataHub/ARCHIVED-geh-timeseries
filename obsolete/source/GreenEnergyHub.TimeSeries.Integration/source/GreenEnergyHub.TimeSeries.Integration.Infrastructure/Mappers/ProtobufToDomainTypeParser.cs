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
using Energinet.DataHub.MeteringPoints.IntegrationEventContracts;
using Google.Protobuf.WellKnownTypes;
using GreenEnergyHub.TimeSeries.Integration.Domain;
using NodaTime;

namespace GreenEnergyHub.TimeSeries.Integration.Infrastructure.Mappers
{
    public static class ProtobufToDomainTypeParser
    {
        public static Unit ParseUnitType(ConsumptionMeteringPointCreated.Types.UnitType unitType)
        {
            return unitType switch
            {
                ConsumptionMeteringPointCreated.Types.UnitType.UtKwh => Unit.Kwh,
                _ => throw new ArgumentOutOfRangeException(nameof(unitType), unitType, "Could not parse argument")
            };
        }

        public static Product ParseProduct(ConsumptionMeteringPointCreated.Types.ProductType product)
        {
            return product switch
            {
                ConsumptionMeteringPointCreated.Types.ProductType.PtEnergyactive => Product.EnergyActive,
                _ => throw new ArgumentOutOfRangeException(nameof(product), product, "Could not parse argument")
            };
        }

        public static ConnectionState ParseConnectionState(ConsumptionMeteringPointCreated.Types.ConnectionState connectionState)
        {
            return connectionState switch
            {
                ConsumptionMeteringPointCreated.Types.ConnectionState.CsNew => ConnectionState.New,
                _ => throw new ArgumentOutOfRangeException(nameof(connectionState), connectionState, "Could not parse argument")
            };
        }

        public static Resolution ParseMeterReadingPeriodicity(ConsumptionMeteringPointCreated.Types.MeterReadingPeriodicity meterReadingPeriodicity)
        {
            return meterReadingPeriodicity switch
            {
                ConsumptionMeteringPointCreated.Types.MeterReadingPeriodicity.MrpHourly => Resolution.Hourly,
                ConsumptionMeteringPointCreated.Types.MeterReadingPeriodicity.MrpQuarterly => Resolution.Quarterly,
                _ => throw new ArgumentOutOfRangeException(nameof(meterReadingPeriodicity), meterReadingPeriodicity, "Could not parse argument")
            };
        }

        public static MeteringMethod ParseMeteringMethod(ConsumptionMeteringPointCreated.Types.MeteringMethod meteringMethod)
        {
            return meteringMethod switch
            {
                ConsumptionMeteringPointCreated.Types.MeteringMethod.MmCalculated => MeteringMethod.Calculated,
                ConsumptionMeteringPointCreated.Types.MeteringMethod.MmPhysical => MeteringMethod.Physical,
                ConsumptionMeteringPointCreated.Types.MeteringMethod.MmVirtual => MeteringMethod.Virtual,
                _ => throw new ArgumentOutOfRangeException(nameof(meteringMethod), meteringMethod, "Could not parse argument")
            };
        }

        public static SettlementMethod ParseSettlementMethod(ConsumptionMeteringPointCreated.Types.SettlementMethod settlementMethod)
        {
            return settlementMethod switch
            {
                ConsumptionMeteringPointCreated.Types.SettlementMethod.SmFlex => SettlementMethod.Flex,
                ConsumptionMeteringPointCreated.Types.SettlementMethod.SmProfiled => SettlementMethod.Profiled,
                ConsumptionMeteringPointCreated.Types.SettlementMethod.SmNonprofiled => SettlementMethod.NonProfiled,
                _ => throw new ArgumentOutOfRangeException(nameof(settlementMethod), settlementMethod, "Could not parse argument")
            };
        }

        public static Instant ParseEffectiveDate(Timestamp effectiveDate)
        {
            if (effectiveDate == null) throw new ArgumentNullException(nameof(effectiveDate));
            var time = Instant.FromUnixTimeSeconds(effectiveDate.Seconds);
            return time.PlusNanoseconds(effectiveDate.Nanos);
        }
    }
}
