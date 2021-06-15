# PySpark custom action

This action allows you to execute code, written for Spark, Databricks or other similar engines.
It is based on [this](https://docs.github.com/en/free-pro-team@latest/actions/creating-actions/creating-a-docker-container-action) tutorial and running in [jupyter/pyspark-notebook](https://hub.docker.com/r/jupyter/pyspark-notebook) container.

## Usage

In private repository:

```yaml
- name: Unit tests
  uses: ./.github/actions/databricks-unit-test
```

## Script

```bash
cd ./source/streaming/processing/tests
pytest
```
