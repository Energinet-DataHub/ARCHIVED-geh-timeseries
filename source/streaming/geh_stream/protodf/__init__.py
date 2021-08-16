# Copyright 2020 Energinet DataHub A/S
#
# Licensed under the Apache License, Version 2.0 (the "License2");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

"""
TODO

This file is from https://raw.githubusercontent.com/aroch/protobuf-dataframe/ and should be
pushed back with a pull-request when improved to satisfy the needs in geh-timeseries.
"""

from collections.abc import Mapping
from pyspark.sql.types import Row, StringType, StructType, StructField, LongType, DoubleType, FloatType, IntegerType, \
    BooleanType, BinaryType, ArrayType

# https://developers.google.com/protocol-buffers/docs/reference/cpp/google.protobuf.descriptor#FieldDescriptor.Type.details
possible_types = {
    1: lambda t: DoubleType(),
    2: lambda t: FloatType(),
    3: lambda t: LongType(),
    4: lambda t: LongType(),
    5: lambda t: IntegerType(),
    8: lambda t: BooleanType(),
    9: lambda t: StringType(),
    11: lambda t: schema_for(t.message_type),
    12: lambda t: BinaryType(),
    13: lambda t: LongType(),
    14: lambda t: StructType(
        [
            StructField("name", StringType(), True),
            StructField("number", IntegerType(), True)
        ]),  # enum type
    15: lambda t: IntegerType()
}


def schema_for(descriptor):
    if descriptor is None:
        return None

    struct_type = StructType()
    for field_descriptor in sorted(descriptor.fields, key=lambda x: x.name):
        struct_type.add(
            field_descriptor.name,
            __type_for(field_descriptor),
            field_descriptor.label != field_descriptor.LABEL_REQUIRED
        )

    return struct_type


def __type_for(field_descriptor):
    get_type = possible_types.get(field_descriptor.type, lambda t: StringType())
    if field_descriptor.label == field_descriptor.LABEL_REPEATED:
        return ArrayType(get_type(field_descriptor))
    return get_type(field_descriptor)


def message_to_row(descriptor, message):
    field_map = {}
    for field_tuple in message.ListFields():
        field_map[field_tuple[0].name] = field_tuple[1]

    values = {}
    for field_descriptor in sorted(descriptor.fields, key=lambda x: x.name):
        values[field_descriptor.name] = __get_field_value(field_descriptor, field_map)

    return Row(**values)


def __get_field_value(field_descriptor, field_map):
    if field_descriptor.name not in field_map:
        return None

    if isinstance(field_map[field_descriptor.name], Mapping):
        return [Row(**{'key': k, 'value': __to_row_data(field_descriptor.message_type.fields_by_name['value'], v)}) for
                k, v in field_map[field_descriptor.name].items()]

    if field_descriptor.label == field_descriptor.LABEL_REPEATED:
        return [__to_row_data(field_descriptor, data) for data in field_map[field_descriptor.name]]

    return __to_row_data(field_descriptor, field_map[field_descriptor.name])


def __to_row_data(field_descriptor, data):
    if field_descriptor.message_type is not None:
        return message_to_row(field_descriptor.message_type, data)
    if field_descriptor.type == field_descriptor.TYPE_ENUM:
        return __enum_to_row_data(field_descriptor, data)
    return data


def __enum_to_row_data(field_descriptor, data):
    try:
        enum_value = field_descriptor.enum_type.values_by_number[data]
        return {"name": enum_value.name, "number": enum_value.number}
    except KeyError:
        raise Exception("Invalid number " + str(data) + " for enum '" + field_descriptor.enum_type.full_name + "'")
