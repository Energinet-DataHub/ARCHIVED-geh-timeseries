# Python Environment

## Set up development environment

Follow this guide to set up your Time Series streaming development environment:

* Install [Python](https://www.python.org/downloads)
* Install Docker for Windows and use WSL based engine
* Clone `geh-timeseries` repository
* Open `geh-timeseries` folder in Visual Studio Code
* Install extensions:
    * Azure Account
	* Azure Event Hub Explorer
	* Docker
	* Remote - Containers
	* GitLens
	* indent-rainbow
	* Python (with Jupyter and Pylance)
	* vscode-icons or Material Icon Theme
    * Test Explorer UI
* Setup your environment:
    * Make sure you have deployed Time Series infrastructure to Azure development environment (preferably developer sandbox), by following this [guide](https://github.com/Energinet-DataHub/geh-timeseries/blob/main/build/infrastructure/README.md)
    * Navigate to the `.vscode` folder in the `geh-timeseries` folder and make your own copy of `launch.json.sample` and `settings.json.sample` witout the `.sample` extension
    * No need to change settings in `settings.json`
    * Insert your own settings in `launch.json`:
        * storage-account-name: Account name of your `data` data storage
        * storage-account-key: Account key of your `data` data storage
        * storage-container-name: Storage container name in your `data` data storage
        * master-data-path: Path to master-data.csv in storage container (master-data/master-data.csv)
        * input-eh-connection-string: Connectionstring to `evhnm-received-queue-` Event Hub queue including EntityPath (`;EntityPath=evh-received-queue`)
        * telemetry-instrumentation-key: Instrumentation key (GUID) to `appi-` Time Series Application Insights    
* Use `Ctrl + Shift + p` and run `Remote-Containers: Open Folder in Container...` command and select the `geh-timeseries` folder

Debug or run streaming:
* Hit `F5`
* Hit `Ctrl + F5`

Tips and tricks for developing in Visual Studio Code:
* [Navigation](https://code.visualstudio.com/docs/editor/editingevolved)
* [IntelliSense](https://code.visualstudio.com/docs/editor/intellisense)
* [Debugging](https://code.visualstudio.com/docs/editor/debugging)

## Source Code

Source code is mainly located in folder `geh_stream`. This folder also constitutes the functionality of the `geh_stream` [wheel](https://pythonwheels.com/) package.

## Unit test with Pytest

[Pytest](https://pytest.org/) is used for unit testing.

Unit tests can be debugged or run in the `Testing` tab in the left menu (using Python Test Explorer for Visual Studio Code).
You can debug or run either all tests, tests in a pytest-file or a single pytest using the `Debug` or `Run` icon for each element.

### Testing PySpark using fixture factories

It is quite cumbersome to unit test PySpark with data frames with a large number of columns.

In order to do this various concepts have been invented. In general you should start searching in the `conftest.py` files.

One of the probably hardest concepts to understand from reading code is the fixture factory concept. Basically it has the following form:

```python
@pytest.fixture(scope="session")
def enriched_data_factory(dependency1, dependency2):
    def factory(col1="default value",
                col2="default value",
                ...):
        result = {calculate from default values and dependencies}
        return result
    return factory
```

Then a test can depend on this fixture without any other transient dependency like especially the `SparkSession`.

By providing default values for columns also allow tests to have to specify values they care about.

Test example:

```python
def test_enricher_adds_metering_point_type(enriched_data):
    assert has_column(enriched_data, "md.meteringPointType")
```

## Package Wheel

Pipelines create the wheel and deploy it where needed.

Note that pipelines create the file `./VERSION`, which is needed by `setup.py`.
When running manual you must create the file yourself.

### Usage python package geh_stream

#### Install on your environment

`python setup.py install`

#### Create wheel

`python setup.py sdist bdist_wheel`

#### Run tests based on local changes

`python -m pytest tests`

#### Run tests based on installed package

`pytest tests`

## Test coverage

Test coverage can be calculated by executing the script `create_coverage_report.sh`. This generates the HTML report `htmlcov/index.html`.

## Attach vs code debugger to pytest

Running the `debugz.sh` script in 'source/streaming' allows you to debug the pytests with VS code.
In your `launch.json` file add the following configuration:

```json

 {
            "name": "Python: Attach container",
            "type": "python",
            "request": "attach",
            "port": 3000,
            "host": "localhost"
          }

```

You can now launch your [VS code debugger](https://code.visualstudio.com/docs/editor/debugging#_launch-configurations) with the "Python: Attach container" configuration.
