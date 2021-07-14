#!/bin/sh -l

python -m pip install flake8
python setup.py install
python setup.py sdist bdist_wheel
python pip install -e ./
bash debugz.sh