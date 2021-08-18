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
using System.Linq;

namespace GreenEnergyHub.TimeSeries.Benchmark
{
    public class CachedArrayBinarySearch : IEnumValueIsDefined
    {
        private readonly Dictionary<Type, Array> _cache = new ();

        public bool CheckValueIsDefined<TEnum>(int value)
        {
            var array = GetArray(typeof(TEnum));
            return Array.BinarySearch(array, value) > -1;
        }

        /// <summary>
        ///     Get a sorted array
        /// </summary>
        /// <param name="enumType">enum type</param>
        /// <returns>Sorted array</returns>
        private Array GetArray(Type enumType)
        {
            if (_cache.ContainsKey(enumType)) return _cache[enumType];

            var arr = Enum.GetValues(enumType).Cast<int>().ToArray();
            Array.Sort(arr);
            _cache[enumType] = arr;

            return _cache[enumType];
        }
    }
}
