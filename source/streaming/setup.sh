#!/bin/sh -l

python -m pip install flake8
python setup.py install
python setup.py sdist bdist_wheel
python -m pip install -e ./
#bash debugz.sh  # TODO: What is the purpose of debugz.sh?