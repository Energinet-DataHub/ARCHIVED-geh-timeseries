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

namespace GreenEnergyHub.TimeSeries.Infrastructure.Messaging.Serialization.Commands
{
    /// <summary>
    /// Strings used in CIM/XML for elements, namespaces or attributes that we need to
    /// use when parsing a XML document
    ///
    /// This class is specifically used for string specific to the time series command messages
    /// </summary>
    internal static class CimTimeSeriesCommandConstants
    {
        internal const string Namespace = "urn:ebix:org:NotifyValidatedMeasureData:1:0";

        internal const string Series = "Series";

        internal const string Id = "mRID";

        internal const string Product = "product";

        internal const string MeteringPointId = "marketEvaluationPoint.mRID";

        internal const string MeteringPointType = "marketEvaluationPoint.type";

        internal const string SettlementMethod = "marketEvaluationPoint.settlementMethod";

        internal const string RegistrationDateTime = "registration_DateAndOrTime.dateTime";

        internal const string Unit = "measure_Unit.name";

        internal const string Period = "Period";

        internal const string Resolution = "resolution";

        internal const string TimeInterval = "timeInterval";

        internal const string StartDateTime = "start";

        internal const string EndDateTime = "end";

        internal const string Point = "Point";

        internal const string Position = "position";

        internal const string Quantity = "quantity";

        internal const string Quality = "quality";
    }
}
