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

using Energinet.DataHub.TimeSeries.Application.Enums;
using NodaTime;

namespace Energinet.DataHub.TimeSeries.Application.Dtos
{
    public class SeriesDto
    {
        public string Id { get; set; }

        public string TransactionId { get; set; }

        public string MeteringPointId { get; set; }

        public MeteringPointType MeteringPointType { get; set; }

        public Instant RegistrationDateTime { get; set; }

        public string Product { get; set; }

        public MeasureUnit MeasureUnit { get; set; }

        public PeriodDto Period { get; set; }
    }
}
