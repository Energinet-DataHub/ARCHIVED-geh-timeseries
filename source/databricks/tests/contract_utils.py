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

import json


def read_contract(path):
    jsonFile = open(path)
    return json.load(jsonFile)


def assert_contract_matches_schema(contract_path, schema):
    def assert_schema_recursively(expected_schema, actual_schema):

        # Assert: Schema and contract has the same number of fields
        assert len(actual_schema) == len(expected_schema)

        # Assert: Schema matches contract
        for expected_field in expected_schema:
            actual_field = next(
                (f for f in actual_schema if expected_field["name"] == f["name"]),
                None,
            )

            assert (
                actual_field is not None
            ), f"""Actual schema is missing field '{expected_field["name"]}' from contract."""

            expected_field_type = expected_field["type"]
            actual_field_type = actual_field["type"]

            if isinstance(expected_field_type, dict):
                assert_schema_recursively(
                    expected_field_type["fields"], actual_field_type["fields"]
                )

            elif expected_field_type == "array":
                assert_schema_recursively(
                    expected_field["elementType"]["fields"],
                    actual_field_type["elementType"]["fields"],
                )

            else:
                assert (
                    actual_field_type == expected_field_type
                ), f"""Actual type ({actual_field_type}) of field {expected_field["name"]} does not match the expected type ({expected_field["type"]})."""

    expected_schema = read_contract(contract_path)["fields"]
    actual_schema = json.loads(schema.json())["fields"]

    assert_schema_recursively(expected_schema, actual_schema)


def assert_codelist_matches_contract(codelist, contract_path):
    supported_literals = read_contract(contract_path)["literals"]
    literals = [member for member in codelist]

    # Assert: The enum is a subset of contract
    assert len(literals) <= len(supported_literals)

    # Assert: The enum values must match contract
    for literal in literals:
        supported_arg = next(
            (x for x in supported_literals if literal.name == x["name"]), None
        )
        if supported_arg is None:
            raise Exception(f"Literal '{literal.name}' does not exist in contract.")
        assert literal.value == supported_arg["value"]
