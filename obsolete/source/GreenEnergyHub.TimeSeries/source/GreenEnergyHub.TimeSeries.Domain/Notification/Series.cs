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

using System.Collections.Generic;
using NodaTime;

#pragma warning disable 8618

namespace GreenEnergyHub.TimeSeries.Domain.Notification
{
    public class Series
    {
        public Series()
        {
            Points = new List<Point>();
        }

        /// <summary>
        /// Contains a unique ID for the specific times series, provided by the sender.
        /// </summary>
        public string Id { get; set; }

        public string MeteringPointId { get; set; }

        public MeteringPointType MeteringPointType { get; set; }

        public SettlementMethod? SettlementMethod { get; set; }

        public Instant RegistrationDateTime { get; set; }

        public Product Product { get; set; }

        public MeasureUnit Unit { get; set; }

        public TimeSeriesResolution Resolution { get; set; }

        public Instant StartDateTime { get; set; }

        public Instant EndDateTime { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227", Justification = "Ease of use, for example JSON deserialization")]
        public List<Point> Points { get; set; }
    }
}
