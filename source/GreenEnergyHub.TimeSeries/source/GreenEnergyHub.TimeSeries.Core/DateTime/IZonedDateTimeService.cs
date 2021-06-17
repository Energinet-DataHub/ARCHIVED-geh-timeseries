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

using NodaTime;

namespace GreenEnergyHub.TimeSeries.Core.DateTime
{
    /// <summary>
    /// Interface for service that provides help with handling time zones in NodaTime
    /// </summary>
    public interface IZonedDateTimeService
    {
        /// <summary>
        /// Retrieve the current zoned date and time
        /// </summary>
        /// <returns>The ZonedDateTime that represents the current time</returns>
        ZonedDateTime GetZonedDateTimeNow();

        /// <summary>
        /// Converts a local date and time to a zoned date and time
        /// </summary>
        /// <param name="localDateTime">The local date and time to convert</param>
        /// <param name="strategy">The strategy used to solve ambiguity</param>
        /// <returns>ZonedDateTime representing the provided local date and time</returns>
        ZonedDateTime GetZonedDateTime(LocalDateTime localDateTime, ResolutionStrategy strategy);

        /// <summary>
        /// Converts an instant to a zoned date and time
        /// </summary>
        /// <param name="instant">The instant to convert</param>
        /// <returns>Zoned date and time representing the provided instant</returns>
        ZonedDateTime GetZonedDateTime(Instant instant);
    }
}
