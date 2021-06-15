@echo off

echo =======================================================================================
echo Deploy Azure functions to your private sandbox
echo .
echo Assuming: Domain=tseries, Environment=s
echo *** Make sure that you created all local.settings.json settings files ***
echo *** Deployments are executed in separate windows in order to execute in parallel ***
echo =======================================================================================

setlocal

set /p organization=Enter organization used with Terraform (perhaps your initials?): 
set /p doBuild=Build solution ([y]/n)?
set /p deployMessageReceiver=Deploy message receiver ([y]/n)?

IF /I not "%doBuild%" == "n" (
    rem Clean is necessary if e.g. a function project name has changed because otherwise both assemblies will be picked up by deployment
    dotnet clean GreenEnergyHub.TimeSeries.sln -c Release
    dotnet build GreenEnergyHub.TimeSeries.sln -c Release
)

rem All (but the last) deployments are opened in separate windows in order to execute in parallel

IF /I not "%deployMessageReceiver%" == "n" (
    pushd source\GreenEnergyHub.TimeSeries.MessageReceiver\bin\Release\netcoreapp3.1
    start "Deploy: Message Receiver" cmd /c "func azure functionapp publish azfun-message-receiver-tseries-%organization%-s & pause"
    popd
)

endlocal
