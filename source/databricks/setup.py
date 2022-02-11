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
from setuptools import setup, find_packages

# File 'VERSION' is created by pipeline. If executed manual it must be created manually.
__version__ = ""
with open('VERSION') as version_file:
    __version__ = version_file.read().strip()

setup(name='geh_stream',
      version=__version__,
      description='Tools for timeseries streaming',
      long_description="",
      long_description_content_type='text/markdown',
      license='MIT',
      packages=find_packages()
      )