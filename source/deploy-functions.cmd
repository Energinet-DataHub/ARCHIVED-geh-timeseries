@echo off

echo =======================================================================================
echo Deploy Azure functions to your private sandbox
echo .
echo Assuming: Domain=tseries, Environment=s
echo *** Make sure that you created all local.settings.json settings files ***
echo *** Deployments are executed in separate windows in order to execute in parallel ***
echo =======================================================================================

setlocal

set /p organization=Enter organization used with Terraform (perhaps your initials or team name?):
set /p environment=Enter environment used with Terraform (perhaps u002 or s?):
set /p doBuild=Build solution ([y]/n)?
set /p deployMessageReceiver=Deploy message receiver ([y]/n)?
set /p deployIntegrationEventListener=Deploy integration event listener ([y]/n)?

IF /I not "%doBuild%" == "n" (
    rem Clean is necessary if e.g. a function project name has changed because otherwise both assemblies will be picked up by deployment
    dotnet clean GreenEnergyHub.TimeSeries\GreenEnergyHub.TimeSeries.sln -c Release
    dotnet build GreenEnergyHub.TimeSeries\GreenEnergyHub.TimeSeries.sln -c Release
    dotnet clean GreenEnergyHub.TimeSeries.Integration\GreenEnergyHub.TimeSeries.Integration.sln -c Release
    dotnet build GreenEnergyHub.TimeSeries.Integration\GreenEnergyHub.TimeSeries.Integration.sln -c Release
)

rem All (but the last) deployments are opened in separate windows in order to execute in parallel

IF /I not "%deployMessageReceiver%" == "n" (
    pushd GreenEnergyHub.TimeSeries\source\GreenEnergyHub.TimeSeries.MessageReceiver\bin\Release\net5.0
    start "Deploy: Message Receiver" cmd /c "func azure functionapp publish azfun-message-receiver-tseries-%organization%-%environment% & pause"
    popd
)

IF /I not "%deployIntegrationEventListener%" == "n" (
    pushd GreenEnergyHub.TimeSeries.Integration\source\GreenEnergyHub.TimeSeries.Integration.IntegrationEventListener\bin\Release\net5.0
    start "Deploy: Integration Event Listener" cmd /c "func azure functionapp publish azfun-integration-event-listener-tseries-%organization%-%environment% & pause"
    popd
)

endlocal
