#!/bin/sh -l

cd ./source/streaming
python setup.py install
# Generate Python classes from ProtoBuf contracts
python -m pip install -e ./
# python coverage-threshold install
pip install coverage-threshold
coverage run --branch -m pytest tests/
# Create data for threshold evaluation
coverage json
# Create human reader friendly HTML report
coverage html
coverage-threshold