# Databricks

[![Code style: black](https://img.shields.io/badge/code%20style-black-000000.svg)](https://github.com/psf/black)

## Table of content

* [Getting started with Databricks development in Time Series](#getting-started-with-databrick-development-in-time-series)
* [Running Tests](#running-tests)
* [Debugging Tests](#debugging-tests)
* [Styling and Formatting](#styling-and-formatting)
* [Test Python code in CI pipeline](#test-python-code-in-ci-pipeline)

## Getting started with Databricks development in Time Series

### Install necessary tools needed for development

* #### [Docker](https://www.docker.com/get-started)

    * use WSL 2, you will get a prompt with a guide after installing docker

* #### [Visual Studio Code](https://code.visualstudio.com/#alt-downloads) (system installer)

    * Extension called ***Remote - Containers***

### Get workspace ready for development

* Open ***geh-timeseries*** folder in Visual Studio Code

* Select ***Remote Explorer*** in the left toolbar

* Click on the ***plus icon*** in the top of the panel to the right of ***Containers*** and select ***Open Current Folder in Container***

* Wait for the container to build (*This will take a few minutes the first time*) and then you are ready to go

## Running Tests

* To run all test you will need to execute the following command in the workspace terminal

    ```text
    pytest
    ```

* For more verbose output use

    ```text
    pytest -vv -s
    ```

* To run tests in a specific folder simply navigate to the folder in the terminal and use the same command as above

* To run tests in a specific file navigate to the folder where the file is located in the terminal and execute the following command

    ```text
    pytest file-name.py
    ```

* You can also run a specific test in the file by executing the following command

    ```text
    pytest file-name.py::function-name
    ```

## Debugging Tests

Use the [Python Test Explorer for Visual Studio Code](https://marketplace.visualstudio.com/items?itemName=LittleFoxTeam.vscode-python-test-adapter) extension in VSCode. It is automatically installed in the container (see [`.devcontainer/devcontainer.json`](https://github.com/Energinet-DataHub/geh-timeseries/blob/main/.devcontainer/devcontainer.json)).

### Alternative Debug Approach

This is a less simple and intuitive way of debugging,
but may serve as an alternative in case of problems with the recommended way of debugging.

* To debug tests you need to execute the following command

    Using debugz.sh with the following command

    ````text
    sh debugz.sh
    ````

    Or using command inside debugz.sh

    ```text
    python -m ptvsd --host 0.0.0.0 --port 3000 --wait -m pytest -v
    ```

* Create a ***launch.json*** file in the ***Run and Debug*** panel and add the following

    ```json
    {
        "name": "Python: Attach container",
        "type": "python",
        "request": "attach",
        "port": 3000,
        "host": "localhost"
    }
    ```

* Start debugging on the ***Python: Attach container*** in the ***Run and Debug*** panel

## Styling and Formatting

We try to follow [PEP8](https://peps.python.org/pep-0008/) as much as possible, we do this by using [Flake8](https://flake8.pycqa.org/en/latest/) and [Black](https://black.readthedocs.io/en/stable/)
The following Flake8 codes are ignored:

* Module imported but unused ([F401](https://www.flake8rules.com/rules/F401.html))
* Module level import not at top of file ([E402](https://www.flake8rules.com/rules/E402.html))
* Whitespace before ':' ([E203](https://www.flake8rules.com/rules/E203.html)) (*Needed for black you work well with Flake8, see documentation [here](https://github.com/psf/black/blob/main/docs/guides/using_black_with_other_tools.md#flake8)*)
* Line too long (82 &gt; 79 characters) ([E501](https://www.flake8rules.com/rules/E501.html)) (*Only ignored in CI step*)
* Line break occurred before a binary operator ([W503](https://www.flake8rules.com/rules/W503.html)) (*Black formatting does not follow this rule*)

Links to files containing  Flake8 ignore [`tox.ini`](../../tox.ini) and [`ci.yml`](../../.github/workflows/ci.yml)

We are using standard [Black code style](https://github.com/psf/black/blob/main/docs/the_black_code_style/current_style.md#the-black-code-style).

## Test Python code in CI pipeline

### Building and publishing a Docker image for testing

In the CI pipeline, the tests are executed towards a Docker image, which is described in the a [Dockerfile](../../.docker/Dockerfile).

A new Docker image is build and published using the [Docker CD-pipeline](../../.github/workflows/cd-docker-test-image.yml), meaning that a new Docker image is only published, when changes are made to the files described in the `paths`-sections of the workflow.

If a pull request triggers a new Docker image to be published, a new version of the Docker image is published on each commit. The Docker images published when a pull request is open, are considered `pre-releases`. A `pre-release`-image is assigned a tag with the following format: `pre-release-pr<PR-number>`, e.g. `pre-release-pr311`. When the pull request has been merged, the `Docker CD-pipeline` is run again, and a new `latest` version is published.

### Running the tests using a published Docker image

<!-- markdown-link-check-disable -->
The default Docker image used for testing is the newest version of the "latest"-tagged [databricks-unit-test](https://github.com/orgs/Energinet-DataHub/packages?repo_name=geh-timeseries)-image stored in [GitHub packages](https://docs.github.com/en/packages/learn-github-packages/introduction-to-github-packages), which is a container registry.
<!-- markdown-link-check-enable-->

In a pull request, it is possible to change the version of the Docker image used for running the tests. For example, if a pull request changes the Dockerfile, it might be relevant to run the test base towards the new Docker image. To change the version of the Docker image used, change the `image`-reference in the [docker-compose.yml](../../.devcontainer/docker-compose.yml)-file to e.g. `ghcr.io/energinet-datahub/geh-timeseries/databricks-unit-test:pre-release-pr311`.
