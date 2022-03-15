#FKT 24 Feb 2021: Fixed to specific version because of build errors
FROM jupyter/scipy-notebook:016833b15ceb

SHELL ["/bin/bash", "-o", "pipefail", "-c"]

USER root

ARG openjdk_version="8"

ARG hadoop_version="3.3.2"

ARG spark_version="3.1.2"

ARG py4j_version="0.10.9"

ENV APACHE_SPARK_VERSION="${spark_version}" \
    HADOOP_VERSION="${hadoop_version}"

RUN apt-get -y update && \
    apt-get install --no-install-recommends -y \
    "openjdk-${openjdk_version}-jre-headless" \
    "openssh-client" "git" "bash-completion" \
    "aspell" "aspell-en" \
    "flake8" \
    "ca-certificates-java" && \
    apt-get clean && rm -rf /var/lib/apt/lists/*

ENV JAVA_HOME=/usr/lib/jvm/java-${openjdk_version}-openjdk-amd64 \
    PATH=$JAVA_HOME/bin:$PATH

# Spark and Hadoop installation
WORKDIR /tmp

RUN wget -q $(wget -qO- https://www.apache.org/dyn/closer.cgi/hadoop/common/hadoop-${HADOOP_VERSION}/hadoop-${HADOOP_VERSION}.tar.gz\?as_json | \
    python -c "import sys, json; content=json.load(sys.stdin); print(content['preferred']+content['path_info'])") && \
    tar xzf "hadoop-${HADOOP_VERSION}.tar.gz" -C /usr/local --owner root --group root --no-same-owner && \
    rm "hadoop-${HADOOP_VERSION}.tar.gz"

RUN wget -q $(wget -qO- https://www.apache.org/dyn/closer.lua/spark/spark-${APACHE_SPARK_VERSION}/spark-${APACHE_SPARK_VERSION}-bin-without-hadoop.tgz\?as_json | \
    python -c "import sys, json; content=json.load(sys.stdin); print(content['preferred']+content['path_info'])") && \
    tar xzf "spark-${APACHE_SPARK_VERSION}-bin-without-hadoop.tgz" -C /usr/local --owner root --group root --no-same-owner && \
    rm "spark-${APACHE_SPARK_VERSION}-bin-without-hadoop.tgz"


WORKDIR /usr/local
RUN ln -s "spark-${APACHE_SPARK_VERSION}-bin-without-hadoop" spark && \
    ln -s "hadoop-${HADOOP_VERSION}" hadoop

# Configure Spark
ENV SPARK_HOME=/usr/local/spark \
    HADOOP_HOME=/usr/local/hadoop
    #PYSPARK_SUBMIT_ARGS='--packages io.delta:delta-core_2.12:1.0.0,org.apache.hadoop:hadoop-azure:3.3.2,org.apache.hadoop:hadoop-client:3.3.2 pyspark-shell'

ENV PATH=$SPARK_HOME/bin:$HADOOP_HOME/bin:$PATH \
    PYTHONPATH="${SPARK_HOME}/python:${SPARK_HOME}/python/lib/py4j-${py4j_version}-src.zip" \
    SPARK_OPTS="--driver-java-options=-Xms1024M --driver-java-options=-Xmx4096M --driver-java-options=-Dlog4j.logLevel=info"


RUN wget -P $SPARK_HOME/jars https://repo1.maven.org/maven2/io/delta/delta-core_2.12/1.0.1/delta-core_2.12-1.0.1.jar

RUN wget -P $SPARK_HOME/jars https://repo1.maven.org/maven2/org/apache/hadoop/hadoop-azure/3.3.2/hadoop-azure-3.3.2.jar

USER $NB_UID

ENV PATH=$JAVA_HOME/bin:$SPARK_HOME/bin:$HADOOP_HOME/bin:$PATH
RUN echo "export SPARK_DIST_CLASSPATH=$(hadoop classpath)" >> $HOME/.bashrc

WORKDIR $HOME

# [Optional] Uncomment to install a different version of Python than the default
# RUN conda install -y python=3.5 \
#     && pip install --no-cache-dir pipx \
#     && pipx reinstall-all

RUN conda install --quiet --yes --satisfied-skip-solve \
    'pyarrow=2.0.*' 'rope=0.18.*' 'pytest=6.1.*' 'autopep8=1.5.*' 'configargparse=1.2.3' 'applicationinsights=0.11.9' \
    'coverage=5.3.*' 'azure-storage-blob=12.8.*' 'pytest-mock=3.5.*' \
    && \
    conda install --quiet --yes --satisfied-skip-solve -c anaconda 'protobuf=3.14.*' && \
    pip --no-cache-dir install pyspelling azure-eventhub coverage-threshold ptvsd azure-servicebus protodf delta-spark && \
    conda clean --all -f -y && \
    fix-permissions "${CONDA_DIR}" && \
    fix-permissions "/home/${NB_USER}"

EXPOSE 3000