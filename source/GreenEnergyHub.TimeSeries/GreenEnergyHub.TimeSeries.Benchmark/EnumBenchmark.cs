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

using BenchmarkDotNet.Attributes;

namespace GreenEnergyHub.TimeSeries.Benchmark
{
    public class EnumBenchmark
    {
        private readonly IEnumValueIsDefined _cachedArray = new CachedArrayBinarySearch();
        private readonly IEnumValueIsDefined _cachedHashset = new CachedHashset();
        private readonly IEnumValueIsDefined _defaultImplementation = new DefaultImplementation();
        private readonly IEnumValueIsDefined _keyTypeArraySearch = new KeyTypeArraySearch();
        private readonly IEnumValueIsDefined _keyTypeHashSet = new KeyTypeHashSet();

        [Benchmark(Baseline = true, Description = nameof(Benchmark.DefaultImplementation))]
        public bool DefaultImplementation()
        {
            return _defaultImplementation.CheckValueIsDefined<Pet>(9);
        }

        [Benchmark(Description = nameof(Benchmark.CachedHashset))]
        public bool CachedHashset()
        {
            return _cachedHashset.CheckValueIsDefined<Pet>(9);
        }

        [Benchmark(Description = nameof(CachedArrayBinarySearch))]
        public bool CachedArray()
        {
            return _cachedArray.CheckValueIsDefined<Pet>(9);
        }

        [Benchmark(Description = nameof(KeyTypeHashSet))]
        public bool KeyTypeHAshSet()
        {
            return _keyTypeHashSet.CheckValueIsDefined<Pet>(9);
        }

        [Benchmark(Description = nameof(Benchmark.KeyTypeArraySearch))]
        public bool KeyTypeArraySearch()
        {
            return _keyTypeArraySearch.CheckValueIsDefined<Pet>(9);
        }
    }
}
