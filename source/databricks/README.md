# Getting started with Databricks development in TimeSeries
## Install necessary tools needed for development

- ### [Docker](https://www.docker.com/get-started)
  - use wsl 2, you will get a prompt with a guide after installing docker
- ### [Visual Studio Code](https://code.visualstudio.com/#alt-downloads) (system installer)
    - Extention called ***Remote - Containers***

## Get workspace ready for development
- Open ***geh-timeseries*** folder in Visual Studio Code
- Select ***Remote Explorer*** in the left toolbar
- Click on the ***plus icon*** in the top of the panel to the right of ***Containers*** and select ***Open Current Folder in Container***
- Wait for the container to build (*This will take a few minuts the first time*) and then you are ready to go

## Running Tests
- To run all test you will need to execute the following command in the workspace terminal
    ```
    pytest
    ```
- For more information on the tests use
    ```
    pytest -vv -s
    ```
- To run tests in a specific folder simply navigate to the folder in the terminal and use the samme command as above
- To run tests in a specific file navigate to the folder where the file is located in the terminal and execute the following command
    ```
    pytest file-name.py -vv -s
    ```
- You can also run a specific test in the file by executing the following command
    ```
    pytest file-name.py::function-name -vv -s
    ```