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
using System.Threading;

namespace GreenEnergyHub.TimeSeries.Core
{
    internal class KeyTypeHashSet
    {
        private static int _typeIndex;
        private readonly object _resizeLock = new ();
        private HashSet<int>[] _cache = new HashSet<int>[64];

        public bool CheckValueIsDefined<TEnum>(int value)
        {
            var index = KeyType<TEnum>.Index;
            return (_cache[index] ??= GetEnumValues<TEnum>(index)).Contains(value);
        }

        private HashSet<int> GetEnumValues<T>(int index)
        {
            lock (_resizeLock)
            {
                if (index >= _cache.Length) Array.Resize(ref _cache, index + 64);
            }

            var array = Enum.GetValues(typeof(T));
            return new HashSet<int>(array.Cast<int>());
        }

        private static class KeyType<T>
        {
            internal static readonly int Index = Interlocked.Increment(ref _typeIndex);
        }
    }
}
