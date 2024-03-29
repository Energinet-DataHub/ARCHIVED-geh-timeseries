﻿// Copyright 2020 Energinet DataHub A/S
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
using GreenEnergyHub.TimeSeries.Core.Enumeration;
using Xunit;

namespace GreenEnergyHub.TimeSeries.Tests.Core.Enumeration
{
    public enum SupersetEnum
    {
        X = 1,
        OnlyInSuperset = 2,
    }

    public enum SubsetEnum
    {
        X = 1,
    }

    public class EnumExtensionsTests
    {
        [Fact]
        public void Cast_Casts_WhenTargetEnumHasValue()
        {
            var actual = SupersetEnum.X.Cast<SubsetEnum>();
            Assert.Equal(SubsetEnum.X, actual);
        }

        [Fact]
        public void Cast_ThrowsInvalidCastException_WhenTargetEnumDoesNotHaveValue()
        {
            var exception = Assert.Throws<InvalidCastException>(() => SupersetEnum.OnlyInSuperset.Cast<SubsetEnum>());

            // Assert that exception contains name of offending enum value
            Assert.Contains("SupersetEnum.OnlyInSuperset", exception.Message);
        }
    }
}
