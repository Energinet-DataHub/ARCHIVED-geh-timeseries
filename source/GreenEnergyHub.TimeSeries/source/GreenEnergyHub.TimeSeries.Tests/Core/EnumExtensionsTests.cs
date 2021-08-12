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
using GreenEnergyHub.TimeSeries.Core;
using Xunit;

namespace GreenEnergyHub.TimeSeries.Tests.Core
{
    public enum A
    {
        X = 1,
        Y = 2,
    }

    public enum B
    {
        X = 1,
    }

    public class EnumExtensionsTests
    {
        [Fact]
        public void Cast_Casts_WhenTargetEnumHasValue()
        {
            var actual = A.X.Cast<B>();
            Assert.Equal(B.X, actual);
        }

        [Fact]
        public void Cast_ThrowsInvalidCastException_WhenTargetEnumDoesNotHaveValue()
        {
            Assert.Throws<InvalidCastException>(() => A.Y.Cast<B>());
        }
    }
}
