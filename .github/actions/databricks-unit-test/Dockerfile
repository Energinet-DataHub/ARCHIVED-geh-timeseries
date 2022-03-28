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

FROM jupyter/pyspark-notebook:45bfe5a474fa

COPY spark-defaults.conf /spark-defaults.conf
COPY entrypoint.sh /entrypoint.sh

USER root

RUN chmod 777 /entrypoint.sh
RUN ["ls", "-lah", "/"]

ENV PYSPARK_SUBMIT_ARGS='--packages io.delta:delta-core_2.12:1.0.0 pyspark-shell'

RUN conda install --quiet --yes --satisfied-skip-solve \
    'rope=0.18.*' 'pytest=6.1.*' 'pylint=2.6.*' 'autopep8=1.5.*' 'configargparse=1.2.3' 'applicationinsights=0.11.9' 'coverage=5.3.*' \
    'pytest-mock=3.5.*' \
    && \
    pip --no-cache-dir install delta-spark pytest-asyncio && \
    conda clean --all -f -y && \
    fix-permissions "${CONDA_DIR}" && \
    fix-permissions "/home/${NB_USER}"

RUN sudo ln -s `(realpath /spark-defaults.conf)` $SPARK_HOME/conf/spark-defaults.conf

ENTRYPOINT ["/entrypoint.sh"]