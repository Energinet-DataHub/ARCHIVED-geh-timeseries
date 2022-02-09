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

using GreenEnergyHub.TimeSeries.Domain.Notification;

namespace GreenEnergyHub.TimeSeries.Infrastructure.Messaging.Serialization.Commands
{
    public static class TimeSeriesResolutionMapper
    {
        public static TimeSeriesResolution Map(string value)
        {
            return value switch
            {
                "PT15M" => TimeSeriesResolution.QuarterOfHour,
                "PT1H" => TimeSeriesResolution.Hour,
                "P1D" => TimeSeriesResolution.Day,
                "P1M" => TimeSeriesResolution.Month,
                _ => TimeSeriesResolution.Unknown,
            };
        }

        public static string Map(TimeSeriesResolution resolution)
        {
            return resolution switch
            {
                TimeSeriesResolution.QuarterOfHour => "PT15M",
                TimeSeriesResolution.Hour => "PT1H",
                TimeSeriesResolution.Day => "P1D",
                TimeSeriesResolution.Month => "P1M",
                _ => string.Empty,
            };
        }
    }
}
