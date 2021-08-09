#!/bin/sh -l

python -m pip install flake8
python setup.py install
python setup.py sdist bdist_wheel

# Generate Python classes from ProtoBuf contracts
python -m pip install -e ./

# Enable debugging of pytest unit tests - works with "Attach container" in launch.json
bash debugz.sh
