@if "%SCM_TRACE_LEVEL%" NEQ "4" @echo off

:: Prerequisites
:: -------------

:: Setup 
:: -----

setlocal enabledelayedexpansion

IF NOT DEFINED SOLUTION_NAME (
	SET SOLUTION_NAME=Dockyard.sln
)

IF NOT DEFINED DEPLOYMENT_SOURCE (
  SET DEPLOYMENT_SOURCE=%~dp0%.
)

IF NOT DEFINED DEPLOYMENT_TEMP (
  SET DEPLOYMENT_TEMP=%temp%\___deployTemp%random%
  SET CLEAN_LOCAL_DEPLOYMENT_TEMP=true
)

IF NOT DEFINED MSBUILD_PATH (
  SET MSBUILD_PATH=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe
)

IF NOT DEFINED NUNIT_RUNNERS_VERSION (
  Set NUNIT_RUNNERS_VERSION=2.6.3
)

IF NOT DEFINED NUNIT_RUNNERS (
  SET NUNIT_RUNNERS=NUnit.Runners.%NUNIT_RUNNERS_VERSION%\tools\nunit-console.exe
)

IF NOT DEFINED NUGET_EXE (
	SET NUGET_EXE=.nuget/nuget.exe
)

IF NOT DEFINED IN_PLACE_DEPLOYMENT (
	SET IN_PLACE_DEPLOYMENT=1
) 

set USERPROFILE=C:\Users\%USERNAME%
set APPDATA=%USERPROFILE%\AppData\

::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
:: Building
:: ----------

:: 1. Clean workspace
echo Cleaning workspace..
call :ExecuteCmd "git" clean -fdx

:: 2. NuGet Restore
echo Restoring nuget packages..
call :ExecuteCmd "%NUGET_EXE%" restore Dockyard.sln

:: 3. Build to the temporary path
IF /I "%IN_PLACE_DEPLOYMENT%" NEQ "1" (
  echo Building application to temp folder
  call :ExecuteCmd "%MSBUILD_PATH%" "%DEPLOYMENT_SOURCE%\Web.csproj" /nologo /verbosity:m /t:Build /p:WarningLevel=0 /t:pipelinePreDeployCopyAllFilesToOneFolder /p:_PackageTempDir="%DEPLOYMENT_TEMP%";AutoParameterizationWebConfigConnectionStrings=false;Configuration=Release /p:SolutionDir="%DEPLOYMENT_SOURCE%\.\\" %SCM_BUILD_ARGS%
  IF !ERRORLEVEL! NEQ 0 goto error
  call :ExecuteCmd "%MSBUILD_PATH%" "%DEPLOYMENT_SOURCE%\Tests\DockyardTest\DockyardTest.csproj" /nologo /verbosity:m /t:Build /p:WarningLevel=0 /p:_PackageTempDir="%DEPLOYMENT_TEMP%";AutoParameterizationWebConfigConnectionStrings=false;Configuration=Release /p:SolutionDir="%DEPLOYMENT_SOURCE%\.\\" %SCM_BUILD_ARGS%
  IF !ERRORLEVEL! NEQ 0 goto error


) ELSE (
  echo Building application in place
  call :ExecuteCmd "%MSBUILD_PATH%" "%DEPLOYMENT_SOURCE%\Web.csproj" /nologo /verbosity:m /t:Build /p:WarningLevel=0 /p:AutoParameterizationWebConfigConnectionStrings=false;Configuration=Release /p:SolutionDir="%DEPLOYMENT_SOURCE%\.\\" %SCM_BUILD_ARGS%
  IF !ERRORLEVEL! NEQ 0 goto error
  call :ExecuteCmd "%MSBUILD_PATH%" "%DEPLOYMENT_SOURCE%\Tests\DockyardTest\DockyardTest.csproj" /nologo /verbosity:m /t:Build /p:WarningLevel=0 /p:AutoParameterizationWebConfigConnectionStrings=false;Configuration=Release /p:SolutionDir="%DEPLOYMENT_SOURCE%\.\\" %SCM_BUILD_ARGS%
  IF !ERRORLEVEL! NEQ 0 goto error

)

:: 5.
echo Running tests
call :ExecuteCmd "%NUGET_EXE%" install NUnit.Runners -Version %NUNIT_RUNNERS_VERSION%
IF !ERRORLEVEL! NEQ 0 goto error
call :ExecuteCmd "%NUNIT_RUNNERS%" -labels "%DEPLOYMENT_SOURCE%\Tests\DockyardTest\bin\Release\DockyardTest.dll"
IF !ERRORLEVEL! NEQ 0 goto error


goto end

:: Execute command routine that will echo out when error
:ExecuteCmd
setlocal
set _CMD_=%*
call %_CMD_%
if "%ERRORLEVEL%" NEQ "0" echo Failed exitCode=%ERRORLEVEL%, command=%_CMD_%
exit /b %ERRORLEVEL%

:error
endlocal
echo An error has occurred during web site deployment.
call :exitSetErrorLevel
call :exitFromFunction 2>nul

:exitSetErrorLevel
exit /b 1

:exitFromFunction
()

:end
endlocal
echo Finished successfully.
