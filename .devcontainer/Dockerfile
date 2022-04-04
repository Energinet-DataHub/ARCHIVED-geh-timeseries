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

FROM jupyter/pyspark-notebook:spark-3.1.2

SHELL ["/bin/bash", "-o", "pipefail", "-c"]
#COPY entrypoint.sh /entrypoint.sh

USER root
# RUN chmod 777 /entrypoint.sh
COPY spark-defaults.conf $SPARK_HOME/conf/spark-defaults.conf


# Install Azurite (support wasb-protocol storage for Delta tables in integration tests)
RUN apt-get update; \
    apt-get install -y npm && npm install -g n && n lts && hash -r && npm install -g azurite

RUN mamba install --quiet --yes --satisfied-skip-solve \
    'pyarrow=2.0.*' 'rope=0.18.*' 'pytest=6.1.*' 'autopep8=1.5.*' 'configargparse=1.2.3'  \
    'coverage=5.3.*' 'azure-storage-blob=12.8.*' 'pytest-mock=3.5.*' \
    && \
    # conda install --quiet --yes --satisfied-skip-solve -c anaconda 'protobuf=3.14.*' && \
    pip --no-cache-dir install pyspelling azure-eventhub coverage-threshold ptvsd azure-servicebus protodf delta-spark pytest-asyncio flake8 black && \
    mamba clean --all -f -y && \
    fix-permissions "${CONDA_DIR}" && \
    fix-permissions "/home/${NB_USER}"

ENV PATH=$SPARK_HOME/bin:$HADOOP_HOME/bin:$PATH \
    PYTHONPATH="${SPARK_HOME}/python:${SPARK_HOME}/python/lib/py4j-${py4j_version}-src.zip" \
    SPARK_OPTS="--driver-java-options=-Xms1024M --driver-java-options=-Xmx4096M --driver-java-options=-Dlog4j.logLevel=info"
