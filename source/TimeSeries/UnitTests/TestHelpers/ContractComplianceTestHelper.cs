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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Energinet.DataHub.TimeSeries.UnitTests.TestHelpers;

internal static class ContractComplianceTestHelper
{
    public static async Task VerifyEnumCompliesWithContractAsync<T>(Stream contractStream)
        where T : struct, Enum
    {
        using var streamReader = new StreamReader(contractStream);
        var contractJson = await streamReader.ReadToEndAsync();
        var contractDescription = JsonConvert.DeserializeObject<dynamic>(contractJson)!;

        var expectedLiterals = contractDescription.literals;
        var actualNames = Enum.GetNames<T>();

        // Assert: Number of literals must match.
        actualNames.Length.Should().Be(expectedLiterals.Count);

        foreach (var expectedLiteral in expectedLiterals)
        {
            string expectedName = expectedLiteral.name;
            T expectedValue = expectedLiteral.value;

            // Assert: Lookup literal by name
            var actualLiteral = Enum.Parse<T>(expectedName, true);

            // Assert: Value of literal match
            actualLiteral.Should().Be(expectedValue);
        }
    }

    public static async Task VerifyTypeCompliesWithContractAsync<T>(Stream contractStream)
    {
        static void VerifyTypeCompliesWithContractRecursively(dynamic contractProps, Type actualType)
        {
            var actualProps = actualType
                .GetProperties()
                .ToDictionary(info => info.Name);

            // Assert: Number of props match
            actualProps.Count.Should().Be(contractProps.Count);

            foreach (var expectedProp in contractProps)
            {
                string expectedPropName = expectedProp.name;

                // Assert: Lookup property by name
                var actualProp = actualProps[expectedPropName];

                if (expectedProp.type is JObject)
                {
                    VerifyTypeCompliesWithContractRecursively(
                        expectedProp.type.fields,
                        actualProp.PropertyType);
                }
                else if (expectedProp.type == "array")
                {
                    var arrayType = actualProp.PropertyType.GetElementType();
                    Assert.NotNull(arrayType);

                    VerifyTypeCompliesWithContractRecursively(
                        expectedProp.elementType.fields,
                        arrayType);
                }
                else
                {
                    // Assert: Property types match
                    var actualPropertyType = MapToContractType(actualProp.PropertyType);
                    actualPropertyType.Should().Be((string)expectedProp.type);
                }
            }
        }

        using var streamReader = new StreamReader(contractStream);
        var contractJson = await streamReader.ReadToEndAsync();
        var contractDescription = JsonConvert.DeserializeObject<dynamic>(contractJson)!;

        VerifyTypeCompliesWithContractRecursively(contractDescription.fields, typeof(T));
    }

    private static string MapToContractType(Type propertyType)
    {
        if (propertyType.IsEnum)
            return MapToContractType(Enum.GetUnderlyingType(propertyType));

        if (Nullable.GetUnderlyingType(propertyType) is { } underlyingType)
            return MapToContractType(underlyingType);

        return propertyType.Name switch
        {
            "Int32" => "long",
            "String" => "string",
            "Guid" => "string",
            "Instant" => "timestamp",
            _ => throw new NotImplementedException($"Property type '{propertyType.Name}' not implemented."),
        };
    }
}
